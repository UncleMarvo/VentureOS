using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResearchQualityCode
{
    UnsupportedNumericalClaim,
    InvalidObservationIndex,
    InvalidEvidenceIndex,
    InvalidAssumptionIndex,
    InvalidChallengeTargetType,
    InvalidChallengeTargetIndex,
    GenericObservation,
    WeakAssumption,
    UntestableHypothesis,
    DuplicateObservation,
    EvidenceIntroducesNewFact,
    RequiredFieldMissing
}
