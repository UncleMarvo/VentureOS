namespace VentureOS.Application.Decisions.GetDecisionContext;

public sealed record GetDecisionContextQuery(
    Guid CaseId,
    Guid DecisionId);
