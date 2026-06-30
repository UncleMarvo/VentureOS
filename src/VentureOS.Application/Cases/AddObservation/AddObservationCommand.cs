using VentureOS.Domain.Common;
using VentureOS.Domain.Observations;

namespace VentureOS.Application.Cases.AddObservation;

public sealed record AddObservationCommand(
    Guid CaseId,
    string ObservationText,
    string Summary,
    string SourceReference,
    ObservationSource ObservationSource,
    Confidence Confidence);