using System.Text.RegularExpressions;
using VentureOS.Application.Research.ResearchCase;

namespace VentureOS.Application.Research.ResearchQuality;

public static class ResearchQualityChecker
{
    private static readonly Regex NumberPattern = new(
        @"(\d+(\.\d+)?\s?%|\$\s?\d+|\d+[\–-]\d+|\d+)",
        RegexOptions.Compiled);

    public static IReadOnlyList<ResearchQualityIssueDto> Check(
        ResearchPackageDto package)
    {
        var issues = new List<ResearchQualityIssueDto>();

        CheckObservations(package, issues);
        CheckEvidence(package, issues);
        CheckAssumptions(package, issues);
        CheckOpportunities(package, issues);
        CheckHypotheses(package, issues);
        CheckChallenges(package, issues);

        return issues;
    }

    private static void CheckObservations(
        ResearchPackageDto package,
        List<ResearchQualityIssueDto> issues)
    {
        for (var i = 0; i < package.Observations.Count; i++)
        {
            var observation = package.Observations[i];

            CheckUnsupportedNumber(
                observation.ObservationText,
                observation.SourceReference,
                $"observations[{i}].observationText",
                issues);

            CheckUnsupportedNumber(
                observation.Summary,
                observation.SourceReference,
                $"observations[{i}].summary",
                issues);
        }
    }

    private static void CheckEvidence(
        ResearchPackageDto package,
        List<ResearchQualityIssueDto> issues)
    {
        for (var i = 0; i < package.Evidence.Count; i++)
        {
            var evidence = package.Evidence[i];

            CheckUnsupportedNumber(
                evidence.Summary,
                null,
                $"evidence[{i}].summary",
                issues);

            CheckUnsupportedNumber(
                evidence.Interpretation,
                null,
                $"evidence[{i}].interpretation",
                issues);

            foreach (var observationIndex in evidence.ObservationIndexes)
            {
                if (observationIndex < 0 || observationIndex >= package.Observations.Count)
                {
                    issues.Add(new ResearchQualityIssueDto(
                        ResearchQualitySeverity.Error,
                        ResearchQualityCode.InvalidObservationIndex,
                        $"evidence[{i}].observationIndexes",
                        $"Observation index {observationIndex} does not exist."));
                }
            }
        }
    }

    private static void CheckAssumptions(
        ResearchPackageDto package,
        List<ResearchQualityIssueDto> issues)
    {
        for (var i = 0; i < package.Assumptions.Count; i++)
        {
            var assumption = package.Assumptions[i];

            CheckUnsupportedNumber(
                assumption.Statement,
                null,
                $"assumptions[{i}].statement",
                issues);

            CheckUnsupportedNumber(
                assumption.Rationale,
                null,
                $"assumptions[{i}].rationale",
                issues);
        }
    }

    private static void CheckOpportunities(
        ResearchPackageDto package,
        List<ResearchQualityIssueDto> issues)
    {
        for (var i = 0; i < package.Opportunities.Count; i++)
        {
            var opportunity = package.Opportunities[i];

            CheckUnsupportedNumber(opportunity.Statement, null, $"opportunities[{i}].statement", issues);
            CheckUnsupportedNumber(opportunity.CustomerValue, null, $"opportunities[{i}].customerValue", issues);
            CheckUnsupportedNumber(opportunity.CommercialValue, null, $"opportunities[{i}].commercialValue", issues);
            CheckUnsupportedNumber(opportunity.Differentiation, null, $"opportunities[{i}].differentiation", issues);
            CheckUnsupportedNumber(opportunity.Timing, null, $"opportunities[{i}].timing", issues);

            foreach (var evidenceIndex in opportunity.EvidenceIndexes)
            {
                if (evidenceIndex < 0 || evidenceIndex >= package.Evidence.Count)
                {
                    issues.Add(new ResearchQualityIssueDto(
                        ResearchQualitySeverity.Error,
                        ResearchQualityCode.InvalidEvidenceIndex,
                        $"opportunities[{i}].evidenceIndexes",
                        $"Evidence index {evidenceIndex} does not exist."));
                }
            }

            foreach (var assumptionIndex in opportunity.AssumptionIndexes)
            {
                if (assumptionIndex < 0 || assumptionIndex >= package.Assumptions.Count)
                {
                    issues.Add(new ResearchQualityIssueDto(
                        ResearchQualitySeverity.Error,
                        ResearchQualityCode.InvalidAssumptionIndex,
                        $"opportunities[{i}].assumptionIndexes",
                        $"Assumption index {assumptionIndex} does not exist."));
                }
            }
        }
    }

