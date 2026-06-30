using VentureOS.Domain.Common;

namespace VentureOS.Domain.Assumptions;

public sealed record AssumptionDraft(
    string Statement,
    string Rationale,
    Confidence Confidence);