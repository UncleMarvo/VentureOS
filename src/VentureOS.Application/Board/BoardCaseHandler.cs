using VentureOS.Application.Cases;
using VentureOS.Domain.Common;

namespace VentureOS.Application.Board;

public sealed class BoardCaseHandler
{
    private readonly ICaseRepository _caseRepository;
    private readonly IBoardReviewService _boardReviewService;

    public BoardCaseHandler(
        ICaseRepository caseRepository,
        IBoardReviewService boardReviewService)
    {
        _caseRepository = caseRepository;
        _boardReviewService = boardReviewService;
    }

    public async Task<Result<BoardBriefingDto>> HandleAsync(
        BoardCaseQuery query,
        CancellationToken cancellationToken = default)
    {
        var ventureCase = await _caseRepository.GetByIdAsync(
            query.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<BoardBriefingDto>.Failure("Case not found.");
        }

        var dossier = BoardDossierAssembler.Assemble(
            ventureCase,
            query.ResearchQualityFindings,
            query.RedTeamQualityFindings);

        var narrative = await _boardReviewService.ReviewAsync(dossier, cancellationToken);

        return Result<BoardBriefingDto>.Success(new BoardBriefingDto(dossier, narrative));
    }
}
