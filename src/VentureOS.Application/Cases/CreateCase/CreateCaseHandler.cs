using VentureOS.Domain.Cases;
using VentureOS.Domain.Common;

namespace VentureOS.Application.Cases.CreateCase;

public sealed class CreateCaseHandler
{
    private readonly ICaseRepository _caseRepository;

    public CreateCaseHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
    }

    public async Task<Result<CreateCaseResult>> HandleAsync(
        CreateCaseCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var createResult = Case.Create(command.Title, command.Mission);

        if (createResult.IsFailure)
        {
            return Result<CreateCaseResult>.Failure(
                createResult.Error ?? "Case could not be created.");
        }

        var ventureCase = createResult.Value;

        await _caseRepository.AddAsync(ventureCase, cancellationToken);

        return Result<CreateCaseResult>.Success(
            new CreateCaseResult(
                ventureCase.Id,
                ventureCase.Title,
                ventureCase.Mission));
    }
}