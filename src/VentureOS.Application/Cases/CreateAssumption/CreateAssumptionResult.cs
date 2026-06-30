namespace VentureOS.Application.Cases.CreateAssumption;

public sealed record CreateAssumptionResult(
    Guid CaseId,
    Guid AssumptionId,
    string Statement);