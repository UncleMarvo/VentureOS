using VentureOS.Domain.Common;

namespace VentureOS.Domain.Challenges;

public sealed record ChallengeDraft(
    ChallengeTarget Target,
    Guid TargetId,
    string Statement,
    string Reasoning,
    Confidence Confidence);