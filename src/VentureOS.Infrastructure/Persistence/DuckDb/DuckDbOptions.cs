namespace VentureOS.Infrastructure.Persistence.DuckDb;

public sealed class DuckDbOptions
{
    public string DatabasePath { get; init; } = "data/ventureos.duckdb";
}