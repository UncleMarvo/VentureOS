using VentureOS.Domain.Common;
using VentureOS.Domain.Evidence;

namespace VentureOS.Application.Cases.CreateEvidence;

public sealed class CreateEvidenceHandler
{
    private readonly ICaseRepository _caseRepository;

    public CreateEvidenceHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
    }

    public async Task<Result<CreateEvidenceResult>> HandleAsync(
        CreateEvidenceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ventureCase = await _caseRepository.GetByIdAsync(
            command.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<CreateEvidenceResult>.Failure("Case not found.");
        }

        var draft = new EvidenceDraft(
            command.Summary,
            command.Interpretation,
            command.Direction,
            command.ObservationIds);

        var createEvidenceResult = ventureCase.CreateEvidence(draft);

        if (createEvidenceResult.IsFailure)
        {
            return Result<CreateEvidenceResult>.Failure(
                createEvidenceResult.Error ?? "Evidence could not be created.");
        }

        var evidence = createEvidenceResult.Value;

        await _caseRepository.UpdateAsync(ventureCase, cancellationToken);

        return Result<CreateEvidenceResult>.Success(
            new CreateEvidenceResult(
                ventureCase.Id,
                evidence.Id,
                evidence.Summary,
                evidence.Direction));
    }
}