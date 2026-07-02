using VentureOS.Domain.Common;

namespace VentureOS.Domain.Opportunities;

public sealed record OpportunityDraft(
    string Statement,
    string CustomerValue,
    string CommercialValue,
    string Differentiation,
    string Timing,
    Confidence Confidence,
    IReadOnlyCollection<Guid> EvidenceIds,
    IReadOnlyCollection<Guid> AssumptionIds);
