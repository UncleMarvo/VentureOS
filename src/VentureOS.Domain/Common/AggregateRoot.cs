namespace VentureOS.Domain.Common;

public abstract class AggregateRoot : Entity
{
    private readonly List<object> _domainEvents = [];

    protected AggregateRoot(Guid id) : base(id)
    {
    }

    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(object domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}