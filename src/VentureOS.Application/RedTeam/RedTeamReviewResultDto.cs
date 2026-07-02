namespace VentureOS.Application.RedTeam;

public sealed record RedTeamReviewResultDto(
    Guid CaseId,
    string Mission,
    RedTeamGenerationDto Generation,
    IReadOnlyList<RedTeamProposedChallengeDto> Challenges);

public sealed record RedTeamProposedChallengeDto(
    string Statement,
    string Reasoning,
    int Confidence,
    string TargetType,
    Guid TargetId);

public sealed record RedTeamGenerationDto(
    string Provider,
    string Model,
    string Persona,
    string PersonaVersion,
    string PromptVersion,
    DateTime GeneratedAtUtc,
    TimeSpan Duration,
    string Limitations);
