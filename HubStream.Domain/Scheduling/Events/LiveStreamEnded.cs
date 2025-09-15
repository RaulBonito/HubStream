using HubStream.Shared.Kernel.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Scheduling.Events
{
    public sealed class LiveStreamEnded : IDomainEvent
    {
        public Identifier LiveStreamId { get; }
        public Identifier StreamerId { get; }
        public LiveStreamEnded(Identifier streamId, Identifier streamerId)
        {
            LiveStreamId = streamId;
            StreamerId = streamerId;
        }
    }
}
