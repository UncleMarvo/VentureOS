using VentureOS.Domain.Common;
using VentureOS.Domain.Decisions;

namespace VentureOS.Application.Cases.RecordDecision;

public sealed class RecordDecisionHandler
{
    private readonly ICaseRepository _caseRepository;

    public RecordDecisionHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
    }

    public async Task<Result<RecordDecisionResult>> HandleAsync(
        RecordDecisionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ventureCase = await _caseRepository.GetByIdAsync(
            command.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<RecordDecisionResult>.Failure("Case not found.");
        }

        var draft = new DecisionDraft(
            command.Question,
            command.Outcome,
            command.Rationale,
            command.ExpectedOutcome,
            command.Confidence,
            command.EvidenceIds,
            command.AssumptionIds,
            command.HypothesisIds,
            command.ChallengeIds);

        var recordDecisionResult = ventureCase.RecordDecision(draft);

        if (recordDecisionResult.IsFailure)
        {
            return Result<RecordDecisionResult>.Failure(
                recordDecisionResult.Error ?? "Decision could not be recorded.");
        }

        var decision = ventureCase.Decisions
            .OrderByDescending(item => item.CreatedAtUtc)
            .First();

        await _caseRepository.UpdateAsync(
            ventureCase,
            cancellationToken);

        return Result<RecordDecisionResult>.Success(
            new RecordDecisionResult(
                ventureCase.Id,
                decision.Id,
                decision.Outcome,
                decision.Question));
    }
}