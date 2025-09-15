using HubStream.Shared.Kernel.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Scheduling.Events
{
    public sealed class LiveStreamScheduled : IDomainEvent
    {
        public Identifier LiveStreamId { get; }
        public Identifier streamerId { get; }
        public string Title { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }

        public LiveStreamScheduled(Identifier liveStreamId, Identifier streamerId, string title, DateTime start, TimeSpan duration)
        {
            if (liveStreamId == default)
                throw new ArgumentException("El identificador del LiveStream no puede ser vacío.", nameof(liveStreamId));

            if (streamerId == default)
                throw new ArgumentException("El identificador del Mentor no puede ser vacío.", nameof(streamerId));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("El título no puede ser vacío.", nameof(title));

            LiveStreamId = liveStreamId;
            streamerId = streamerId;
            Title = title;
            Start = start;
            Duration = duration;
        }
    }
}
