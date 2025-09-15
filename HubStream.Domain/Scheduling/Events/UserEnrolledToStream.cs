using HubStream.Shared.Kernel.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Scheduling.Events
{
    public sealed class UserEnrolledToStream : IDomainEvent
    {
        public Identifier EnrollmentId { get; }
        public Identifier LiveStreamId { get; }
        public DateTime EnrollmentDate { get; }

        public UserEnrolledToStream(Identifier enrollmentId, Identifier liveStreamId, DateTime enrollmentDate)
        {
            EnrollmentId = enrollmentId;
            LiveStreamId = liveStreamId;
            EnrollmentDate = enrollmentDate;
        }
    }
}
