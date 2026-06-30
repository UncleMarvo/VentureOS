using System.Data;
using System.Text.Json;
using VentureOS.Domain.Cases;
using VentureOS.Domain.Evidence;

namespace VentureOS.Infrastructure.Persistence.DuckDb;

public sealed class EvidenceStore
{
    public Task<IReadOnlyCollection<Evidence>> LoadAsync(
        IDbConnection connection,
        Guid caseId,
        CancellationToken cancellationToken = default)
    {
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                id,
                case_id,
                summary,
                interpretation,
                direction,
                observation_ids,
                created_at_utc
            FROM evidence
            WHERE case_id = $case_id
            ORDER BY created_at_utc;
            """;

        AddParameter(command, "case_id", caseId);

        using var reader = command.ExecuteReader();

        var evidenceItems = new List<Evidence>();

        while (reader.Read())
        {
            var observationIdsJson =
                reader.GetString(reader.GetOrdinal("observation_ids"));

            var observationIds =
                JsonSerializer.Deserialize<List<Guid>>(observationIdsJson)
                ?? new List<Guid>();

            evidenceItems.Add(
                Evidence.Restore(
                    reader.GetGuid(reader.GetOrdinal("id")),
                    reader.GetGuid(reader.GetOrdinal("case_id")),
                    reader.GetString(reader.GetOrdinal("summary")),
                    reader.GetString(reader.GetOrdinal("interpretation")),
                    Enum.Parse<EvidenceDirection>(
                        reader.GetString(reader.GetOrdinal("direction"))),
                    observationIds,
                    reader.GetDateTime(reader.GetOrdinal("created_at_utc"))));
        }

        return Task.FromResult<IReadOnlyCollection<Evidence>>(evidenceItems);
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
        foreach (var evidence in ventureCase.Evidence)
        {
            using var command = connection.CreateCommand();

            command.CommandText = """
                INSERT INTO evidence (
                    id,
                    case_id,
                    summary,
                    interpretation,
                    direction,
                    observation_ids,
                    created_at_utc
                )
                VALUES (
                    $id,
                    $case_id,
                    $summary,
                    $interpretation,
                    $direction,
                    $observation_ids,
                    $created_at_utc
                );
                """;

            AddParameter(command, "id", evidence.Id);
            AddParameter(command, "case_id", evidence.CaseId);
            AddParameter(command, "summary", evidence.Summary);
            AddParameter(command, "interpretation", evidence.Interpretation);
            AddParameter(command, "direction", evidence.Direction.ToString());
            AddParameter(
                command,
                "observation_ids",
                JsonSerializer.Serialize(evidence.ObservationIds));
            AddParameter(command, "created_at_utc", evidence.CreatedAtUtc);

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
            DELETE FROM evidence
            WHERE case_id = $case_id;
            """;

        AddParameter(command, "case_id", caseId);

        command.ExecuteNonQuery();

        return Task.CompletedTask;
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