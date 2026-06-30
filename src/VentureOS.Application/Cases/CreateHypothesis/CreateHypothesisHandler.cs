using VentureOS.Domain.Common;
using VentureOS.Domain.Hypotheses;

namespace VentureOS.Application.Cases.CreateHypothesis;

public sealed class CreateHypothesisHandler
{
    private readonly ICaseRepository _caseRepository;

    public CreateHypothesisHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
    }

    public async Task<Result<CreateHypothesisResult>> HandleAsync(
        CreateHypothesisCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ventureCase = await _caseRepository.GetByIdAsync(
            command.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<CreateHypothesisResult>.Failure("Case not found.");
        }

        var draft = new HypothesisDraft(
            command.Statement,
            command.Reasoning,
            command.ExpectedOutcome,
            command.SuccessCriteria,
            command.Confidence,
            command.EvidenceIds,
            command.AssumptionIds);

        var createHypothesisResult = ventureCase.CreateHypothesis(draft);

        if (createHypothesisResult.IsFailure)
        {
            return Result<CreateHypothesisResult>.Failure(
                createHypothesisResult.Error ?? "Hypothesis could not be created.");
        }

        var hypothesis = ventureCase.Hypotheses
            .OrderByDescending(item => item.CreatedAtUtc)
            .First();

        await _caseRepository.UpdateAsync(ventureCase, cancellationToken);

        return Result<CreateHypothesisResult>.Success(
            new CreateHypothesisResult(
                ventureCase.Id,
                hypothesis.Id,
                hypothesis.Statement));
    }
}