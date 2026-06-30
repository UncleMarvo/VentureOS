using VentureOS.Domain.Common;
using VentureOS.Domain.Cases;
using VentureOS.Domain.Cases.Events;
using VentureOS.Domain.Evidence;
using VentureOS.Domain.Hypotheses;
using VentureOS.Domain.Observations;
using VentureOS.Domain.Assumptions;
using VentureOS.Domain.Challenges;

namespace VentureOS.Domain.Tests.Cases;

public sealed class CaseTests
{
    [Fact]
    public void Create_WithValidDetails_ReturnsSuccessfulResult()
    {
        var result = Case.Create(
            "AI Document Chasing for Accountants",
            "Investigate whether this is a viable software opportunity.");

        Assert.True(result.IsSuccess);
        Assert.Equal("AI Document Chasing for Accountants", result.Value.Title);
        Assert.Equal("Investigate whether this is a viable software opportunity.", result.Value.Mission);
        Assert.Equal(CaseStatus.Draft, result.Value.Status);
    }

    [Fact]
    public void Create_WithEmptyTitle_ReturnsFailure()
    {
        var result = Case.Create("", "A valid mission.");

        Assert.True(result.IsFailure);
        Assert.Equal("Case title is required.", result.Error);
    }

    [Fact]
    public void Create_WithEmptyMission_ReturnsFailure()
    {
        var result = Case.Create("Valid title", "");

        Assert.True(result.IsFailure);
        Assert.Equal("Case mission is required.", result.Error);
    }

    [Fact]
    public void Create_RaisesCaseCreatedDomainEvent()
    {
        var result = Case.Create("Valid title", "Valid mission.");

        var domainEvent = Assert.Single(result.Value.DomainEvents);

        Assert.IsType<CaseCreatedEvent>(domainEvent);
    }

    [Fact]
    public void Activate_WhenDraft_ChangesStatusToActive()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.Activate();

