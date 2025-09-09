using HubStream.Domain.Users.ValueObjects;
using HubStream.Shared.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Users.Events
{
    public record UserPromotedToStreamer(Identifier UserId) : IDomainEvent;
}
