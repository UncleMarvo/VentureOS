namespace VentureOS.Application.Cases.CreateCase;

public sealed record CreateCaseResult(
    Guid CaseId,
    string Title,
    string Mission);