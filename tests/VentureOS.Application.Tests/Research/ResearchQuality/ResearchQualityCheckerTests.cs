using VentureOS.Application.Research.ResearchCase;
using VentureOS.Application.Research.ResearchQuality;

namespace VentureOS.Application.Tests.Research.ResearchQuality;

public sealed class ResearchQualityCheckerTests
{
    [Fact]
    public void Check_WithValidPackage_ReturnsNoIssues()
    {
        var issues = ResearchQualityChecker.Check(ValidPackage());

        Assert.Empty(issues);
    }

    // ======================================
    // UNSUPPORTED NUMERICAL CLAIMS
    // ======================================
    [Fact]
    public void Check_WithObservationNumberAndPlaceholderSource_ReturnsUnsupportedNumericalClaimWarning()
    {
        var package = ValidPackage(
            observations:
            [
                new ProposedObservationDto(
                    "We identified 15 competitors in the market.",
                    "Summary.",
                    "AI-generated research hypothesis",
                    50)
            ]);

        var issues = ResearchQualityChecker.Check(package);

        var issue = Assert.Single(issues);
        Assert.Equal(ResearchQualitySeverity.Warning, issue.Severity);
        Assert.Equal(ResearchQualityCode.UnsupportedNumericalClaim, issue.Code);
        Assert.Equal("observations[0].observationText", issue.Path);
    }

    [Fact]
    public void Check_WithObservationNumberAndRealSource_ReturnsNoIssue()
    {
        var package = ValidPackage(
            observations:
            [
                new ProposedObservationDto(
                    "We identified 15 competitors in the market.",
                    "Summary.",
                    "Companies House filing 2024",
                    50)
            ]);

        var issues = ResearchQualityChecker.Check(package);

        Assert.Empty(issues);
    }

    [Fact]
    public void Check_WithEvidenceContainingNumber_AlwaysReturnsWarning_RegardlessOfSourceReference()
    {
        // ProposedEvidenceDto has no SourceReference field at all, so unlike Observations,
        // there is no way to suppress this warning for Evidence today.
        var package = ValidPackage(
            evidence: [ValidEvidenceWithSummary("Market grew by 20% last year.", 0)]);

        var issues = ResearchQualityChecker.Check(package);

        var issue = Assert.Single(issues);
        Assert.Equal(ResearchQualitySeverity.Warning, issue.Severity);
        Assert.Equal(ResearchQualityCode.UnsupportedNumericalClaim, issue.Code);
        Assert.Equal("evidence[0].summary", issue.Path);
    }

    // ======================================
    // INDEX VALIDATION
    // ======================================
    [Fact]
    public void Check_WithEvidenceReferencingOutOfRangeObservationIndex_ReturnsInvalidObservationIndexError()
    {
        var package = ValidPackage(evidence: [ValidEvidence(5)]);

        var issues = ResearchQualityChecker.Check(package);

        var issue = Assert.Single(issues);
        Assert.Equal(ResearchQualitySeverity.Error, issue.Severity);
        Assert.Equal(ResearchQualityCode.InvalidObservationIndex, issue.Code);
        Assert.Equal("evidence[0].observationIndexes", issue.Path);
    }

    [Fact]
    public void Check_WithOpportunityReferencingOutOfRangeEvidenceIndex_ReturnsInvalidEvidenceIndexError()
    {
        var package = ValidPackage(
            opportunities: [ValidOpportunity([5], [])]);

        var issues = ResearchQualityChecker.Check(package);

        var issue = Assert.Single(issues);
        Assert.Equal(ResearchQualitySeverity.Error, issue.Severity);
        Assert.Equal(ResearchQualityCode.InvalidEvidenceIndex, issue.Code);
        Assert.Equal("opportunities[0].evidenceIndexes", issue.Path);
    }

    [Fact]
    public void Check_WithOpportunityReferencingOutOfRangeAssumptionIndex_ReturnsInvalidAssumptionIndexError()
    {
        var package = ValidPackage(
            opportunities: [ValidOpportunity([0], [5])]);

        var issues = ResearchQualityChecker.Check(package);

        var issue = Assert.Single(issues);
        Assert.Equal(ResearchQualitySeverity.Error, issue.Severity);
        Assert.Equal(ResearchQualityCode.InvalidAssumptionIndex, issue.Code);
        Assert.Equal("opportunities[0].assumptionIndexes", issue.Path);
    }

    [Fact]
    public void Check_WithHypothesisReferencingOutOfRangeEvidenceIndex_ReturnsInvalidEvidenceIndexError()
    {
        var package = ValidPackage(
            hypotheses: [ValidHypothesis([5], [0])]);

        var issues = ResearchQualityChecker.Check(package);

        var issue = Assert.Single(issues);
        Assert.Equal(ResearchQualitySeverity.Error, issue.Severity);
        Assert.Equal(ResearchQualityCode.InvalidEvidenceIndex, issue.Code);
        Assert.Equal("hypotheses[0].evidenceIndexes", issue.Path);
    }

    [Fact]
    public void Check_WithHypothesisReferencingOutOfRangeAssumptionIndex_ReturnsInvalidAssumptionIndexError()
    {
        var package = ValidPackage(
            hypotheses: [ValidHypothesis([0], [5])]);

        var issues = ResearchQualityChecker.Check(package);

        var issue = Assert.Single(issues);
        Assert.Equal(ResearchQualitySeverity.Error, issue.Severity);
        Assert.Equal(ResearchQualityCode.InvalidAssumptionIndex, issue.Code);
        Assert.Equal("hypotheses[0].assumptionIndexes", issue.Path);
    }

