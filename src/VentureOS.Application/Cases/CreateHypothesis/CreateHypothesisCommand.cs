using VentureOS.Domain.Common;

namespace VentureOS.Application.Cases.CreateHypothesis;

public sealed record CreateHypothesisCommand(
    Guid CaseId,
    string Statement,
    string Reasoning,
    string ExpectedOutcome,
    string SuccessCriteria,
    Confidence Confidence,
    IReadOnlyCollection<Guid> EvidenceIds,
    IReadOnlyCollection<Guid> AssumptionIds);