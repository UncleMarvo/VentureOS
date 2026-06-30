namespace VentureOS.Application.Cases.CreateHypothesis;

public sealed record CreateHypothesisResult(
    Guid CaseId,
    Guid HypothesisId,
    string Statement);