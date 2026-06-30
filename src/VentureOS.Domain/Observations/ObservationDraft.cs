using System.Security.AccessControl;
using VentureOS.Domain.Common;

namespace VentureOS.Domain.Observations;

public sealed record ObservationDraft(
    string ObservationText,
    string Summary,
    string SourceReference,
    ObservationSource ObservationSource,
    Confidence Confidence);