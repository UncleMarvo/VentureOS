namespace VentureOS.Domain.Common;

public abstract class AggregateRoot : Entity
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected AggregateRoot(Guid id) : base(id)
    {
    }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}