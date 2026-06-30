namespace VentureOS.Application.Cases.AddObservation;

public sealed record AddObservationResult(
    Guid CaseId,
    Guid ObservationId,
    string Summary);