    private static void CheckHypotheses(
        ResearchPackageDto package,
        List<ResearchQualityIssueDto> issues)
    {
        for (var i = 0; i < package.Hypotheses.Count; i++)
        {
            var hypothesis = package.Hypotheses[i];

            CheckUnsupportedNumber(hypothesis.Statement, null, $"hypotheses[{i}].statement", issues);
            CheckUnsupportedNumber(hypothesis.Reasoning, null, $"hypotheses[{i}].reasoning", issues);
            CheckUnsupportedNumber(hypothesis.ExpectedOutcome, null, $"hypotheses[{i}].expectedOutcome", issues);
            CheckUnsupportedNumber(hypothesis.SuccessCriteria, null, $"hypotheses[{i}].successCriteria", issues);

            foreach (var evidenceIndex in hypothesis.EvidenceIndexes)
            {
                if (evidenceIndex < 0 || evidenceIndex >= package.Evidence.Count)
                {
                    issues.Add(new ResearchQualityIssueDto(
                        ResearchQualitySeverity.Error,
                        ResearchQualityCode.InvalidEvidenceIndex,
                        $"hypotheses[{i}].evidenceIndexes",
                        $"Evidence index {evidenceIndex} does not exist."));
                }
            }

            foreach (var assumptionIndex in hypothesis.AssumptionIndexes)
            {
                if (assumptionIndex < 0 || assumptionIndex >= package.Assumptions.Count)
                {
                    issues.Add(new ResearchQualityIssueDto(
                        ResearchQualitySeverity.Error,
                        ResearchQualityCode.InvalidAssumptionIndex,
                        $"hypotheses[{i}].assumptionIndexes",
                        $"Assumption index {assumptionIndex} does not exist."));
                }
            }
        }
    }

    private static void CheckChallenges(
        ResearchPackageDto package,
        List<ResearchQualityIssueDto> issues)
    {
        for (var i = 0; i < package.Challenges.Count; i++)
        {
            var challenge = package.Challenges[i];

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

            CheckUnsupportedNumber(
                challenge.Statement,
                null,
                $"challenges[{i}].statement",
                issues);

            CheckUnsupportedNumber(
                challenge.Reasoning,
                null,
                $"challenges[{i}].reasoning",
                issues);

            if (string.IsNullOrWhiteSpace(challenge.TargetType))
            {
                issues.Add(new ResearchQualityIssueDto(
                    ResearchQualitySeverity.Error,
                    ResearchQualityCode.InvalidChallengeTargetType,
                    $"challenges[{i}].targetType",
                    "Challenge target type is required."));

                continue;
            }

            var maxIndex = challenge.TargetType switch
            {
                "Evidence" => package.Evidence.Count,
                "Assumption" => package.Assumptions.Count,
                "Hypothesis" => package.Hypotheses.Count,
                "Opportunity" => package.Opportunities.Count,
                _ => -1
            };

            if (maxIndex == -1)
            {
                issues.Add(new ResearchQualityIssueDto(
                    ResearchQualitySeverity.Error,
                    ResearchQualityCode.InvalidChallengeTargetType,
                    $"challenges[{i}].targetType",
                    $"Target type '{challenge.TargetType}' is not valid."));
            }
            else if (challenge.TargetIndex < 0 || challenge.TargetIndex >= maxIndex)
            {
                issues.Add(new ResearchQualityIssueDto(
                    ResearchQualitySeverity.Error,
                    ResearchQualityCode.InvalidChallengeTargetIndex,
                    $"challenges[{i}].targetIndex",
                    $"Target index {challenge.TargetIndex} does not exist for target type '{challenge.TargetType}'."));
            }
        }
    }

    private static void CheckUnsupportedNumber(
        string? text,
        string? sourceReference,
        string path,
        List<ResearchQualityIssueDto> issues)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (!NumberPattern.IsMatch(text))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(sourceReference) &&
            !sourceReference.Equals("AI-generated research hypothesis", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        issues.Add(new ResearchQualityIssueDto(
            ResearchQualitySeverity.Warning,
            ResearchQualityCode.UnsupportedNumericalClaim,
            path,
            "Numerical claim appears without explicit provenance."));
    }

    private static void CheckRequiredText(
        string? text,
        string path,
        string message,
        List<ResearchQualityIssueDto> issues)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        issues.Add(new ResearchQualityIssueDto(
            ResearchQualitySeverity.Error,
            ResearchQualityCode.RequiredFieldMissing,
            path,
            message));
    }
}
