namespace VentureOS.Application.Cases.CreateCase;

public sealed record CreateCaseCommand(
    string Title,
    string Mission);