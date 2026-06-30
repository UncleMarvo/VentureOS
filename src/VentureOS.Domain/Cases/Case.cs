using VentureOS.Domain.Cases.Events;
using VentureOS.Domain.Common;
using VentureOS.Domain.Observations;
using VentureOS.Domain.Evidence;
using VentureOS.Domain.Hypotheses;
using VentureOS.Domain.Assumptions;
using VentureOS.Domain.Challenges;
using VentureOS.Domain.Decisions;
using VentureOS.Domain.Lessons;

namespace VentureOS.Domain.Cases;

public sealed class Case : AggregateRoot
{
    private readonly List<Observation> _observations = [];
    private readonly List<Evidence.Evidence> _evidence = [];
    private readonly List<Hypothesis> _hypotheses = [];
    private readonly List<Assumption> _assumptions = [];
    private readonly List<Challenge> _challenges = [];
    private readonly List<Decision> _decisions = [];
    private readonly List<Lesson> _lessons = [];

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

    private Case(
        Guid id,
        string title,
        string mission,
        CaseStatus status,
        DateTime createdAtUtc,
        DateTime updatedAtUtc) : base(id)
    {
        Title = title;
        Mission = mission;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public string Title { get; private set; }

    public string Mission { get; private set; }

    public CaseStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<Observation> Observations => _observations.AsReadOnly();

    public IReadOnlyCollection<Evidence.Evidence> Evidence => _evidence.AsReadOnly();

    public IReadOnlyCollection<Hypothesis> Hypotheses => _hypotheses.AsReadOnly();

    public IReadOnlyCollection<Assumption> Assumptions => _assumptions.AsReadOnly();

    public IReadOnlyCollection<Challenge> Challenges => _challenges.AsReadOnly();

    public IReadOnlyCollection<Decision> Decisions => _decisions.AsReadOnly();

    public IReadOnlyCollection<Lesson> Lessons => _lessons.AsReadOnly();

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

    public static Case Rehydrate(CaseRehydrationState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var ventureCase = new Case(
            state.Id,
            state.Title,
            state.Mission,
            state.Status,
            state.CreatedAtUtc,
            state.UpdatedAtUtc);

        ventureCase._observations.AddRange(
            state.Observations.Select(observation =>
                new Observation(
                    observation.Id,
                    observation.CaseId,
                    observation.ObservationText,
                    observation.Summary,
                    observation.SourceReference,
                    observation.ObservationSource,
                    observation.Confidence,
                    observation.CreatedAtUtc)));

        ventureCase._evidence.AddRange(
            state.Evidence.Select(evidence =>
                new Evidence.Evidence(
                    evidence.Id,
                    evidence.CaseId,
                    evidence.Summary,
                    evidence.Interpretation,
                    evidence.Direction,
                    evidence.ObservationIds,
                    evidence.CreatedAtUtc)));

        ventureCase._assumptions.AddRange(
            state.Assumptions.Select(assumption =>
                new Assumption(
                    assumption.Id,
                    assumption.CaseId,
                    assumption.Statement,
                    assumption.Rationale,
                    assumption.Confidence,
                    assumption.CreatedAtUtc)));

        ventureCase._hypotheses.AddRange(
            state.Hypotheses.Select(hypothesis =>
                new Hypothesis(
                    hypothesis.Id,
                    hypothesis.CaseId,
                    hypothesis.Statement,
                    hypothesis.Reasoning,
                    hypothesis.ExpectedOutcome,
                    hypothesis.SuccessCriteria,
                    hypothesis.Confidence,
                    hypothesis.EvidenceIds,
                    hypothesis.AssumptionIds,
                    hypothesis.CreatedAtUtc)));

        ventureCase._challenges.AddRange(
            state.Challenges.Select(challenge =>
                new Challenge(
                    challenge.Id,
                    challenge.CaseId,
                    challenge.Target,
                    challenge.TargetId,
                    challenge.Statement,
                    challenge.Reasoning,
                    challenge.Confidence,
                    challenge.CreatedAtUtc)));

        ventureCase._decisions.AddRange(
            state.Decisions.Select(decision =>
                new Decision(
                    decision.Id,
                    decision.CaseId,
                    decision.Question,
                    decision.Outcome,
                    decision.Rationale,
                    decision.ExpectedOutcome,
                    decision.Confidence,
                    decision.EvidenceIds,
                    decision.AssumptionIds,
                    decision.HypothesisIds,
                    decision.ChallengeIds,
                    decision.CreatedAtUtc)));

        ventureCase._lessons.AddRange(
            state.Lessons.Select(lesson =>
                new Lesson(
                    lesson.Id,
                    lesson.CaseId,
                    lesson.Summary,
                    lesson.Detail,
                    lesson.Confidence,
                    lesson.DecisionIds,
                    lesson.CreatedAtUtc)));

        return ventureCase;
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

    // ============================
    // OBSERVATIONS
    // ============================
    public Result AddObservation(ObservationDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (Status == CaseStatus.Archived)
        {
            return Result.Failure("Cannot add observations to an archived case.");
        }

        if (string.IsNullOrWhiteSpace(draft.ObservationText))
        {
            return Result.Failure("Observation text is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.Summary))
        {
            return Result.Failure("Observation summary is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.SourceReference))
        {
            return Result.Failure("Observation source is required.");
        }

        var observation = new Observation(
            Guid.NewGuid(),
            Id,
            draft.ObservationText.Trim(),
            draft.Summary.Trim(),
            draft.SourceReference.Trim(),
            draft.ObservationSource,
            draft.Confidence,
            DateTime.UtcNow);

        _observations.Add(observation);
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new ObservationAddedEvent(Id, observation.Id, observation.Summary));

        return Result.Success();
    }

    // ============================
    // EVIDENCE
    // ============================
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

    // ============================
    // HYPOTHESIS
    // ============================
    public Result CreateHypothesis(HypothesisDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (Status == CaseStatus.Archived)
        {
            return Result.Failure("Cannot create hypotheses for an archived case.");
        }

        if (string.IsNullOrWhiteSpace(draft.Statement))
        {
            return Result.Failure("Hypothesis statement is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.Reasoning))
        {
            return Result.Failure("Hypothesis reasoning is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.ExpectedOutcome))
        {
            return Result.Failure("Hypothesis expected outcome is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.SuccessCriteria))
        {
            return Result.Failure("Hypothesis success criteria is required.");
        }

        if (draft.EvidenceIds.Count == 0)
        {
            return Result.Failure("Hypothesis must reference at least one piece of evidence.");
        }

        if (draft.AssumptionIds.Count == 0)
        {
            return Result.Failure("Hypothesis must reference at least one assumption.");
        }

        var knownEvidenceIds = _evidence.Select(evidence => evidence.Id).ToHashSet();

        var unknownEvidenceIds = draft.EvidenceIds
            .Where(evidenceId => !knownEvidenceIds.Contains(evidenceId))
            .ToArray();

        if (unknownEvidenceIds.Length > 0)
        {
            return Result.Failure("Hypothesis cannot reference evidence that does not belong to the case.");
        }

        var knownAssumptionIds = _assumptions.Select(assumption => assumption.Id).ToHashSet();

        var unknownAssumptionIds = draft.AssumptionIds
            .Where(assumptionId => !knownAssumptionIds.Contains(assumptionId))
            .ToArray();

        if (unknownAssumptionIds.Length > 0)
        {
            return Result.Failure("Hypothesis cannot reference assumptions that do not belong to the case.");
        }

        var hypothesis = new Hypothesis(
            Guid.NewGuid(),
            Id,
            draft.Statement.Trim(),
            draft.Reasoning.Trim(),
            draft.ExpectedOutcome.Trim(),
            draft.SuccessCriteria.Trim(),
            draft.Confidence,
            draft.EvidenceIds.Distinct().ToArray(),
            draft.AssumptionIds.Distinct().ToArray(),
            DateTime.UtcNow);

        _hypotheses.Add(hypothesis);
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new HypothesisCreatedEvent(Id, hypothesis.Id, hypothesis.Statement));

        return Result.Success();
    }

    // ============================
    // ASSUMPTIONS
    // ============================
    public Result CreateAssumption(AssumptionDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (Status == CaseStatus.Archived)
        {
            return Result.Failure("Cannot create assumptions for an archived case.");
        }

        if (string.IsNullOrWhiteSpace(draft.Statement))
        {
            return Result.Failure("Assumption statement is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.Rationale))
        {
            return Result.Failure("Assumption rationale is required.");
        }

        var assumption = new Assumption(
            Guid.NewGuid(),
            Id,
            draft.Statement.Trim(),
            draft.Rationale.Trim(),
            draft.Confidence,
            DateTime.UtcNow);

        _assumptions.Add(assumption);
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new AssumptionCreatedEvent(Id, assumption.Id, assumption.Statement));

        return Result.Success();
    }

    // ============================
    // CHALLENGES
    // ============================
    public Result RaiseChallenge(ChallengeDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (Status == CaseStatus.Archived)
        {
            return Result.Failure("Cannot raise challenges for an archived case.");
        }

        if (draft.TargetId == Guid.Empty)
        {
            return Result.Failure("Challenge target ID is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.Statement))
        {
            return Result.Failure("Challenge statement is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.Reasoning))
        {
            return Result.Failure("Challenge reasoning is required.");
        }

        if (!TargetExists(draft.Target, draft.TargetId))
        {
            return Result.Failure("Challenge target does not belong to the case.");
        }

        var challenge = new Challenge(
            Guid.NewGuid(),
            Id,
            draft.Target,
            draft.TargetId,
            draft.Statement.Trim(),
            draft.Reasoning.Trim(),
            draft.Confidence,
            DateTime.UtcNow);

        _challenges.Add(challenge);
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new ChallengeRaisedEvent(
            Id,
            challenge.Id,
            challenge.Target,
            challenge.TargetId,
            challenge.Statement));

        return Result.Success();
    }

    private bool TargetExists(ChallengeTarget target, Guid targetId)
    {
        return target switch
        {
            ChallengeTarget.Evidence => _evidence.Any(evidence => evidence.Id == targetId),
            ChallengeTarget.Assumption => _assumptions.Any(assumption => assumption.Id == targetId),
            ChallengeTarget.Hypothesis => _hypotheses.Any(hypothesis => hypothesis.Id == targetId),
            ChallengeTarget.Decision => false,
            _ => false
        };
    }

    // ============================
    // DECISIONS
    // ============================
    public Result RecordDecision(DecisionDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (Status == CaseStatus.Archived)
        {
            return Result.Failure("Cannot record decisions for an archived case.");
        }

        if (string.IsNullOrWhiteSpace(draft.Question))
        {
            return Result.Failure("Decision question is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.Rationale))
        {
            return Result.Failure("Decision rationale is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.ExpectedOutcome))
        {
            return Result.Failure("Decision expected outcome is required.");
        }

        if (draft.EvidenceIds.Count == 0)
        {
            return Result.Failure("Decision must reference at least one piece of evidence.");
        }

        if (draft.HypothesisIds.Count == 0)
        {
            return Result.Failure("Decision must reference at least one hypothesis.");
        }

        if (!AllEvidenceBelongsToCase(draft.EvidenceIds))
        {
            return Result.Failure("Decision cannot reference evidence that does not belong to the case.");
        }

        if (!AllAssumptionsBelongToCase(draft.AssumptionIds))
        {
            return Result.Failure("Decision cannot reference assumptions that do not belong to the case.");
        }

        if (!AllHypothesesBelongToCase(draft.HypothesisIds))
        {
            return Result.Failure("Decision cannot reference hypotheses that do not belong to the case.");
        }

        if (!AllChallengesBelongToCase(draft.ChallengeIds))
        {
            return Result.Failure("Decision cannot reference challenges that do not belong to the case.");
        }

        var decision = new Decision(
            Guid.NewGuid(),
            Id,
            draft.Question.Trim(),
            draft.Outcome,
            draft.Rationale.Trim(),
            draft.ExpectedOutcome.Trim(),
            draft.Confidence,
            draft.EvidenceIds.Distinct().ToArray(),
            draft.AssumptionIds.Distinct().ToArray(),
            draft.HypothesisIds.Distinct().ToArray(),
            draft.ChallengeIds.Distinct().ToArray(),
            DateTime.UtcNow);

        _decisions.Add(decision);
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new DecisionRecordedEvent(
            Id,
            decision.Id,
            decision.Outcome,
            decision.Question));

        return Result.Success();
    }

    private bool AllEvidenceBelongsToCase(IReadOnlyCollection<Guid> evidenceIds)
    {
        var knownEvidenceIds = _evidence.Select(evidence => evidence.Id).ToHashSet();
        return evidenceIds.All(knownEvidenceIds.Contains);
    }

    private bool AllAssumptionsBelongToCase(IReadOnlyCollection<Guid> assumptionIds)
    {
        var knownAssumptionIds = _assumptions.Select(assumption => assumption.Id).ToHashSet();
        return assumptionIds.All(knownAssumptionIds.Contains);
    }

    private bool AllHypothesesBelongToCase(IReadOnlyCollection<Guid> hypothesisIds)
    {
        var knownHypothesisIds = _hypotheses.Select(hypothesis => hypothesis.Id).ToHashSet();
        return hypothesisIds.All(knownHypothesisIds.Contains);
    }

    private bool AllChallengesBelongToCase(IReadOnlyCollection<Guid> challengeIds)
    {
        var knownChallengeIds = _challenges.Select(challenge => challenge.Id).ToHashSet();
        return challengeIds.All(knownChallengeIds.Contains);
    }

    // ============================
    // LESSONS
    // ============================
    public Result RecordLesson(LessonDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (Status == CaseStatus.Archived)
        {
            return Result.Failure("Cannot record lessons for an archived case.");
        }

        if (string.IsNullOrWhiteSpace(draft.Summary))
        {
            return Result.Failure("Lesson summary is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.Detail))
        {
            return Result.Failure("Lesson detail is required.");
        }

        if (draft.DecisionIds.Count == 0)
        {
            return Result.Failure("Lesson must reference at least one decision.");
        }

        if (!AllDecisionsBelongToCase(draft.DecisionIds))
        {
            return Result.Failure("Lesson cannot reference decisions that do not belong to the case.");
        }

        var lesson = new Lesson(
            Guid.NewGuid(),
            Id,
            draft.Summary.Trim(),
            draft.Detail.Trim(),
            draft.Confidence,
            draft.DecisionIds.Distinct().ToArray(),
            DateTime.UtcNow);

        _lessons.Add(lesson);
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new LessonRecordedEvent(
            Id,
            lesson.Id,
            lesson.Summary));

        return Result.Success();
    }

    private bool AllDecisionsBelongToCase(IReadOnlyCollection<Guid> decisionIds)
    {
        var knownDecisionIds = _decisions.Select(decision => decision.Id).ToHashSet();
        return decisionIds.All(knownDecisionIds.Contains);
    }
}