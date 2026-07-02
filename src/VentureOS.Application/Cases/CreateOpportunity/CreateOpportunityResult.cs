namespace VentureOS.Application.Cases.CreateOpportunity;

public sealed record CreateOpportunityResult(
    Guid CaseId,
    Guid OpportunityId,
    string Statement);
