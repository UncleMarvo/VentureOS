using VentureOS.Application.Cases;
using VentureOS.Application.RedTeam.RedTeamQuality;
using VentureOS.Domain.Common;

namespace VentureOS.Application.RedTeam.RedTeamCase;

public sealed class RedTeamCaseHandler
{
    private readonly ICaseRepository _caseRepository;
    private readonly IRedTeamReviewService _redTeamReviewService;

    public RedTeamCaseHandler(
        ICaseRepository caseRepository,
        IRedTeamReviewService redTeamReviewService)
    {
        _caseRepository = caseRepository;
        _redTeamReviewService = redTeamReviewService;
    }

    public async Task<Result<RedTeamCaseResultDto>> HandleAsync(
        RedTeamCaseQuery query,
        CancellationToken cancellationToken = default)
    {
        var ventureCase = await _caseRepository.GetByIdAsync(
            query.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<RedTeamCaseResultDto>.Failure("Case not found.");
        }

        var review = await _redTeamReviewService.ReviewCaseAsync(
            ventureCase,
            cancellationToken);

        var qualityIssues = RedTeamQualityChecker.Check(review, ventureCase);

        return Result<RedTeamCaseResultDto>.Success(
            new RedTeamCaseResultDto(review, qualityIssues));
    }
}
