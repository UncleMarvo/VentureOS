using VentureOS.Domain.Cases;
using VentureOS.Domain.Challenges;
using VentureOS.Domain.Evidence;

namespace VentureOS.Application.Board;

public static class BoardDossierAssembler
{
    public static BoardDossierDto Assemble(
        Case ventureCase,
        IReadOnlyList<BoardQualityFindingDto> researchQualityFindings,
        IReadOnlyList<BoardQualityFindingDto> redTeamQualityFindings)
    {
        var observations = ventureCase.Observations
            .OrderBy(observation => observation.CreatedAtUtc)
            .Select(observation => new BoardObservationDto(
                observation.Id,
                observation.Summary,
                observation.SourceReference,
                observation.Confidence.Value,
                observation.CreatedAtUtc))
            .ToList();

        var supportingEvidence = MapEvidence(ventureCase, EvidenceDirection.Supports);
        var contradictingEvidence = MapEvidence(ventureCase, EvidenceDirection.Contradicts);
        var neutralEvidence = MapEvidence(ventureCase, EvidenceDirection.Neutral);

        var unresolvedAssumptions = ventureCase.Assumptions
            .Select(assumption => new BoardAssumptionDto(
                assumption.Id,
                assumption.Statement,
                assumption.Rationale,
                assumption.Status,
                assumption.Confidence.Value,
                assumption.CreatedAtUtc))
            .ToList();

        var hypotheses = ventureCase.Hypotheses
            .Select(hypothesis => new BoardHypothesisDto(
                hypothesis.Id,
                hypothesis.Statement,
                hypothesis.Reasoning,
                hypothesis.Status,
                hypothesis.Confidence.Value,
                hypothesis.CreatedAtUtc))
            .ToList();

        var opportunities = ventureCase.Opportunities
            .Select(opportunity => new BoardOpportunityDto(
                opportunity.Id,
                opportunity.Statement,
                opportunity.CustomerValue,
                opportunity.CommercialValue,
                opportunity.Status,
                opportunity.Confidence.Value,
                opportunity.CreatedAtUtc))
            .ToList();

        var challenges = ventureCase.Challenges
            .Select(challenge => new BoardChallengeDto(
                challenge.Id,
                challenge.Target,
                challenge.TargetId,
                ResolveTargetText(ventureCase, challenge),
                challenge.Statement,
                challenge.Reasoning,
                challenge.Confidence.Value,
                challenge.CreatedAtUtc))
            .ToList();

        return new BoardDossierDto(
            ventureCase.Id,
            ventureCase.Title,
            ventureCase.Mission,
            observations,
            supportingEvidence,
            contradictingEvidence,
            neutralEvidence,
            unresolvedAssumptions,
            hypotheses,
            opportunities,
            challenges,
            researchQualityFindings,
            redTeamQualityFindings);
    }

    private static List<BoardEvidenceDto> MapEvidence(Case ventureCase, EvidenceDirection direction)
    {
        return ventureCase.Evidence
            .Where(evidence => evidence.Direction == direction)
            .Select(evidence => new BoardEvidenceDto(
                evidence.Id,
                evidence.Summary,
                evidence.Interpretation,
                evidence.Direction,
                evidence.CreatedAtUtc))
            .ToList();
    }

    // Mirrors Case.TargetExists's switch shape, but resolves the target's own text instead of a
    // bool. ChallengeTarget.Decision stays dead here too: Case.TargetExists hard-codes it false,
    // so no Challenge can ever target a Decision today - this branch should never actually be hit.
    private static string ResolveTargetText(Case ventureCase, Challenge challenge)
    {
        return challenge.Target switch
        {
            ChallengeTarget.Evidence => ventureCase.Evidence
                .FirstOrDefault(evidence => evidence.Id == challenge.TargetId)?.Summary
                ?? "(target evidence not found)",
            ChallengeTarget.Assumption => ventureCase.Assumptions
                .FirstOrDefault(assumption => assumption.Id == challenge.TargetId)?.Statement
                ?? "(target assumption not found)",
            ChallengeTarget.Hypothesis => ventureCase.Hypotheses
                .FirstOrDefault(hypothesis => hypothesis.Id == challenge.TargetId)?.Statement
                ?? "(target hypothesis not found)",
            ChallengeTarget.Opportunity => ventureCase.Opportunities
                .FirstOrDefault(opportunity => opportunity.Id == challenge.TargetId)?.Statement
                ?? "(target opportunity not found)",
            ChallengeTarget.Decision => "(Decision targets are not currently supported)",
            _ => "(unknown challenge target)"
        };
    }
}
