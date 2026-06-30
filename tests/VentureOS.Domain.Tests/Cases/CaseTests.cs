using VentureOS.Domain.Common;
using VentureOS.Domain.Cases;
using VentureOS.Domain.Cases.Events;
using VentureOS.Domain.Evidence;
using VentureOS.Domain.Hypotheses;
using VentureOS.Domain.Observations;

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

    // ==================================
    // HELPER METHODS
    // ==================================
    private static Hypothesis CreateValidHypothesis(Case ventureCase)
    {
        var evidenceId = CreateValidEvidence(ventureCase);

        ventureCase.CreateHypothesis(
            new HypothesisDraft(
                "Valid hypothesis statement.",
                "Valid hypothesis reasoning.",
                "Valid expected outcome.",
                "Valid success criteria.",
                Confidence.FromPercentage(60),
                [evidenceId]));

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
}


