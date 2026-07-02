using VentureOS.Domain.Cases;

namespace VentureOS.Application.RedTeam.RedTeamQuality;

public static class RedTeamQualityChecker
{
    public static IReadOnlyList<RedTeamQualityIssueDto> Check(
        RedTeamReviewResultDto review,
        Case ventureCase)
    {
        var issues = new List<RedTeamQualityIssueDto>();

        var knownEvidenceIds = ventureCase.Evidence.Select(evidence => evidence.Id).ToHashSet();
        var knownAssumptionIds = ventureCase.Assumptions.Select(assumption => assumption.Id).ToHashSet();
        var knownHypothesisIds = ventureCase.Hypotheses.Select(hypothesis => hypothesis.Id).ToHashSet();
        var knownOpportunityIds = ventureCase.Opportunities.Select(opportunity => opportunity.Id).ToHashSet();

        for (var i = 0; i < review.Challenges.Count; i++)
        {
            var challenge = review.Challenges[i];

            CheckRequiredText(
                challenge.Statement,
                $"challenges[{i}].statement",
                "Challenge statement is required.",
                issues);

            CheckRequiredText(
                challenge.Reasoning,
                $"challenges[{i}].reasoning",
                "Challenge reasoning is required.",
                issues);

            if (string.IsNullOrWhiteSpace(challenge.TargetType))
            {
                issues.Add(new RedTeamQualityIssueDto(
                    RedTeamQualitySeverity.Error,
                    RedTeamQualityCode.InvalidChallengeTargetType,
                    $"challenges[{i}].targetType",
                    "Challenge target type is required."));

                continue;
            }

            var knownTargetIds = challenge.TargetType switch
            {
                "Evidence" => knownEvidenceIds,
                "Assumption" => knownAssumptionIds,
                "Hypothesis" => knownHypothesisIds,
                "Opportunity" => knownOpportunityIds,
                _ => null
            };

            if (knownTargetIds is null)
            {
                issues.Add(new RedTeamQualityIssueDto(
                    RedTeamQualitySeverity.Error,
                    RedTeamQualityCode.InvalidChallengeTargetType,
                    $"challenges[{i}].targetType",
                    $"Target type '{challenge.TargetType}' is not valid."));

                continue;
            }

            if (challenge.TargetId == Guid.Empty)
            {
                issues.Add(new RedTeamQualityIssueDto(
                    RedTeamQualitySeverity.Error,
                    RedTeamQualityCode.InvalidChallengeTarget,
                    $"challenges[{i}].targetId",
                    "Challenge target ID is required."));

                continue;
            }

            if (!knownTargetIds.Contains(challenge.TargetId))
            {
                issues.Add(new RedTeamQualityIssueDto(
                    RedTeamQualitySeverity.Error,
                    RedTeamQualityCode.InvalidChallengeTarget,
                    $"challenges[{i}].targetId",
                    $"Target ID {challenge.TargetId} does not exist for target type '{challenge.TargetType}'."));
            }
        }

        return issues;
    }

    private static void CheckRequiredText(
        string? text,
        string path,
        string message,
        List<RedTeamQualityIssueDto> issues)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        issues.Add(new RedTeamQualityIssueDto(
            RedTeamQualitySeverity.Error,
            RedTeamQualityCode.RequiredFieldMissing,
            path,
            message));
    }
}
