using VentureOS.Domain.Cases;
using VentureOS.Domain.Cases.Events;

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
}