using VentureOS.Domain.Common;

namespace VentureOS.Application.Cases.CreateOpportunity;

public sealed record CreateOpportunityCommand(
    Guid CaseId,
    string Statement,
    string CustomerValue,
    string CommercialValue,
    string Differentiation,
    string Timing,
    Confidence Confidence,
    IReadOnlyCollection<Guid> EvidenceIds,
    IReadOnlyCollection<Guid> AssumptionIds);
