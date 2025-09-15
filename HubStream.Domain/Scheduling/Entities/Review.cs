using HubStream.Domain.Scheduling.Events;
using HubStream.Domain.Scheduling.ValueObjects;
using HubStream.Shared.Kernel.Common;
using HubStream.Shared.Kernel.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Scheduling.Entities
{
    public class Review : AggregateRoot<Identifier>
    {
        public Identifier LiveStreamId { get; private set; }
        public Identifier UserId { get; private set; }
        public Identifier StreamerId { get; private set; }
        public Rating Stars { get; private set; }
        public string Comment { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Review(Identifier id, Identifier liveStreamId, Identifier userId, Identifier streamerId, Rating stars, string comment) : base(id)
        {
            LiveStreamId = liveStreamId;
            UserId = userId;
            StreamerId = streamerId;
            Stars = stars;
            Comment = comment;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<Review> Submit(Identifier liveStreamId, Identifier userId, Identifier streamerId, int stars, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return Result<Review>.Failure(new Error("Review.Submit.EmptyComment", "The comment cannot be empty."));
            }
            var ratingResult = Rating.Create(stars);

            if (ratingResult.IsFailure)
            {
                return Result<Review>.Failure(ratingResult.Error);
            }

            var review = new Review(Identifier.New(), liveStreamId, userId, streamerId, ratingResult.Value, comment);

            review.AddDomainEvent(new ReviewSubmitted(review.Id, review.StreamerId, review.Stars.Value));

            return Result<Review>.Success(review);
        }
    }
}
