public sealed record AcceptedResearchPackageDto(
    Guid CaseId,
    int ObservationsCreated,
    int EvidenceCreated,
    int AssumptionsCreated,
    int OpportunitiesCreated,
    int HypothesesCreated,
    int ChallengesCreated);
