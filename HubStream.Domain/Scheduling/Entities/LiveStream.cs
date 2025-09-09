using HubStream.Domain.Currencies.Entities;
using HubStream.Domain.Scheduling.Enums;
using HubStream.Domain.Scheduling.Events;
using HubStream.Domain.Scheduling.ValueObjects;
using HubStream.Domain.Users.ValueObjects;
using HubStream.Shared.Kernel;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;


namespace HubStream.Domain.Scheduling.Entities
{
    public class LiveStream : AggregateRoot<Identifier>, IAuditableEntity
    {
        private readonly List<Enrollment> _enrollments = new();

        public Identifier StreamerId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string ThumbnailUrl { get; private set; }
        public Money Price { get; private set; }
        public StreamTimeSlot TimeSlot { get; private set; }
        public StreamCapacity Capacity { get; private set; }
        public LiveStreamStatus Status { get; private set; }
        public IReadOnlyCollection<Enrollment> Enrollments => _enrollments.AsReadOnly();

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        public LiveStream(Identifier id, Identifier streamerId, string title, string description, string thumbnailUrl, Money price, StreamTimeSlot timeSlot, StreamCapacity capacity) : base(id)
        {
            StreamerId = streamerId;
            Title = title;
            Description = description;
            ThumbnailUrl = thumbnailUrl;
            Price = price;
            TimeSlot = timeSlot;
            Capacity = capacity;
            Status = LiveStreamStatus.Draft;
        }

        public static Result<LiveStream> Schedule(Identifier streamerId, string title, string description, string thumbnailUrl, Money price, DateTime startDate, TimeSpan duration, int maxAttendees)
        {
            // Validaciones de negocio
            if (startDate < DateTime.UtcNow)
                return Result<LiveStream>.Failure(new Error("LiveStream.Schedule.DateInPast", "Cannot schedule a stream in the past."));

            var timeSlot = new StreamTimeSlot(startDate, duration);
            var capacity = new StreamCapacity(maxAttendees);

            var liveStream = new LiveStream(Identifier.New(), streamerId, title, description, thumbnailUrl, price, timeSlot, capacity);

            liveStream.Status = LiveStreamStatus.Scheduled;

            liveStream.AddDomainEvent(new LiveStreamScheduled(
                liveStream.Id,
                liveStream.StreamerId,
                liveStream.Title,
                liveStream.TimeSlot.Start,
                liveStream.TimeSlot.Duration));

            return Result<LiveStream>.Success(liveStream);
        }

        public Result EnrollUser(Identifier userId)
        {
            // Protección de Invariantes del Agregado
            if (Status != LiveStreamStatus.Scheduled)
                return Result.Failure(new Error("LiveStream.Enroll.NotScheduled", "Can only enroll in a scheduled stream."));

            if (_enrollments.Count >= Capacity.MaxParticipants)
                return Result.Failure(new Error("LiveStream.Enroll.Full", "The stream is full."));

            if (_enrollments.Any(e => e.EnrolledUserId == userId))
                return Result.Failure(new Error("LiveStream.Enroll.AlreadyEnrolled", "User is already enrolled."));

            var enrollment = new Enrollment(Identifier.New(), Id, userId);
            _enrollments.Add(enrollment);

            AddDomainEvent(new UserEnrolledToStream(Id, userId, DateTime.UtcNow));
            return Result.Success();
        }

        public Result Cancel()
        {
            if (Status != LiveStreamStatus.Scheduled)
                return Result.Failure(new Error("LiveStream.Cancel.NotScheduled", "Can only cancel a scheduled stream."));

            Status = LiveStreamStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new LiveStreamCancelled(Id, StreamerId));
            return Result.Success();
        }

        public Result Start()
        {
            if (Status != LiveStreamStatus.Scheduled)
                return Result.Failure(new Error("LiveStream.Start.NotScheduled", "Can only start a scheduled stream."));
            if (DateTime.UtcNow < TimeSlot.Start)
                return Result.Failure(new Error("LiveStream.Start.TooEarly", "Cannot start the stream before its scheduled time."));

            Status = LiveStreamStatus.Live;

            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new LiveStreamStarted(Id, StreamerId));

            return Result.Success();
        }

        public Result End()
        {
            if (Status != LiveStreamStatus.Live)
                return Result.Failure(new Error("LiveStream.End.NotLive", "Can only end a live stream."));
            Status = LiveStreamStatus.Ended;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new LiveStreamEnded(Id, StreamerId));

            return Result.Success();
        }
    }


}