    // ======================================
    // CHALLENGE VALIDATION
    // ======================================
    [Fact]
    public void Check_WithChallengeBlankStatement_ReturnsRequiredFieldMissingError()
    {
        var package = ValidPackage(
            challenges: [new ProposedChallengeDto("", "Valid reasoning.", 50, "Hypothesis", 0)]);

        var issues = ResearchQualityChecker.Check(package);

        var issue = Assert.Single(issues);
        Assert.Equal(ResearchQualitySeverity.Error, issue.Severity);
        Assert.Equal(ResearchQualityCode.RequiredFieldMissing, issue.Code);
        Assert.Equal("challenges[0].statement", issue.Path);
    }

    [Fact]
    public void Check_WithChallengeBlankReasoning_ReturnsRequiredFieldMissingError()
    {
        var package = ValidPackage(
            challenges: [new ProposedChallengeDto("Valid statement.", "", 50, "Hypothesis", 0)]);

        var issues = ResearchQualityChecker.Check(package);

        var issue = Assert.Single(issues);
        Assert.Equal(ResearchQualitySeverity.Error, issue.Severity);
        Assert.Equal(ResearchQualityCode.RequiredFieldMissing, issue.Code);
        Assert.Equal("challenges[0].reasoning", issue.Path);
    }

    [Fact]
    public void Check_WithChallengeBlankTargetType_ReturnsExactlyOneInvalidChallengeTargetTypeError()
    {
        // A blank target type short-circuits (continue) before the target-index check runs,
        // so exactly one issue should be produced, not two.
        var package = ValidPackage(
            challenges: [new ProposedChallengeDto("Valid statement.", "Valid reasoning.", 50, "", 0)]);

        var issues = ResearchQualityChecker.Check(package);

        var issue = Assert.Single(issues);
        Assert.Equal(ResearchQualitySeverity.Error, issue.Severity);
        Assert.Equal(ResearchQualityCode.InvalidChallengeTargetType, issue.Code);
        Assert.Equal("challenges[0].targetType", issue.Path);
    }

    [Fact]
    public void Check_WithChallengeUnrecognizedTargetType_ReturnsInvalidChallengeTargetTypeError()
    {
        var package = ValidPackage(
            challenges: [new ProposedChallengeDto("Valid statement.", "Valid reasoning.", 50, "Bogus", 0)]);

        var issues = ResearchQualityChecker.Check(package);

        var issue = Assert.Single(issues);
        Assert.Equal(ResearchQualitySeverity.Error, issue.Severity);
        Assert.Equal(ResearchQualityCode.InvalidChallengeTargetType, issue.Code);
        Assert.Equal("challenges[0].targetType", issue.Path);
    }

    [Theory]
    [InlineData("Evidence")]
    [InlineData("Assumption")]
    [InlineData("Hypothesis")]
    [InlineData("Opportunity")]
    public void Check_WithChallengeTargetIndexOutOfRange_ReturnsInvalidChallengeTargetIndexError(string targetType)
    {
        var package = ValidPackage(
            challenges: [new ProposedChallengeDto("Valid statement.", "Valid reasoning.", 50, targetType, 99)]);

        var issues = ResearchQualityChecker.Check(package);

        var issue = Assert.Single(issues);
        Assert.Equal(ResearchQualitySeverity.Error, issue.Severity);
        Assert.Equal(ResearchQualityCode.InvalidChallengeTargetIndex, issue.Code);
        Assert.Equal("challenges[0].targetIndex", issue.Path);
    }

    // ======================================
    // HELPERS
    // ======================================
    private static ProposedObservationDto ValidObservation() => new(
        "Observation text.", "Summary.", "Companies House filing 2024", 50);

    private static ProposedEvidenceDto ValidEvidence(params int[] observationIndexes) => new(
        "Summary.", "Interpretation.", 0, observationIndexes);

    private static ProposedEvidenceDto ValidEvidenceWithSummary(string summary, params int[] observationIndexes) => new(
        summary, "Interpretation.", 0, observationIndexes);

    private static ProposedAssumptionDto ValidAssumption() => new(
        "Statement.", "Rationale.", 50);

    private static ProposedOpportunityDto ValidOpportunity(int[] evidenceIndexes, int[] assumptionIndexes) => new(
        "Statement.", "Customer value.", "Commercial value.", "Differentiation.", "Timing.", 50,
        evidenceIndexes, assumptionIndexes);

    private static ProposedHypothesisDto ValidHypothesis(int[] evidenceIndexes, int[] assumptionIndexes) => new(
        "Statement.", "Reasoning.", "Expected outcome.", "Success criteria.", 50,
        evidenceIndexes, assumptionIndexes);

    private static ProposedChallengeDto ValidChallenge(string targetType, int targetIndex) => new(
        "Statement.", "Reasoning.", 50, targetType, targetIndex);

    private static ResearchPackageDto ValidPackage(
        IReadOnlyList<ProposedObservationDto>? observations = null,
        IReadOnlyList<ProposedEvidenceDto>? evidence = null,
        IReadOnlyList<ProposedAssumptionDto>? assumptions = null,
        IReadOnlyList<ProposedOpportunityDto>? opportunities = null,
        IReadOnlyList<ProposedHypothesisDto>? hypotheses = null,
        IReadOnlyList<ProposedChallengeDto>? challenges = null) => new(
            Guid.NewGuid(),
            "Mission.",
            new ResearchGenerationDto(
                "Ollama", "test-model", "Research Analyst", "1.0.0", "1.0.0",
                DateTime.UtcNow, TimeSpan.Zero, "Limitations."),
            observations ?? [ValidObservation()],
            evidence ?? [ValidEvidence(0)],
            assumptions ?? [ValidAssumption()],
            opportunities ?? [ValidOpportunity([0], [0])],
            hypotheses ?? [ValidHypothesis([0], [0])],
            challenges ?? [ValidChallenge("Hypothesis", 0)]);
}
