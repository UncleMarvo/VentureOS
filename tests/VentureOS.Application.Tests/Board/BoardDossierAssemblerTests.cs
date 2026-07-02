using VentureOS.Application.Board;
using VentureOS.Domain.Assumptions;
using VentureOS.Domain.Cases;
using VentureOS.Domain.Challenges;
using VentureOS.Domain.Common;
using VentureOS.Domain.Evidence;
using VentureOS.Domain.Hypotheses;
using VentureOS.Domain.Observations;
using VentureOS.Domain.Opportunities;

namespace VentureOS.Application.Tests.Board;

public sealed class BoardDossierAssemblerTests
{
    [Fact]
    public void Assemble_PartitionsEvidenceByDirection()
    {
        var ventureCase = Case.Create("Valid case title.", "Valid case mission.").Value;

        var supportingObservationId = AddObservation(ventureCase);
        ventureCase.CreateEvidence(new EvidenceDraft(
            "Supporting summary.", "Supporting interpretation.", EvidenceDirection.Supports, [supportingObservationId]));

        var contradictingObservationId = AddObservation(ventureCase);
        ventureCase.CreateEvidence(new EvidenceDraft(
            "Contradicting summary.", "Contradicting interpretation.", EvidenceDirection.Contradicts, [contradictingObservationId]));

        var neutralObservationId = AddObservation(ventureCase);
        ventureCase.CreateEvidence(new EvidenceDraft(
            "Neutral summary.", "Neutral interpretation.", EvidenceDirection.Neutral, [neutralObservationId]));

        var dossier = BoardDossierAssembler.Assemble(ventureCase, [], []);

        Assert.Single(dossier.SupportingEvidence);
        Assert.Equal("Supporting summary.", dossier.SupportingEvidence[0].Summary);

        Assert.Single(dossier.ContradictingEvidence);
        Assert.Equal("Contradicting summary.", dossier.ContradictingEvidence[0].Summary);

        Assert.Single(dossier.NeutralEvidence);
        Assert.Equal("Neutral summary.", dossier.NeutralEvidence[0].Summary);
    }

    [Fact]
    public void Assemble_AllAssumptionsAreUnresolved_RegardlessOfStatus()
    {
        // Assumption has no status-transition mutator in the Domain today, so every assumption
        // is permanently Status == Proposed from creation. If assumption status transitions are
        // ever implemented, this test should be revisited alongside BoardDossierAssembler's
        // "unresolved" definition.
        var ventureCase = Case.Create("Valid case title.", "Valid case mission.").Value;

        ventureCase.CreateAssumption(new AssumptionDraft(
            "Statement.", "Rationale.", Confidence.FromPercentage(50)));

        var dossier = BoardDossierAssembler.Assemble(ventureCase, [], []);

        var assumption = Assert.Single(dossier.UnresolvedAssumptions);
        Assert.Equal(AssumptionStatus.Proposed, assumption.Status);
    }

    [Theory]
    [InlineData(ChallengeTarget.Evidence)]
    [InlineData(ChallengeTarget.Assumption)]
    [InlineData(ChallengeTarget.Hypothesis)]
    [InlineData(ChallengeTarget.Opportunity)]
    public void Assemble_ResolvesChallengeTargetTextForEachTargetType(ChallengeTarget target)
    {
        var ventureCase = ValidCase();

        var targetId = target switch
        {
            ChallengeTarget.Evidence => ventureCase.Evidence.Last().Id,
            ChallengeTarget.Assumption => ventureCase.Assumptions.Last().Id,
            ChallengeTarget.Hypothesis => ventureCase.Hypotheses.Last().Id,
            ChallengeTarget.Opportunity => ventureCase.Opportunities.Last().Id,
            _ => throw new InvalidOperationException("Unsupported target for this test.")
        };

        var expectedText = target switch
        {
            ChallengeTarget.Evidence => ventureCase.Evidence.Last().Summary,
            ChallengeTarget.Assumption => ventureCase.Assumptions.Last().Statement,
            ChallengeTarget.Hypothesis => ventureCase.Hypotheses.Last().Statement,
            ChallengeTarget.Opportunity => ventureCase.Opportunities.Last().Statement,
            _ => throw new InvalidOperationException("Unsupported target for this test.")
        };

        ventureCase.RaiseChallenge(new ChallengeDraft(
            target, targetId, "Challenge statement.", "Challenge reasoning.", Confidence.FromPercentage(50)));

        var dossier = BoardDossierAssembler.Assemble(ventureCase, [], []);

        var challenge = Assert.Single(dossier.Challenges);
        Assert.Equal(expectedText, challenge.TargetText);
    }

    [Fact]
    public void Assemble_CountsMatchUnderlyingCaseCollections()
    {
        var ventureCase = ValidCase();

        var dossier = BoardDossierAssembler.Assemble(ventureCase, [], []);

        Assert.Equal(ventureCase.Observations.Count, dossier.Observations.Count);
        Assert.Equal(ventureCase.Evidence.Count,
            dossier.SupportingEvidence.Count + dossier.ContradictingEvidence.Count + dossier.NeutralEvidence.Count);
        Assert.Equal(ventureCase.Assumptions.Count, dossier.UnresolvedAssumptions.Count);
        Assert.Equal(ventureCase.Hypotheses.Count, dossier.Hypotheses.Count);
        Assert.Equal(ventureCase.Opportunities.Count, dossier.Opportunities.Count);
    }

    [Fact]
    public void Assemble_WithNoQualityFindings_ReturnsEmptyLists()
    {
        var ventureCase = Case.Create("Valid case title.", "Valid case mission.").Value;

        var dossier = BoardDossierAssembler.Assemble(ventureCase, [], []);

        Assert.Empty(dossier.ResearchQualityFindings);
        Assert.Empty(dossier.RedTeamQualityFindings);
    }

    [Fact]
    public void Assemble_WithQualityFindings_PassesThemThroughUnmodified()
    {
        var ventureCase = Case.Create("Valid case title.", "Valid case mission.").Value;

        var researchFindings = new[]
        {
            new BoardQualityFindingDto("Warning", "UnsupportedNumericalClaim", "evidence[0].summary", "Message.")
        };

        var redTeamFindings = new[]
        {
            new BoardQualityFindingDto("Error", "InvalidChallengeTarget", "challenges[0].targetId", "Message.")
        };

        var dossier = BoardDossierAssembler.Assemble(ventureCase, researchFindings, redTeamFindings);

        Assert.Same(researchFindings, dossier.ResearchQualityFindings);
        Assert.Same(redTeamFindings, dossier.RedTeamQualityFindings);
    }

    // ======================================
    // HELPERS
    // ======================================
    private static Guid AddObservation(Case ventureCase)
    {
        ventureCase.AddObservation(
            new ObservationDraft(
                "Raw observation text.",
                "Valid observation summary.",
                "Manual research note",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        return ventureCase.Observations.Last().Id;
    }

    private static Case ValidCase()
    {
        var ventureCase = Case.Create("Valid case title.", "Valid case mission.").Value;

        var observationId = AddObservation(ventureCase);

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
}
