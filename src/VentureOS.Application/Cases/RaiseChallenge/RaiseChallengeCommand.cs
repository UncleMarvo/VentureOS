using VentureOS.Domain.Challenges;
using VentureOS.Domain.Common;

namespace VentureOS.Application.Cases.RaiseChallenge;

public sealed record RaiseChallengeCommand(
    Guid CaseId,
    ChallengeTarget Target,
    Guid TargetId,
    string Statement,
    string Reasoning,
    Confidence Confidence);