using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Shared.Kernel
{
    public abstract class Entity<TId> where TId : notnull   
    {
        public TId Id { get; protected set; } = default!;
        protected Entity(TId id)
        {
            Id = id;
        }
    }
}
