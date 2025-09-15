using HubStream.Domain.Users.Events;
using HubStream.Shared.Kernel.Domain;
using Microsoft.AspNetCore.Identity;

namespace HubStream.Domain.Users.Entities
{
    public class ApplicationUser : IdentityUser<Identifier>
    {
        public string FullName { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        #region DomainSetup
        private readonly List<IDomainEvent> _domainEvents;
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public ApplicationUser() : base()
        {
            _domainEvents = new List<IDomainEvent>();
        }
        protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        public void ClearDomainEvents() => _domainEvents.Clear();
        #endregion

        // Phone number changed event
        public void SetPhoneNumber(string phoneNumber)
        {
            if (PhoneNumber != phoneNumber)
            {
                PhoneNumber = phoneNumber;
                AddDomainEvent(new PhoneNumberChangedEvent(Id, phoneNumber));
            }
        }



    }
}
