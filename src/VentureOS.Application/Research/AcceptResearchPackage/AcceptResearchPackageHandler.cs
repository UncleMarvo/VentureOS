using VentureOS.Application.Cases.AddObservation;
using VentureOS.Application.Cases.CreateAssumption;
using VentureOS.Application.Cases.CreateEvidence;
using VentureOS.Application.Cases.CreateHypothesis;
using VentureOS.Application.Cases.CreateOpportunity;
using VentureOS.Application.Cases.RaiseChallenge;
using VentureOS.Domain.Challenges;
using VentureOS.Domain.Common;
using VentureOS.Domain.Evidence;
using VentureOS.Domain.Observations;

public sealed class AcceptResearchPackageHandler
{
    private readonly AddObservationHandler _addObservationHandler;
    private readonly CreateEvidenceHandler _createEvidenceHandler;
    private readonly CreateAssumptionHandler _createAssumptionHandler;
    private readonly CreateOpportunityHandler _createOpportunityHandler;
    private readonly CreateHypothesisHandler _createHypothesisHandler;
    private readonly RaiseChallengeHandler _raiseChallengeHandler;

    public AcceptResearchPackageHandler(
        AddObservationHandler addObservationHandler,
        CreateEvidenceHandler createEvidenceHandler,
        CreateAssumptionHandler createAssumptionHandler,
        CreateOpportunityHandler createOpportunityHandler,
        CreateHypothesisHandler createHypothesisHandler,
        RaiseChallengeHandler raiseChallengeHandler)
    {
        _addObservationHandler = addObservationHandler;
        _createEvidenceHandler = createEvidenceHandler;
        _createAssumptionHandler = createAssumptionHandler;
        _createOpportunityHandler = createOpportunityHandler;
        _createHypothesisHandler = createHypothesisHandler;
        _raiseChallengeHandler = raiseChallengeHandler;
    }

