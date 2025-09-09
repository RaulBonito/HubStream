using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Shared.Kernel
{
    public readonly record struct Identifier(Guid Id)
    {
        public static Identifier New() => new(Guid.NewGuid());
    }
}
