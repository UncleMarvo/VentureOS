namespace VentureOS.Infrastructure.AI.Ollama;

public sealed class OllamaOptions
{
    public string BaseUrl { get; init; } = "http://localhost:11434";
    public string Model { get; init; } = "llama3.3:latest";
}
