using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Shared.Kernel
{
    public interface IAuditableEntity
    {
        public DateTime CreatedAt { get; }
        public DateTime? UpdatedAt { get; }
    }
}
