using System.Data;
using VentureOS.Application.Cases;
using VentureOS.Domain.Cases;

namespace VentureOS.Infrastructure.Persistence.DuckDb;

public sealed class DuckDbCaseRepository : ICaseRepository
{
    private readonly DuckDbConnectionFactory _connectionFactory;

    public DuckDbCaseRepository(DuckDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task AddAsync(
        Case ventureCase,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ventureCase);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO cases (
                id,
                title,
                mission,
                status,
                created_at_utc,
                updated_at_utc
            )
            VALUES (
                $id,
                $title,
                $mission,
                $status,
                $created_at_utc,
                $updated_at_utc
            );
            """;

        AddParameter(command, "id", ventureCase.Id);
        AddParameter(command, "title", ventureCase.Title);
        AddParameter(command, "mission", ventureCase.Mission);
        AddParameter(command, "status", ventureCase.Status.ToString());
        AddParameter(command, "created_at_utc", ventureCase.CreatedAtUtc);
        AddParameter(command, "updated_at_utc", ventureCase.UpdatedAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Case?> GetByIdAsync(
        Guid caseId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                id,
                title,
                mission,
                status,
                created_at_utc,
                updated_at_utc
            FROM cases
            WHERE id = $id;
            """;

        AddParameter(command, "id", caseId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return Case.Restore(
            reader.GetGuid(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("title")),
            reader.GetString(reader.GetOrdinal("mission")),
            Enum.Parse<CaseStatus>(reader.GetString(reader.GetOrdinal("status"))),
            reader.GetDateTime(reader.GetOrdinal("created_at_utc")),
            reader.GetDateTime(reader.GetOrdinal("updated_at_utc")),
            Array.Empty<VentureOS.Domain.Observations.Observation>(),
            Array.Empty<VentureOS.Domain.Evidence.Evidence>(),
            Array.Empty<VentureOS.Domain.Assumptions.Assumption>(),
            Array.Empty<VentureOS.Domain.Hypotheses.Hypothesis>(),
            Array.Empty<VentureOS.Domain.Challenges.Challenge>(),
            Array.Empty<VentureOS.Domain.Decisions.Decision>(),
            Array.Empty<VentureOS.Domain.Lessons.Lesson>());
    }

    public async Task UpdateAsync(
        Case ventureCase,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ventureCase);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE cases
            SET
                title = $title,
                mission = $mission,
                status = $status,
                updated_at_utc = $updated_at_utc
            WHERE id = $id;
            """;

        AddParameter(command, "id", ventureCase.Id);
        AddParameter(command, "title", ventureCase.Title);
        AddParameter(command, "mission", ventureCase.Mission);
        AddParameter(command, "status", ventureCase.Status.ToString());
        AddParameter(command, "updated_at_utc", ventureCase.UpdatedAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddParameter(
        IDbCommand command,
        string name,
        object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}