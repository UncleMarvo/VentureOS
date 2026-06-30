using VentureOS.Domain.Common;

namespace VentureOS.Domain.Hypotheses;

public sealed record HypothesisDraft(
    string Statement,
    string Reasoning,
    string ExpectedOutcome,
    string SuccessCriteria,
    Confidence Confidence,
    IReadOnlyCollection<Guid> EvidenceIds,
    IReadOnlyCollection<Guid> AssumptionIds);