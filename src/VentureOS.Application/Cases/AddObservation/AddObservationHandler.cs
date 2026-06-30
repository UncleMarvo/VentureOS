using VentureOS.Domain.Common;
using VentureOS.Domain.Observations;

namespace VentureOS.Application.Cases.AddObservation;

public sealed class AddObservationHandler
{
    private readonly ICaseRepository _caseRepository;

    public AddObservationHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
    }

    public async Task<Result<AddObservationResult>> HandleAsync(
        AddObservationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ventureCase = await _caseRepository.GetByIdAsync(
            command.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<AddObservationResult>.Failure("Case not found.");
        }

        var draft = new ObservationDraft(
            command.ObservationText,
            command.Summary,
            command.SourceReference,
            command.ObservationSource,
            command.Confidence);

        var addObservationResult = ventureCase.AddObservation(draft);

        if (addObservationResult.IsFailure)
        {
            return Result<AddObservationResult>.Failure(
                addObservationResult.Error ?? "Observation could not be added.");
        }

        var observation = ventureCase.Observations
            .OrderByDescending(item => item.CreatedAtUtc)
            .First();

        await _caseRepository.UpdateAsync(ventureCase, cancellationToken);

        return Result<AddObservationResult>.Success(
            new AddObservationResult(
                ventureCase.Id,
                observation.Id,
                observation.Summary));
    }
}