        Assert.True(result.IsSuccess);
        Assert.Equal(CaseStatus.Active, ventureCase.Status);
    }

    [Fact]
    public void Archive_WhenNotArchived_ChangesStatusToArchived()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.Archive();

        Assert.True(result.IsSuccess);
        Assert.Equal(CaseStatus.Archived, ventureCase.Status);
    }

    [Fact]
    public void Activate_WhenArchived_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        ventureCase.Archive();

        var result = ventureCase.Activate();

        Assert.True(result.IsFailure);
        Assert.Equal("Archived cases cannot be activated.", result.Error);
    }

    // ======================================
    // OBSERVATION TESTS
    // ======================================

    [Fact]
    public void AddObservation_WithValidDetails_AddsObservationToCase()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.AddObservation(
            new ObservationDraft(
                "Every month-end I spend half my Friday chasing invoices.",
                "Accountants report spending time chasing client documents.",
                "Manual research note",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        Assert.True(result.IsSuccess);

        var observation = Assert.Single(ventureCase.Observations);

        Assert.Equal(ventureCase.Id, observation.CaseId);
        Assert.Equal("Every month-end I spend half my Friday chasing invoices.", observation.ObservationText);
        Assert.Equal("Accountants report spending time chasing client documents.", observation.Summary);
        Assert.Equal("Manual research note", observation.SourceReference);
        Assert.Equal(ObservationSource.ManualNote, observation.ObservationSource);
        Assert.Equal(80, observation.Confidence.Value);
    }

    [Fact]
    public void AddObservation_WithEmptyText_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.AddObservation(
            new ObservationDraft(
                "",
                "Valid observation.",
                "Manual Research Note",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        Assert.True(result.IsFailure);
        Assert.Equal("Observation text is required.", result.Error);
    }

    [Fact]
    public void AddObservation_WithEmptySummary_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.AddObservation(
            new ObservationDraft(
                "Every month-end I spend half my Friday chasing invoices.",
                "",
                "Manual research note",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        Assert.True(result.IsFailure);
        Assert.Equal("Observation summary is required.", result.Error);
    }

    [Fact]
    public void AddObservation_WithEmptySource_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.AddObservation(
            new ObservationDraft(
                "Every month-end I spend half my Friday chasing invoices.",
                "Valid observation.",
                "",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        Assert.True(result.IsFailure);
        Assert.Equal("Observation source is required.", result.Error);
    }

    [Fact]
    public void AddObservation_WhenCaseArchived_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        ventureCase.Archive();

        var result = ventureCase.AddObservation(
            new ObservationDraft(
                "Every month-end I spend half my Friday chasing invoices.",
                "Valid observation.",
                "Manual research note",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        Assert.True(result.IsFailure);
        Assert.Equal("Cannot add observations to an archived case.", result.Error);
    }

    [Fact]
    public void AddObservation_WithValidDetails_RaisesObservationAddedDomainEvent()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        ventureCase.ClearDomainEvents();

        var result = ventureCase.AddObservation(
            new ObservationDraft(
                "Every month-end I spend half my Friday chasing invoices.",
                "Valid observation.",
                "Manual research note",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        Assert.True(result.IsSuccess);

        var domainEvent = Assert.Single(ventureCase.DomainEvents);

        Assert.IsType<ObservationAddedEvent>(domainEvent);
    }

    // ======================================
    // EVIDENCE TESTS
    // ======================================

    [Fact]
    public void CreateEvidence_WithValidDraft_CreatesEvidence()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        ventureCase.AddObservation(
            new ObservationDraft(
                "Every month-end I spend half my Friday chasing invoices.",
                "Accountants report spending time chasing client documents.",
                "Manual research note",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        var observationId = ventureCase.Observations.Single().Id;

        var result = ventureCase.CreateEvidence(
            new EvidenceDraft(
                "Document chasing appears to be a repeated admin burden.",
                "The observation suggests accountants experience recurring administrative friction around collecting documents.",
                EvidenceDirection.Supports,
                [observationId]));

        Assert.True(result.IsSuccess);

        var evidence = Assert.Single(ventureCase.Evidence);

        Assert.Equal(ventureCase.Id, evidence.CaseId);
        Assert.Equal("Document chasing appears to be a repeated admin burden.", evidence.Summary);
        Assert.Equal("The observation suggests accountants experience recurring administrative friction around collecting documents.", evidence.Interpretation);
        Assert.Equal(EvidenceDirection.Supports, evidence.Direction);
        Assert.Contains(observationId, evidence.ObservationIds);
    }

    [Fact]
    public void CreateEvidence_WithEmptySummary_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        ventureCase.AddObservation(
            new ObservationDraft(
                "Every month-end I spend half my Friday chasing invoices.",
                "Valid observation.",
                "Manual research note",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        var observationId = ventureCase.Observations.Single().Id;

        var result = ventureCase.CreateEvidence(
            new EvidenceDraft(
                "",
                "Valid interpretation.",
                EvidenceDirection.Supports,
                [observationId]));

        Assert.True(result.IsFailure);
        Assert.Equal("Evidence summary is required.", result.Error);
    }

    [Fact]
    public void CreateEvidence_WithEmptyInterpretation_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        ventureCase.AddObservation(
            new ObservationDraft(
                "Every month-end I spend half my Friday chasing invoices.",
                "Valid observation.",
                "Manual research note",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        var observationId = ventureCase.Observations.Single().Id;

        var result = ventureCase.CreateEvidence(
            new EvidenceDraft(
                "Valid summary.",
                "",
                EvidenceDirection.Supports,
                [observationId]));

        Assert.True(result.IsFailure);
        Assert.Equal("Evidence interpretation is required.", result.Error);
    }

    [Fact]
    public void CreateEvidence_WithNoObservationIds_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.CreateEvidence(
            new EvidenceDraft(
                "Valid summary.",
                "Valid interpretation.",
                EvidenceDirection.Supports,
                []));

        Assert.True(result.IsFailure);
        Assert.Equal("Evidence must reference at least one observation.", result.Error);
    }

    [Fact]
    public void CreateEvidence_WithUnknownObservationId_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.CreateEvidence(
            new EvidenceDraft(
                "Valid summary.",
                "Valid interpretation.",
                EvidenceDirection.Supports,
                [Guid.NewGuid()]));

        Assert.True(result.IsFailure);
        Assert.Equal("Evidence cannot reference observations that do not belong to the case.", result.Error);
    }

    [Fact]
    public void CreateEvidence_WhenCaseArchived_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        ventureCase.Archive();

        var result = ventureCase.CreateEvidence(
            new EvidenceDraft(
                "Valid summary.",
                "Valid interpretation.",
                EvidenceDirection.Supports,
                [Guid.NewGuid()]));

        Assert.True(result.IsFailure);
        Assert.Equal("Cannot create evidence for an archived case.", result.Error);
    }

    [Fact]
    public void CreateEvidence_WithDuplicateObservationIds_StoresDistinctObservationIds()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        ventureCase.AddObservation(
            new ObservationDraft(
                "Every month-end I spend half my Friday chasing invoices.",
                "Valid observation.",
                "Manual research note",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        var observationId = ventureCase.Observations.Single().Id;

        var result = ventureCase.CreateEvidence(
            new EvidenceDraft(
                "Valid summary.",
                "Valid interpretation.",
                EvidenceDirection.Supports,
                [observationId, observationId]));

        Assert.True(result.IsSuccess);

        var evidence = Assert.Single(ventureCase.Evidence);

        Assert.Single(evidence.ObservationIds);
    }

    [Fact]
    public void CreateEvidence_WithValidDraft_RaisesEvidenceCreatedDomainEvent()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        ventureCase.AddObservation(
            new ObservationDraft(
                "Every month-end I spend half my Friday chasing invoices.",
                "Valid observation.",
                "Manual research note",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        var observationId = ventureCase.Observations.Single().Id;

        ventureCase.ClearDomainEvents();

        var result = ventureCase.CreateEvidence(
            new EvidenceDraft(
                "Valid summary.",
                "Valid interpretation.",
                EvidenceDirection.Supports,
                [observationId]));

        Assert.True(result.IsSuccess);

        var domainEvent = Assert.Single(ventureCase.DomainEvents);

        Assert.IsType<EvidenceCreatedEvent>(domainEvent);
    }


    // =========================
    // HYPOTHESIS TESTS
    // =========================
    [Fact]
    public void Hypothesis_MarkSupported_ChangesStatusToSupported()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        var result = hypothesis.MarkSupported();

        Assert.True(result.IsSuccess);
        Assert.Equal(HypothesisStatus.Supported, hypothesis.Status);
    }

    [Fact]
    public void Hypothesis_MarkChallenged_ChangesStatusToChallenged()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        var result = hypothesis.MarkChallenged();

        Assert.True(result.IsSuccess);
        Assert.Equal(HypothesisStatus.Challenged, hypothesis.Status);
    }

    [Fact]
    public void Hypothesis_Accept_ChangesStatusToAccepted()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        var result = hypothesis.Accept();

        Assert.True(result.IsSuccess);
        Assert.Equal(HypothesisStatus.Accepted, hypothesis.Status);
    }

    [Fact]
    public void Hypothesis_Reject_ChangesStatusToRejected()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        var result = hypothesis.Reject();

        Assert.True(result.IsSuccess);
        Assert.Equal(HypothesisStatus.Rejected, hypothesis.Status);
    }

    [Fact]
    public void Hypothesis_Supersede_ChangesStatusToSuperseded()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        var result = hypothesis.Supersede();

        Assert.True(result.IsSuccess);
        Assert.Equal(HypothesisStatus.Superseded, hypothesis.Status);
    }

    [Fact]
    public void Hypothesis_MarkSupported_WhenRejected_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        hypothesis.Reject();

        var result = hypothesis.MarkSupported();

        Assert.True(result.IsFailure);
        Assert.Equal("Rejected or superseded hypotheses cannot be marked as supported.", result.Error);
    }

    [Fact]
    public void Hypothesis_MarkChallenged_WhenSuperseded_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        hypothesis.Supersede();

        var result = hypothesis.MarkChallenged();

        Assert.True(result.IsFailure);
        Assert.Equal("Rejected or superseded hypotheses cannot be challenged.", result.Error);
    }

    [Fact]
    public void Hypothesis_Accept_WhenRejected_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        hypothesis.Reject();

        var result = hypothesis.Accept();

        Assert.True(result.IsFailure);
        Assert.Equal("Rejected hypotheses cannot be accepted.", result.Error);
    }

    [Fact]
    public void Hypothesis_Reject_WhenAccepted_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        hypothesis.Accept();

        var result = hypothesis.Reject();

        Assert.True(result.IsFailure);
        Assert.Equal("Accepted hypotheses cannot be rejected.", result.Error);
    }

    [Fact]
    public void Hypothesis_Supersede_WhenAccepted_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        hypothesis.Accept();

        var result = hypothesis.Supersede();

        Assert.True(result.IsFailure);
        Assert.Equal("Accepted hypotheses cannot be superseded.", result.Error);
    }

    // ======================================
    // ASSUMPTION TESTS
    // ======================================
    [Fact]
    public void CreateAssumption_WithValidDraft_CreatesAssumption()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.CreateAssumption(
            new AssumptionDraft(
                "Small accounting firms are willing to pay for admin automation.",
                "The target users experience recurring admin pain and may value time savings.",
                Confidence.FromPercentage(55)));

        Assert.True(result.IsSuccess);

        var assumption = Assert.Single(ventureCase.Assumptions);

        Assert.Equal(ventureCase.Id, assumption.CaseId);
        Assert.Equal("Small accounting firms are willing to pay for admin automation.", assumption.Statement);
        Assert.Equal("The target users experience recurring admin pain and may value time savings.", assumption.Rationale);
        Assert.Equal(55, assumption.Confidence.Value);
        Assert.Equal(AssumptionStatus.Proposed, assumption.Status);
    }

    [Fact]
    public void CreateAssumption_WithEmptyStatement_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.CreateAssumption(
            new AssumptionDraft(
                "",
                "Valid rationale.",
                Confidence.FromPercentage(50)));

        Assert.True(result.IsFailure);
        Assert.Equal("Assumption statement is required.", result.Error);
    }

    [Fact]
    public void CreateAssumption_WithEmptyRationale_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.CreateAssumption(
            new AssumptionDraft(
                "Valid statement.",
                "",
                Confidence.FromPercentage(50)));

        Assert.True(result.IsFailure);
        Assert.Equal("Assumption rationale is required.", result.Error);
    }

    [Fact]
    public void CreateAssumption_WhenCaseArchived_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        ventureCase.Archive();

        var result = ventureCase.CreateAssumption(
            new AssumptionDraft(
                "Valid statement.",
                "Valid rationale.",
                Confidence.FromPercentage(50)));

        Assert.True(result.IsFailure);
        Assert.Equal("Cannot create assumptions for an archived case.", result.Error);
    }

    [Fact]
    public void CreateAssumption_WithValidDraft_RaisesAssumptionCreatedDomainEvent()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        ventureCase.ClearDomainEvents();

        var result = ventureCase.CreateAssumption(
            new AssumptionDraft(
                "Valid statement.",
                "Valid rationale.",
                Confidence.FromPercentage(50)));

        Assert.True(result.IsSuccess);

        var domainEvent = Assert.Single(ventureCase.DomainEvents);

        Assert.IsType<AssumptionCreatedEvent>(domainEvent);
    }

    [Fact]
    public void CreateHypothesis_WithValidDraft_StoresAssumptionIds()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var evidenceId = CreateValidEvidence(ventureCase);
        var assumptionId = CreateValidAssumption(ventureCase);

        var result = ventureCase.CreateHypothesis(
            new HypothesisDraft(
                "Valid hypothesis statement.",
                "Valid hypothesis reasoning.",
                "Valid expected outcome.",
                "Valid success criteria.",
                Confidence.FromPercentage(60),
                [evidenceId],
                [assumptionId]));

        Assert.True(result.IsSuccess);

        var hypothesis = Assert.Single(ventureCase.Hypotheses);

        Assert.Contains(assumptionId, hypothesis.AssumptionIds);
    }

    [Fact]
    public void CreateHypothesis_WithNoAssumptionIds_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var evidenceId = CreateValidEvidence(ventureCase);

        var result = ventureCase.CreateHypothesis(
            new HypothesisDraft(
                "Valid hypothesis statement.",
                "Valid hypothesis reasoning.",
                "Valid expected outcome.",
                "Valid success criteria.",
                Confidence.FromPercentage(60),
                [evidenceId],
                []));

        Assert.True(result.IsFailure);
        Assert.Equal("Hypothesis must reference at least one assumption.", result.Error);
    }

    [Fact]
    public void CreateHypothesis_WithUnknownAssumptionId_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var evidenceId = CreateValidEvidence(ventureCase);

        var result = ventureCase.CreateHypothesis(
            new HypothesisDraft(
                "Valid hypothesis statement.",
                "Valid hypothesis reasoning.",
                "Valid expected outcome.",
                "Valid success criteria.",
                Confidence.FromPercentage(60),
                [evidenceId],
                [Guid.NewGuid()]));

        Assert.True(result.IsFailure);
        Assert.Equal("Hypothesis cannot reference assumptions that do not belong to the case.", result.Error);
    }

    // ======================================
    // CHALLENGE TESTS
    // ======================================
    [Fact]
    public void RaiseChallenge_AgainstHypothesis_WithValidDraft_CreatesChallenge()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        var result = ventureCase.RaiseChallenge(
            new ChallengeDraft(
                ChallengeTarget.Hypothesis,
                hypothesis.Id,
                "The target market may not be willing to pay.",
                "The hypothesis relies on willingness to pay, but no pricing evidence has been gathered.",
                Confidence.FromPercentage(70)));

        Assert.True(result.IsSuccess);

        var challenge = Assert.Single(ventureCase.Challenges);

        Assert.Equal(ventureCase.Id, challenge.CaseId);
        Assert.Equal(ChallengeTarget.Hypothesis, challenge.Target);
        Assert.Equal(hypothesis.Id, challenge.TargetId);
        Assert.Equal("The target market may not be willing to pay.", challenge.Statement);
        Assert.Equal("The hypothesis relies on willingness to pay, but no pricing evidence has been gathered.", challenge.Reasoning);
        Assert.Equal(70, challenge.Confidence.Value);
    }

    [Fact]
    public void RaiseChallenge_AgainstEvidence_WithValidDraft_CreatesChallenge()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var evidenceId = CreateValidEvidence(ventureCase);

        var result = ventureCase.RaiseChallenge(
            new ChallengeDraft(
                ChallengeTarget.Evidence,
                evidenceId,
                "The evidence may be too weak.",
                "The interpretation is based on a single observation.",
                Confidence.FromPercentage(65)));

        Assert.True(result.IsSuccess);

        var challenge = Assert.Single(ventureCase.Challenges);

        Assert.Equal(ChallengeTarget.Evidence, challenge.Target);
        Assert.Equal(evidenceId, challenge.TargetId);
    }

    [Fact]
    public void RaiseChallenge_AgainstAssumption_WithValidDraft_CreatesChallenge()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var assumptionId = CreateValidAssumption(ventureCase);

        var result = ventureCase.RaiseChallenge(
            new ChallengeDraft(
                ChallengeTarget.Assumption,
                assumptionId,
                "The assumption may be untested.",
                "No direct evidence currently confirms willingness to pay.",
                Confidence.FromPercentage(75)));

        Assert.True(result.IsSuccess);

        var challenge = Assert.Single(ventureCase.Challenges);

        Assert.Equal(ChallengeTarget.Assumption, challenge.Target);
        Assert.Equal(assumptionId, challenge.TargetId);
    }

    [Fact]
    public void RaiseChallenge_WithEmptyTargetId_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.RaiseChallenge(
            new ChallengeDraft(
                ChallengeTarget.Hypothesis,
                Guid.Empty,
                "Valid statement.",
                "Valid reasoning.",
                Confidence.FromPercentage(50)));

        Assert.True(result.IsFailure);
        Assert.Equal("Challenge target ID is required.", result.Error);
    }

    [Fact]
    public void RaiseChallenge_WithEmptyStatement_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        var result = ventureCase.RaiseChallenge(
            new ChallengeDraft(
                ChallengeTarget.Hypothesis,
                hypothesis.Id,
                "",
                "Valid reasoning.",
                Confidence.FromPercentage(50)));

        Assert.True(result.IsFailure);
        Assert.Equal("Challenge statement is required.", result.Error);
    }

    [Fact]
    public void RaiseChallenge_WithEmptyReasoning_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        var result = ventureCase.RaiseChallenge(
            new ChallengeDraft(
                ChallengeTarget.Hypothesis,
                hypothesis.Id,
                "Valid statement.",
                "",
                Confidence.FromPercentage(50)));

        Assert.True(result.IsFailure);
        Assert.Equal("Challenge reasoning is required.", result.Error);
    }

    [Fact]
    public void RaiseChallenge_WithUnknownTarget_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.RaiseChallenge(
            new ChallengeDraft(
                ChallengeTarget.Hypothesis,
                Guid.NewGuid(),
                "Valid statement.",
                "Valid reasoning.",
                Confidence.FromPercentage(50)));

        Assert.True(result.IsFailure);
        Assert.Equal("Challenge target does not belong to the case.", result.Error);
    }

    [Fact]
    public void RaiseChallenge_AgainstDecisionBeforeDecisionExists_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.RaiseChallenge(
            new ChallengeDraft(
                ChallengeTarget.Decision,
                Guid.NewGuid(),
                "Valid statement.",
                "Valid reasoning.",
                Confidence.FromPercentage(50)));

        Assert.True(result.IsFailure);
        Assert.Equal("Challenge target does not belong to the case.", result.Error);
    }

    [Fact]
    public void RaiseChallenge_WhenCaseArchived_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        ventureCase.Archive();

        var result = ventureCase.RaiseChallenge(
            new ChallengeDraft(
                ChallengeTarget.Hypothesis,
                hypothesis.Id,
                "Valid statement.",
                "Valid reasoning.",
                Confidence.FromPercentage(50)));

        Assert.True(result.IsFailure);
        Assert.Equal("Cannot raise challenges for an archived case.", result.Error);
    }

    [Fact]
    public void RaiseChallenge_WithValidDraft_RaisesChallengeRaisedDomainEvent()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;
        var hypothesis = CreateValidHypothesis(ventureCase);

        ventureCase.ClearDomainEvents();

        var result = ventureCase.RaiseChallenge(
            new ChallengeDraft(
                ChallengeTarget.Hypothesis,
                hypothesis.Id,
                "Valid statement.",
                "Valid reasoning.",
                Confidence.FromPercentage(50)));

        Assert.True(result.IsSuccess);

        var domainEvent = Assert.Single(ventureCase.DomainEvents);

        Assert.IsType<ChallengeRaisedEvent>(domainEvent);
    }

    // ==================================
    // HELPER METHODS
    // ==================================
    private static Hypothesis CreateValidHypothesis(Case ventureCase)
    {
        var evidenceId = CreateValidEvidence(ventureCase);
        var assumptionId = CreateValidAssumption(ventureCase);

        ventureCase.CreateHypothesis(
            new HypothesisDraft(
                "Valid hypothesis statement.",
                "Valid hypothesis reasoning.",
                "Valid expected outcome.",
                "Valid success criteria.",
                Confidence.FromPercentage(60),
                [evidenceId],
                [assumptionId]));

        return ventureCase.Hypotheses.Single();
    }

    private static Guid CreateValidEvidence(Case ventureCase)
    {
        ventureCase.AddObservation(
            new ObservationDraft(
                "Raw observation text.",
                "Valid observation summary.",
                "Manual research note",
                ObservationSource.ManualNote,
                Confidence.FromPercentage(80)));

        Guid observationId = ventureCase.Observations.Single().Id;

        ventureCase.CreateEvidence(
            new EvidenceDraft(
                "Valid evidence summary.",
                "Valid evidence interpretation.",
                EvidenceDirection.Supports,
                [observationId]));

        return ventureCase.Evidence.Single().Id;
    }

    private static Guid CreateValidAssumption(Case ventureCase)
    {
        ventureCase.CreateAssumption(
            new AssumptionDraft(
                "Users are willing to pay for the proposed solution.",
                "The problem appears recurring and costly enough to justify paid automation.",
                Confidence.FromPercentage(55)));

        return ventureCase.Assumptions.Single().Id;
    }
}


