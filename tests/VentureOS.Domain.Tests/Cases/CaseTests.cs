using VentureOS.Domain.Cases;
using VentureOS.Domain.Cases.Events;
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

    [Fact]
    public void AddObservation_WithValidDetails_AddsObservationToCase()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.AddObservation(
            new ObservationDraft(
                "Accountants report spending time chasing client documents.",
                "Manual research note"));

        Assert.True(result.IsSuccess);

        var observation = Assert.Single(ventureCase.Observations);

        Assert.Equal(ventureCase.Id, observation.CaseId);
        Assert.Equal("Accountants report spending time chasing client documents.", observation.Summary);
        Assert.Equal("Manual research note", observation.Source);
    }

    [Fact]
    public void AddObservation_WithEmptySummary_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.AddObservation(
            new ObservationDraft(
                "", 
                "Manual research note"));

        Assert.True(result.IsFailure);
        Assert.Equal("Observation summary is required.", result.Error);
    }

    [Fact]
    public void AddObservation_WithEmptySource_ReturnsFailure()
    {
        var ventureCase = Case.Create("Valid title", "Valid mission.").Value;

        var result = ventureCase.AddObservation(
            new ObservationDraft(
                "Valid observation.", 
                ""));

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
                "Valid observation.",
                "Manual research note"));

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
                "Valid observation.",
                "Manual research note"));

        Assert.True(result.IsSuccess);

        var domainEvent = Assert.Single(ventureCase.DomainEvents);

        Assert.IsType<ObservationAddedEvent>(domainEvent);
    }
}