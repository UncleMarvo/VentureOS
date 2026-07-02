using VentureOS.Application.RedTeam;
using VentureOS.Application.RedTeam.RedTeamQuality;
using VentureOS.Domain.Assumptions;
using VentureOS.Domain.Cases;
using VentureOS.Domain.Common;
using VentureOS.Domain.Evidence;
using VentureOS.Domain.Hypotheses;
using VentureOS.Domain.Observations;
using VentureOS.Domain.Opportunities;

namespace VentureOS.Application.Tests.RedTeam.RedTeamQuality;

public sealed class RedTeamQualityCheckerTests
{
    [Fact]
    public void Check_WithValidReview_ReturnsNoIssues()
    {
        var ventureCase = ValidCase();

        var issues = RedTeamQualityChecker.Check(ValidReview(ventureCase), ventureCase);

        Assert.Empty(issues);
    }

    [Fact]
    public void Check_WithChallengeBlankStatement_ReturnsRequiredFieldMissingError()
    {
        var ventureCase = ValidCase();
        var evidenceId = ventureCase.Evidence.Last().Id;

        var review = ValidReview(
            ventureCase,
            [new RedTeamProposedChallengeDto("", "Valid reasoning.", 50, "Evidence", evidenceId)]);

        var issues = RedTeamQualityChecker.Check(review, ventureCase);

        var issue = Assert.Single(issues);
        Assert.Equal(RedTeamQualitySeverity.Error, issue.Severity);
        Assert.Equal(RedTeamQualityCode.RequiredFieldMissing, issue.Code);
        Assert.Equal("challenges[0].statement", issue.Path);
    }

    [Fact]
    public void Check_WithChallengeBlankReasoning_ReturnsRequiredFieldMissingError()
    {
        var ventureCase = ValidCase();
        var evidenceId = ventureCase.Evidence.Last().Id;

        var review = ValidReview(
            ventureCase,
            [new RedTeamProposedChallengeDto("Valid statement.", "", 50, "Evidence", evidenceId)]);

        var issues = RedTeamQualityChecker.Check(review, ventureCase);

        var issue = Assert.Single(issues);
        Assert.Equal(RedTeamQualitySeverity.Error, issue.Severity);
        Assert.Equal(RedTeamQualityCode.RequiredFieldMissing, issue.Code);
        Assert.Equal("challenges[0].reasoning", issue.Path);
    }

    [Fact]
    public void Check_WithChallengeBlankTargetType_ReturnsExactlyOneInvalidChallengeTargetTypeError()
    {
        // A blank target type short-circuits (continue) before the target-id check runs,
        // so exactly one issue should be produced, not two.
        var ventureCase = ValidCase();

        var review = ValidReview(
            ventureCase,
            [new RedTeamProposedChallengeDto("Valid statement.", "Valid reasoning.", 50, "", Guid.Empty)]);

        var issues = RedTeamQualityChecker.Check(review, ventureCase);

        var issue = Assert.Single(issues);
        Assert.Equal(RedTeamQualitySeverity.Error, issue.Severity);
        Assert.Equal(RedTeamQualityCode.InvalidChallengeTargetType, issue.Code);
        Assert.Equal("challenges[0].targetType", issue.Path);
    }

    [Fact]
    public void Check_WithChallengeUnrecognizedTargetType_ReturnsInvalidChallengeTargetTypeError()
    {
        var ventureCase = ValidCase();
        var evidenceId = ventureCase.Evidence.Last().Id;

        var review = ValidReview(
            ventureCase,
            [new RedTeamProposedChallengeDto("Valid statement.", "Valid reasoning.", 50, "Bogus", evidenceId)]);

        var issues = RedTeamQualityChecker.Check(review, ventureCase);

        var issue = Assert.Single(issues);
        Assert.Equal(RedTeamQualitySeverity.Error, issue.Severity);
        Assert.Equal(RedTeamQualityCode.InvalidChallengeTargetType, issue.Code);
        Assert.Equal("challenges[0].targetType", issue.Path);
    }

