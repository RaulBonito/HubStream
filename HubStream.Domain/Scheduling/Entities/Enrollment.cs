using HubStream.Domain.Users.ValueObjects;
using HubStream.Shared.Kernel.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Scheduling.Entities
{
    public class Enrollment : Entity<Identifier>
    {
        public Identifier LiveStreamId { get; private set; }
        public Identifier EnrolledUserId { get; private set; }
        public DateTime EnrolledAtUtc { get; private set; }

        public Enrollment(Identifier id, Identifier liveStreamId, Identifier enrolledUserId) : base(id)
        {
            LiveStreamId = liveStreamId;
            EnrolledUserId = enrolledUserId;
            EnrolledAtUtc = DateTime.UtcNow;
        }
    }
}
