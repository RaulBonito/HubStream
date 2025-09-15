using HubStream.Shared.Kernel.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Users.Events
{
    public class PhoneNumberChangedEvent : IDomainEvent
    {
        public Identifier UserId { get; }
        public string NewPhoneNumber { get; }
        public PhoneNumberChangedEvent(Identifier userId, string newPhoneNumber)
        {
            UserId = userId;
            NewPhoneNumber = newPhoneNumber;
        }
    }
}