    public async Task<Result<AcceptedResearchPackageDto>> HandleAsync(
        AcceptResearchPackageCommand command,
        CancellationToken cancellationToken = default)
    {
        // =====================
        // OBSERVATION
        // =====================
        var observationIdsByIndex = new Dictionary<int, Guid>();
        for (var index = 0; index < command.ResearchPackage.Observations.Count; index++)
        {
            var proposedObservation = command.ResearchPackage.Observations[index];

            var result = await _addObservationHandler.HandleAsync(
                new AddObservationCommand(
                    command.CaseId,
                    proposedObservation.ObservationText,
                    proposedObservation.Summary,
                    proposedObservation.SourceReference,
                    ObservationSource.AiGeneratedResearch,
                    Confidence.FromPercentage(proposedObservation.Confidence)),
                cancellationToken);

            if (result.IsFailure)
            {
                return Result<AcceptedResearchPackageDto>.Failure(
                    result.Error ?? "Failed to create observation.");
            }

            observationIdsByIndex[index] = result.Value.ObservationId;
        }

        // =====================
        // EVIDENCE
        // =====================
        var evidenceIdsByIndex = new Dictionary<int, Guid>();
        for (var index = 0; index < command.ResearchPackage.Evidence.Count; index++)
        {
            var proposedEvidence = command.ResearchPackage.Evidence[index];

            var observationIds = proposedEvidence.ObservationIndexes
                .Select(index => observationIdsByIndex[index])
                .ToList();

            var result = await _createEvidenceHandler.HandleAsync(
                new CreateEvidenceCommand(
                    command.CaseId,
                    proposedEvidence.Summary,
                    proposedEvidence.Interpretation,
                    (EvidenceDirection)proposedEvidence.Direction,
                    observationIds),
                cancellationToken);

            if (result.IsFailure)
            {
                return Result<AcceptedResearchPackageDto>.Failure(
                    result.Error ?? "Failed to create evidence.");
            }

            evidenceIdsByIndex[index] = result.Value.EvidenceId;
        }

        // =====================
        // ASSUMPTION
        // =====================
        var assumptionIdsByIndex = new Dictionary<int, Guid>();
        for (var index = 0; index < command.ResearchPackage.Assumptions.Count; index++)
        {
            var proposedAssumption = command.ResearchPackage.Assumptions[index];

            var result = await _createAssumptionHandler.HandleAsync(
                new CreateAssumptionCommand(
                    command.CaseId,
                    proposedAssumption.Statement,
                    proposedAssumption.Rationale,
                    Confidence.FromPercentage(proposedAssumption.Confidence)),
                cancellationToken);

            if (result.IsFailure)
            {
                return Result<AcceptedResearchPackageDto>.Failure(
                    result.Error ?? "Failed to create assumption.");
            }

            assumptionIdsByIndex[index] = result.Value.AssumptionId;
        }

        // =====================
        // OPPORTUNITY
        // =====================
        var opportunityIdsByIndex = new Dictionary<int, Guid>();
        for (var index = 0; index < command.ResearchPackage.Opportunities.Count; index++)
        {
            var proposedOpportunity = command.ResearchPackage.Opportunities[index];

            var opportunityEvidenceIds = proposedOpportunity.EvidenceIndexes
                .Select(evidenceIndex => evidenceIdsByIndex[evidenceIndex])
                .ToList();

            var opportunityAssumptionIds = proposedOpportunity.AssumptionIndexes
                .Select(assumptionIndex => assumptionIdsByIndex[assumptionIndex])
                .ToList();

            var result = await _createOpportunityHandler.HandleAsync(
                new CreateOpportunityCommand(
                    command.CaseId,
                    proposedOpportunity.Statement,
                    proposedOpportunity.CustomerValue,
                    proposedOpportunity.CommercialValue,
                    proposedOpportunity.Differentiation,
                    proposedOpportunity.Timing,
                    Confidence.FromPercentage(proposedOpportunity.Confidence),
                    opportunityEvidenceIds,
                    opportunityAssumptionIds),
                cancellationToken);

            if (result.IsFailure)
            {
                return Result<AcceptedResearchPackageDto>.Failure(
                    result.Error ?? "Failed to create opportunity.");
            }

            opportunityIdsByIndex[index] = result.Value.OpportunityId;
        }

        // =====================
        // HYPOTHESIS
        // =====================
        var hypothesisIdsByIndex = new Dictionary<int, Guid>();
        for (var index = 0; index < command.ResearchPackage.Hypotheses.Count; index++)
        {
            var proposedHypothesis = command.ResearchPackage.Hypotheses[index];

            var evidenceIds = proposedHypothesis.EvidenceIndexes
                .Select(evidenceIndex => evidenceIdsByIndex[evidenceIndex])
                .ToList();

            var assumptionIds = proposedHypothesis.AssumptionIndexes
                .Select(assumptionIndex => assumptionIdsByIndex[assumptionIndex])
                .ToList();

            var result = await _createHypothesisHandler.HandleAsync(
                new CreateHypothesisCommand(
                    command.CaseId,
                    proposedHypothesis.Statement,
                    proposedHypothesis.Reasoning,
                    proposedHypothesis.ExpectedOutcome,
                    proposedHypothesis.SuccessCriteria,
                    Confidence.FromPercentage(proposedHypothesis.Confidence),
                    evidenceIds,
                    assumptionIds),
                cancellationToken);

            if (result.IsFailure)
            {
                return Result<AcceptedResearchPackageDto>.Failure(
                    result.Error ?? "Failed to create hypothesis.");
            }

            hypothesisIdsByIndex[index] = result.Value.HypothesisId;
        }

        // =====================
        // CHALLENGE
        // =====================
        var challengeIdsByIndex = new Dictionary<int, Guid>();
        for (var index = 0; index < command.ResearchPackage.Challenges.Count; index++)
        {
            var proposedChallenge = command.ResearchPackage.Challenges[index];

            var target = proposedChallenge.TargetType switch
            {
                "Evidence" => ChallengeTarget.Evidence,
                "Assumption" => ChallengeTarget.Assumption,
                "Hypothesis" => ChallengeTarget.Hypothesis,
                "Opportunity" => ChallengeTarget.Opportunity,
                _ => throw new InvalidOperationException(
                    $"Unsupported challenge target type: {proposedChallenge.TargetType}")
            };

            var targetId = target switch
            {
                ChallengeTarget.Evidence => evidenceIdsByIndex[proposedChallenge.TargetIndex],
                ChallengeTarget.Assumption => assumptionIdsByIndex[proposedChallenge.TargetIndex],
                ChallengeTarget.Hypothesis => hypothesisIdsByIndex[proposedChallenge.TargetIndex],
                ChallengeTarget.Opportunity => opportunityIdsByIndex[proposedChallenge.TargetIndex],
                _ => throw new InvalidOperationException(
                    $"Unsupported challenge target: {target}")
            };

            var result = await _raiseChallengeHandler.HandleAsync(
                new RaiseChallengeCommand(
                    command.CaseId,
                    target,
                    targetId,
                    proposedChallenge.Statement,
                    proposedChallenge.Reasoning,
                    Confidence.FromPercentage(proposedChallenge.Confidence)),
                cancellationToken);

            if (result.IsFailure)
            {
                return Result<AcceptedResearchPackageDto>.Failure(
                    result.Error ?? "Failed to create challenge.");
            }

            challengeIdsByIndex[index] = result.Value.ChallengeId;
        }

        var dto = new AcceptedResearchPackageDto(
            command.CaseId,
            observationIdsByIndex.Count,
            evidenceIdsByIndex.Count,
            assumptionIdsByIndex.Count,
            opportunityIdsByIndex.Count,
            hypothesisIdsByIndex.Count,
            challengeIdsByIndex.Count);

        return Result<AcceptedResearchPackageDto>.Success(dto);
    }
}
