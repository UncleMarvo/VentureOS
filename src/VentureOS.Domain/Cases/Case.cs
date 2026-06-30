using VentureOS.Domain.Cases.Events;
using VentureOS.Domain.Common;
using VentureOS.Domain.Observations;
using VentureOS.Domain.Evidence;

namespace VentureOS.Domain.Cases;

public sealed class Case : AggregateRoot
{
    private readonly List<Observation> _observations = [];
    private readonly List<Evidence.Evidence> _evidence = [];

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

    public IReadOnlyCollection<Observation> Observations => _observations.AsReadOnly();

    public IReadOnlyCollection<Evidence.Evidence> Evidence => _evidence.AsReadOnly();

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

    public Result AddObservation(ObservationDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (Status == CaseStatus.Archived)
        {
            return Result.Failure("Cannot add observations to an archived case.");
        }

        if (string.IsNullOrWhiteSpace(draft.Summary))
        {
            return Result.Failure("Observation summary is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.Source))
        {
            return Result.Failure("Observation source is required.");
        }

        var observation = new Observation(
            Guid.NewGuid(),
            Id,
            draft.Summary.Trim(),
            draft.Source.Trim(),
            DateTime.UtcNow);

        _observations.Add(observation);
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new ObservationAddedEvent(Id, observation.Id, observation.Summary));

        return Result.Success();
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

    public Result CreateEvidence(EvidenceDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (Status == CaseStatus.Archived)
        {
            return Result.Failure("Cannot create evidence for an archived case.");
        }

        if (string.IsNullOrWhiteSpace(draft.Summary))
        {
            return Result.Failure("Evidence summary is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.Interpretation))
        {
            return Result.Failure("Evidence interpretation is required.");
        }

        if (draft.ObservationIds.Count == 0)
        {
            return Result.Failure("Evidence must reference at least one observation.");
        }

        var knownObservationIds = _observations.Select(observation => observation.Id).ToHashSet();

        var unknownObservationIds = draft.ObservationIds
            .Where(observationId => !knownObservationIds.Contains(observationId))
            .ToArray();

        if (unknownObservationIds.Length > 0)
        {
            return Result.Failure("Evidence cannot reference observations that do not belong to the case.");
        }

        var evidence = new Evidence.Evidence(
            Guid.NewGuid(),
            Id,
            draft.Summary.Trim(),
            draft.Interpretation.Trim(),
            draft.Direction,
            draft.ObservationIds.Distinct().ToArray(),
            DateTime.UtcNow);

        _evidence.Add(evidence);
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new EvidenceCreatedEvent(Id, evidence.Id, evidence.Direction, evidence.Summary));

        return Result.Success();
    }
}