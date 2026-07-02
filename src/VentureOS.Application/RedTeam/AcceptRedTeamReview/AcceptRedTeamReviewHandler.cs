using VentureOS.Application.Cases.RaiseChallenge;
using VentureOS.Domain.Challenges;
using VentureOS.Domain.Common;

namespace VentureOS.Application.RedTeam.AcceptRedTeamReview;

public sealed class AcceptRedTeamReviewHandler
{
    private readonly RaiseChallengeHandler _raiseChallengeHandler;

    public AcceptRedTeamReviewHandler(RaiseChallengeHandler raiseChallengeHandler)
    {
        _raiseChallengeHandler = raiseChallengeHandler;
    }

    public async Task<Result<AcceptedRedTeamReviewDto>> HandleAsync(
        AcceptRedTeamReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        var challengesCreated = 0;

        foreach (var proposedChallenge in command.Review.Challenges)
        {
            var target = proposedChallenge.TargetType switch
            {
                "Evidence" => ChallengeTarget.Evidence,
                "Assumption" => ChallengeTarget.Assumption,
                "Hypothesis" => ChallengeTarget.Hypothesis,
                "Opportunity" => ChallengeTarget.Opportunity,
                _ => throw new InvalidOperationException(
                    $"Unsupported challenge target type: {proposedChallenge.TargetType}")
            };

            var result = await _raiseChallengeHandler.HandleAsync(
                new RaiseChallengeCommand(
                    command.CaseId,
                    target,
                    proposedChallenge.TargetId,
                    proposedChallenge.Statement,
                    proposedChallenge.Reasoning,
                    Confidence.FromPercentage(proposedChallenge.Confidence)),
                cancellationToken);

            if (result.IsFailure)
            {
                return Result<AcceptedRedTeamReviewDto>.Failure(
                    result.Error ?? "Failed to create challenge.");
            }

            challengesCreated++;
        }

        return Result<AcceptedRedTeamReviewDto>.Success(
            new AcceptedRedTeamReviewDto(command.CaseId, challengesCreated));
    }
}
