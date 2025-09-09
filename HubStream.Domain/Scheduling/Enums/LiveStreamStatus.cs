using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Scheduling.Enums
{
    public enum LiveStreamStatus
    {
        Draft,
        Scheduled,
        Live,
        Ended,
        Cancelled,
        Postponed
    }
}
