using DuckDB.NET.Data;

namespace VentureOS.Infrastructure.Persistence.DuckDb;

public sealed class DuckDbConnectionFactory
{
    private readonly DuckDbOptions _options;

    public DuckDbConnectionFactory(DuckDbOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public DuckDBConnection CreateConnection()
    {
        var directory = Path.GetDirectoryName(_options.DatabasePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return new DuckDBConnection($"Data Source={_options.DatabasePath}");
    }
}