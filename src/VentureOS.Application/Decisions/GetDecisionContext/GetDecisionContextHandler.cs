using VentureOS.Application.Cases;
using VentureOS.Domain.Common;

namespace VentureOS.Application.Decisions.GetDecisionContext;

public sealed class GetDecisionContextHandler
{
    private readonly ICaseRepository _caseRepository;

    public GetDecisionContextHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<Result<DecisionContextDto>> HandleAsync(
        GetDecisionContextQuery query,
        CancellationToken cancellationToken = default)
    {
        var ventureCase = await _caseRepository.GetByIdAsync(
            query.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<DecisionContextDto>.Failure("Case not found.");
        }

        var decision = ventureCase.Decisions
            .FirstOrDefault(decision => decision.Id == query.DecisionId);

        if (decision is null)
        {
            return Result<DecisionContextDto>.Failure("Decision not found.");
        }

        // =======================
        // EVIDENCE
        // =======================
        var evidence = ventureCase.Evidence
            .Where(evidence => decision.EvidenceIds.Contains(evidence.Id))
            .Select(evidence =>
                new DecisionContextEvidenceDto(
                    evidence.Id,
                    evidence.Summary,
                    evidence.Interpretation,
                    evidence.CreatedAtUtc))
            .ToList();

        // =======================
        // ASSUMPTION
        // =======================
        var assumptions = ventureCase.Assumptions
            .Where(assumption => decision.AssumptionIds.Contains(assumption.Id))
            .Select(assumption =>
                new DecisionContextAssumptionDto(
                    assumption.Id,
                    assumption.Statement,
                    assumption.Rationale,
                    assumption.Confidence,
                    assumption.CreatedAtUtc))
            .ToList();

        // =======================
        // HYPOTHESIS
        // =======================
        var hypotheses = ventureCase.Hypotheses
            .Where(hypothesis => decision.HypothesisIds.Contains(hypothesis.Id))
            .Select(hypothesis =>
                new DecisionContextHypothesisDto(
                    hypothesis.Id,
                    hypothesis.Statement,
                    hypothesis.Reasoning,
                    hypothesis.Confidence,
                    hypothesis.CreatedAtUtc))
            .ToList();

        // =======================
        // CHALLENGE
        // =======================
        var challenges = ventureCase.Challenges
            .Where(challenge => decision.ChallengeIds.Contains(challenge.Id))
            .Select(challenge =>
                new DecisionContextChallengeDto(
                    challenge.Id,
                    challenge.Statement,
                    challenge.Reasoning,
                    challenge.Confidence,
                    challenge.CreatedAtUtc))
            .ToList();

        var dto = new DecisionContextDto(
            new DecisionContextDecisionDto(
                decision.Id,
                decision.Question,
                decision.Outcome,
                decision.Rationale,
                decision.ExpectedOutcome,
                decision.Confidence,
                decision.CreatedAtUtc),
            evidence,
            assumptions,
            hypotheses,
            challenges);

        return Result<DecisionContextDto>.Success(dto);
    }
}
