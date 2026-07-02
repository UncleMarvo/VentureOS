using System.Text.Json.Serialization;

namespace VentureOS.Application.RedTeam.RedTeamQuality;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RedTeamQualitySeverity
{
    Information,
    Warning,
    Error
}
