using VentureOS.Domain.Common;

namespace VentureOS.Application.Cases.GetCase;

public sealed class GetCaseHandler
{
    private readonly ICaseRepository _repository;

    public GetCaseHandler(ICaseRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetCaseResult>> HandleAsync(
        GetCaseQuery query,
        CancellationToken cancellationToken = default)
    {
        var ventureCase = await _repository.GetByIdAsync(
            query.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<GetCaseResult>.Failure("Case not found.");
        }

        return Result<GetCaseResult>.Success(
            new GetCaseResult(
                ventureCase.Id,
                ventureCase.Title,
                ventureCase.Mission,
                ventureCase.Status,
                ventureCase.CreatedAtUtc,
                ventureCase.UpdatedAtUtc,
                ventureCase.Observations.Count,
                ventureCase.Evidence.Count,
                ventureCase.Assumptions.Count,
                ventureCase.Hypotheses.Count,
                ventureCase.Challenges.Count,
                ventureCase.Decisions.Count,
                ventureCase.Lessons.Count));
    }
}