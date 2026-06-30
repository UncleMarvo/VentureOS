using VentureOS.Domain.Challenges;
using VentureOS.Domain.Common;

namespace VentureOS.Application.Cases.RaiseChallenge;

public sealed class RaiseChallengeHandler
{
    private readonly ICaseRepository _caseRepository;

    public RaiseChallengeHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
    }

    public async Task<Result<RaiseChallengeResult>> HandleAsync(
        RaiseChallengeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ventureCase = await _caseRepository.GetByIdAsync(
            command.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<RaiseChallengeResult>.Failure("Case not found.");
        }

        var draft = new ChallengeDraft(
            command.Target,
            command.TargetId,
            command.Statement,
            command.Reasoning,
            command.Confidence);

        var raiseChallengeResult = ventureCase.RaiseChallenge(draft);

        if (raiseChallengeResult.IsFailure)
        {
            return Result<RaiseChallengeResult>.Failure(
                raiseChallengeResult.Error ?? "Challenge could not be raised.");
        }

        var challenge = ventureCase.Challenges
            .OrderByDescending(item => item.CreatedAtUtc)
            .First();

        await _caseRepository.UpdateAsync(ventureCase, cancellationToken);

        return Result<RaiseChallengeResult>.Success(
            new RaiseChallengeResult(
                ventureCase.Id,
                challenge.Id,
                challenge.Target,
                challenge.TargetId,
                challenge.Statement));
    }
}