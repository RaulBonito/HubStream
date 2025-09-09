using HubStream.Domain.Scheduling.ValueObjects;
using HubStream.Shared.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Scheduling.Events
{
    public sealed class ReviewSubmitted : IDomainEvent
    {
        public Identifier ReviewId { get; }
        public Identifier StreamerId { get; }
        public int Rating { get; }

        public ReviewSubmitted(Identifier reviewId, Identifier streamerId, int rating)
        {
            ReviewId = reviewId;
            StreamerId = streamerId;
            Rating = rating;
        }
    }
}
