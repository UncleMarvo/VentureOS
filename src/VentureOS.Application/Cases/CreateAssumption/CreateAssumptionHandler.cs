using VentureOS.Domain.Assumptions;
using VentureOS.Domain.Common;

namespace VentureOS.Application.Cases.CreateAssumption;

public sealed class CreateAssumptionHandler
{
    private readonly ICaseRepository _caseRepository;

    public CreateAssumptionHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
    }

    public async Task<Result<CreateAssumptionResult>> HandleAsync(
        CreateAssumptionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ventureCase = await _caseRepository.GetByIdAsync(
            command.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<CreateAssumptionResult>.Failure("Case not found.");
        }

        var draft = new AssumptionDraft(
            command.Statement,
            command.Rationale,
            command.Confidence);

        var createAssumptionResult = ventureCase.CreateAssumption(draft);

        if (createAssumptionResult.IsFailure)
        {
            return Result<CreateAssumptionResult>.Failure(
                createAssumptionResult.Error ?? "Assumption could not be created.");
        }

        var assumption = ventureCase.Assumptions
            .OrderByDescending(item => item.CreatedAtUtc)
            .First();

        await _caseRepository.UpdateAsync(ventureCase, cancellationToken);

        return Result<CreateAssumptionResult>.Success(
            new CreateAssumptionResult(
                ventureCase.Id,
                assumption.Id,
                assumption.Statement));
    }
}