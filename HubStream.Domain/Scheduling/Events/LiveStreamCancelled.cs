using HubStream.Shared.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Scheduling.Events
{
    public sealed class LiveStreamCancelled : IDomainEvent
    {
        public Identifier LiveStreamId { get; }
        public Identifier StreamerId { get; }
        public DateTime CancelledAt { get; }
        public LiveStreamCancelled(Identifier streamId, Identifier streamerId)
        {
            LiveStreamId = streamId;
            StreamerId = streamerId;
            CancelledAt = DateTime.UtcNow;
        }
    }
}
