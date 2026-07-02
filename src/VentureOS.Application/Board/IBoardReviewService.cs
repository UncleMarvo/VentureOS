namespace VentureOS.Application.Board;

public interface IBoardReviewService
{
    Task<BoardNarrativeDto> ReviewAsync(
        BoardDossierDto dossier,
        CancellationToken cancellationToken = default);
}
