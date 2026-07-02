using VentureOS.Domain.Cases;

namespace VentureOS.Application.RedTeam;

public interface IRedTeamReviewService
{
    Task<RedTeamReviewResultDto> ReviewCaseAsync(
        Case ventureCase,
        CancellationToken cancellationToken = default);
}
