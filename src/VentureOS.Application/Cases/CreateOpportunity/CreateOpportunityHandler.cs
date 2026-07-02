using VentureOS.Domain.Common;
using VentureOS.Domain.Opportunities;

namespace VentureOS.Application.Cases.CreateOpportunity;

public sealed class CreateOpportunityHandler
{
    private readonly ICaseRepository _caseRepository;

    public CreateOpportunityHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
    }

    public async Task<Result<CreateOpportunityResult>> HandleAsync(
        CreateOpportunityCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ventureCase = await _caseRepository.GetByIdAsync(
            command.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<CreateOpportunityResult>.Failure("Case not found.");
        }

        var draft = new OpportunityDraft(
            command.Statement,
            command.CustomerValue,
            command.CommercialValue,
            command.Differentiation,
            command.Timing,
            command.Confidence,
            command.EvidenceIds,
            command.AssumptionIds);

        var createOpportunityResult = ventureCase.CreateOpportunity(draft);

        if (createOpportunityResult.IsFailure)
        {
            return Result<CreateOpportunityResult>.Failure(
                createOpportunityResult.Error ?? "Opportunity could not be created.");
        }

        var opportunity = createOpportunityResult.Value;

        await _caseRepository.UpdateAsync(ventureCase, cancellationToken);

        return Result<CreateOpportunityResult>.Success(
            new CreateOpportunityResult(
                ventureCase.Id,
                opportunity.Id,
                opportunity.Statement));
    }
}
