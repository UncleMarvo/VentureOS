using VentureOS.Domain.Decisions;

namespace VentureOS.Application.Board;

public sealed record BoardBriefingDto(
    BoardDossierDto Dossier,
    BoardNarrativeDto Narrative);

public sealed record BoardNarrativeDto(
    string ExecutiveSummary,
    string DecisionFraming,
    IReadOnlyList<string> Risks,
    string OverallConfidenceNarrative,
    IReadOnlyList<string> RecommendedInvestigations,
    IReadOnlyList<BoardDecisionOptionDto> DecisionOptions,
    BoardGenerationDto Generation);

public sealed record BoardDecisionOptionDto(
    DecisionOutcome Outcome,
    string Rationale);

public sealed record BoardGenerationDto(
    string Provider,
    string Model,
    string Persona,
    string PersonaVersion,
    string PromptVersion,
    DateTime GeneratedAtUtc,
    TimeSpan Duration,
    string Limitations);
