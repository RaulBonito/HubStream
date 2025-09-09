namespace HubStream.Shared.Kernel
{
    public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
    {
        public readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        protected AggregateRoot(TId id) : base(id){}

        protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
