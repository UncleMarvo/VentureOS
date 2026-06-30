using VentureOS.Domain.Common;

namespace VentureOS.Application.Cases.CreateAssumption;

public sealed record CreateAssumptionCommand(
    Guid CaseId,
    string Statement,
    string Rationale,
    Confidence Confidence);