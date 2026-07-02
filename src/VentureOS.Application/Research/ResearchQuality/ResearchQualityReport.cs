using VentureOS.Application.Research.ResearchQuality;

public sealed class ResearchQualityReport
{
    private readonly List<ResearchQualityIssueDto> _issues = [];

    public IReadOnlyCollection<ResearchQualityIssueDto> Issues => _issues;

    public bool HasWarnings =>
        _issues.Any(i => i.Severity == ResearchQualitySeverity.Warning);

    public bool HasErrors =>
        _issues.Any(i => i.Severity == ResearchQualitySeverity.Error);

    public void Add(ResearchQualityIssueDto issue)
    {
        _issues.Add(issue);
    }
}
