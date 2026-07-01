using System.Data;
using VentureOS.Domain.Cases;
using VentureOS.Domain.Common;
using VentureOS.Domain.Observations;

namespace VentureOS.Infrastructure.Persistence.DuckDb.Stores;

public sealed class ObservationStore
{
    public Task<IReadOnlyCollection<Observation>> LoadAsync(
        IDbConnection connection,
        Guid caseId,
        CancellationToken cancellationToken = default)
    {
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                id,
                case_id,
                observation_text,
                summary,
                source_reference,
                observation_source,
                confidence,
                created_at_utc
            FROM observations
            WHERE case_id = $case_id
            ORDER BY created_at_utc;
            """;

        command.AddParameter("case_id", caseId);

        using var reader = command.ExecuteReader();

        var observations = new List<Observation>();

        while (reader.Read())
        {
            observations.Add(
                Observation.Restore(
                    reader.GetGuid(reader.GetOrdinal("id")),
                    reader.GetGuid(reader.GetOrdinal("case_id")),
                    reader.GetString(reader.GetOrdinal("observation_text")),
                    reader.GetString(reader.GetOrdinal("summary")),
                    reader.GetString(reader.GetOrdinal("source_reference")),
                    Enum.Parse<ObservationSource>(reader.GetString(reader.GetOrdinal("observation_source"))),
                    Confidence.FromPercentage(int.Parse(reader.GetString(reader.GetOrdinal("confidence")))),
                    reader.GetDateTime(reader.GetOrdinal("created_at_utc"))));
        }

        return Task.FromResult<IReadOnlyCollection<Observation>>(observations);
    }

    public async Task ReplaceAsync(
        IDbConnection connection,
        Case ventureCase,
        CancellationToken cancellationToken = default)
    {
        await DeleteAsync(connection, ventureCase.Id, cancellationToken);
        await InsertAsync(connection, ventureCase, cancellationToken);
    }

    public Task InsertAsync(
        IDbConnection connection,
        Case ventureCase,
        CancellationToken cancellationToken = default)
    {
        foreach (var observation in ventureCase.Observations)
        {
            using var command = connection.CreateCommand();

            command.CommandText = """
                INSERT INTO observations (
                    id,
                    case_id,
                    observation_text,
                    summary,
                    source_reference,
                    observation_source,
                    confidence,
                    created_at_utc
                )
                VALUES (
                    $id,
                    $case_id,
                    $observation_text,
                    $summary,
                    $source_reference,
                    $observation_source,
                    $confidence,
                    $created_at_utc
                );
                """;

            command.AddParameter("id", observation.Id);
            command.AddParameter("case_id", observation.CaseId);
            command.AddParameter("observation_text", observation.ObservationText);
            command.AddParameter("summary", observation.Summary);
            command.AddParameter("source_reference", observation.SourceReference);
            command.AddParameter("observation_source", observation.ObservationSource.ToString());
            command.AddParameter("confidence", observation.Confidence.Value);
            command.AddParameter("created_at_utc", observation.CreatedAtUtc);

            command.ExecuteNonQuery();
        }

        return Task.CompletedTask;
    }

    private static Task DeleteAsync(
        IDbConnection connection,
        Guid caseId,
        CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();

        command.CommandText = """
            DELETE FROM observations
            WHERE case_id = $case_id;
            """;

        command.AddParameter("case_id", caseId);

        command.ExecuteNonQuery();

        return Task.CompletedTask;
    }
}