    [Fact]
    public void Check_WithChallengeTargetTypeDecision_ReturnsInvalidChallengeTargetTypeError()
    {
        // Decision is a real ChallengeTarget elsewhere in the Domain, but Red Team must never be
        // able to target one (mirrors Case.TargetExists hard-coding Decision to false).
        var ventureCase = ValidCase();

        var review = ValidReview(
            ventureCase,
            [new RedTeamProposedChallengeDto("Valid statement.", "Valid reasoning.", 50, "Decision", Guid.NewGuid())]);

        var issues = RedTeamQualityChecker.Check(review, ventureCase);

        var issue = Assert.Single(issues);
        Assert.Equal(RedTeamQualitySeverity.Error, issue.Severity);
        Assert.Equal(RedTeamQualityCode.InvalidChallengeTargetType, issue.Code);
        Assert.Equal("challenges[0].targetType", issue.Path);
    }

    [Fact]
    public void Check_WithChallengeEmptyTargetId_ReturnsInvalidChallengeTargetError()
    {
        var ventureCase = ValidCase();

        var review = ValidReview(
            ventureCase,
            [new RedTeamProposedChallengeDto("Valid statement.", "Valid reasoning.", 50, "Evidence", Guid.Empty)]);

        var issues = RedTeamQualityChecker.Check(review, ventureCase);

        var issue = Assert.Single(issues);
        Assert.Equal(RedTeamQualitySeverity.Error, issue.Severity);
        Assert.Equal(RedTeamQualityCode.InvalidChallengeTarget, issue.Code);
        Assert.Equal("challenges[0].targetId", issue.Path);
    }

    [Theory]
    [InlineData("Evidence")]
    [InlineData("Assumption")]
    [InlineData("Hypothesis")]
    [InlineData("Opportunity")]
    public void Check_WithChallengeTargetIdNotInCase_ReturnsInvalidChallengeTargetError(string targetType)
    {
        var ventureCase = ValidCase();

        var review = ValidReview(
            ventureCase,
            [new RedTeamProposedChallengeDto("Valid statement.", "Valid reasoning.", 50, targetType, Guid.NewGuid())]);

        var issues = RedTeamQualityChecker.Check(review, ventureCase);

        var issue = Assert.Single(issues);
        Assert.Equal(RedTeamQualitySeverity.Error, issue.Severity);
        Assert.Equal(RedTeamQualityCode.InvalidChallengeTarget, issue.Code);
        Assert.Equal("challenges[0].targetId", issue.Path);
    }

    // ======================================
    // HELPERS
    // ======================================
    private static Case ValidCase()
    {
        var ventureCase = Case.Create("Valid case title.", "Valid case mission.").Value;

        ventureCase.AddObservation(
            new ObservationDraft(
                "Raw observation text.",
                "Valid observation summary.",
                "Manual research note",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        var observationId = ventureCase.Observations.Last().Id;

        ventureCase.CreateEvidence(
            new EvidenceDraft(
                "Valid evidence summary.",
                "Valid evidence interpretation.",
                EvidenceDirection.Supports,
                [observationId]));

        var evidenceId = ventureCase.Evidence.Last().Id;

        ventureCase.CreateAssumption(
            new AssumptionDraft(
                "Users are willing to pay for the proposed solution.",
                "The problem appears recurring and costly enough to justify paid automation.",
                Confidence.FromPercentage(55)));

        var assumptionId = ventureCase.Assumptions.Last().Id;

        ventureCase.CreateHypothesis(
            new HypothesisDraft(
                "Valid hypothesis statement.",
                "Valid hypothesis reasoning.",
                "Valid expected outcome.",
                "Valid success criteria.",
                Confidence.FromPercentage(60),
                [evidenceId],
                [assumptionId]));

        ventureCase.CreateOpportunity(
            new OpportunityDraft(
                "Valid opportunity statement.",
                "Valid customer value.",
                "Valid commercial value.",
                "Valid differentiation.",
                "Valid timing considerations.",
                Confidence.FromPercentage(55),
                [evidenceId],
                []));

        return ventureCase;
    }

    private static RedTeamReviewResultDto ValidReview(
        Case ventureCase,
        IReadOnlyList<RedTeamProposedChallengeDto>? challenges = null)
    {
        return new RedTeamReviewResultDto(
            ventureCase.Id,
            ventureCase.Mission,
            new RedTeamGenerationDto(
                "Ollama", "test-model", "Red Team Analyst", "1.0.0", "1.0.0",
                DateTime.UtcNow, TimeSpan.Zero, "Limitations."),
            challenges ?? [new RedTeamProposedChallengeDto(
                "Valid statement.", "Valid reasoning.", 50, "Evidence", ventureCase.Evidence.Last().Id)]);
    }
}
