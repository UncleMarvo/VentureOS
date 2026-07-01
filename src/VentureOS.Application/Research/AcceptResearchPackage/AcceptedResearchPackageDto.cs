public sealed record AcceptedResearchPackageDto(
    Guid CaseId,
    int ObservationsCreated,
    int EvidenceCreated,
    int AssumptionsCreated,
    int HypothesesCreated,
    int ChallengesCreated);
