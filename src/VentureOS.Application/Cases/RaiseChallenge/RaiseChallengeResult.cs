using VentureOS.Domain.Challenges;

namespace VentureOS.Application.Cases.RaiseChallenge;

public sealed record RaiseChallengeResult(
    Guid CaseId,
    Guid ChallengeId,
    ChallengeTarget Target,
    Guid TargetId,
    string Statement);