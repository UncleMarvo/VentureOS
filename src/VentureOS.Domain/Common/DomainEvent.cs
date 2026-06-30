namespace VentureOS.Domain.Common;

public abstract record DomainEvent
{
    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; }

    public DateTime OccurredAtUtc { get; }
}