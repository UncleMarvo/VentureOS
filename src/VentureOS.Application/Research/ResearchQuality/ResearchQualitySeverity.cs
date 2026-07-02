using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResearchQualitySeverity
{
    Information,
    Warning,
    Error
}
