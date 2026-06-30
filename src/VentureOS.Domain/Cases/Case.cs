using VentureOS.Domain.Cases.Events;
using VentureOS.Domain.Common;

namespace VentureOS.Domain.Cases;

public sealed class Case : AggregateRoot
{
    private Case(
        Guid id,
        string title,
        string mission,
        DateTime createdAtUtc) : base(id)
    {
        Title = title;
        Mission = mission;
        Status = CaseStatus.Draft;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;

        AddDomainEvent(new CaseCreatedEvent(Id, Title));
    }

    public string Title { get; private set; }

    public string Mission { get; private set; }

    public CaseStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static Result<Case> Create(string title, string mission)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result<Case>.Failure("Case title is required.");
        }

        if (string.IsNullOrWhiteSpace(mission))
        {
            return Result<Case>.Failure("Case mission is required.");
        }

        var ventureCase = new Case(
            Guid.NewGuid(),
            title.Trim(),
            mission.Trim(),
            DateTime.UtcNow);

        return Result<Case>.Success(ventureCase);
    }

    public Result Activate()
    {
        if (Status == CaseStatus.Archived)
        {
            return Result.Failure("Archived cases cannot be activated.");
        }

        Status = CaseStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Archive()
    {
        if (Status == CaseStatus.Archived)
        {
            return Result.Failure("Case is already archived.");
        }

        Status = CaseStatus.Archived;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }
}