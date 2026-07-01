using System.Data;
using VentureOS.Domain.Assumptions;
using VentureOS.Domain.Cases;
using VentureOS.Domain.Common;

namespace VentureOS.Infrastructure.Persistence.DuckDb;

public sealed class AssumptionStore
{
    public Task<IReadOnlyCollection<Assumption>> LoadAsync(
        IDbConnection connection,
        Guid caseId,
        CancellationToken cancellationToken = default)
    {
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                id,
                case_id,
                statement,
                rationale,
                confidence,
                status,
                created_at_utc,
                updated_at_utc
            FROM assumptions
            WHERE case_id = $case_id
            ORDER BY created_at_utc;
            """;

        AddParameter(command, "case_id", caseId);

        using var reader = command.ExecuteReader();

        var assumptions = new List<Assumption>();

        while (reader.Read())
        {
            assumptions.Add(
                Assumption.Restore(
                    reader.GetGuid(reader.GetOrdinal("id")),
                    reader.GetGuid(reader.GetOrdinal("case_id")),
                    reader.GetString(reader.GetOrdinal("statement")),
                    reader.GetString(reader.GetOrdinal("rationale")),
                    Confidence.FromPercentage(int.Parse(reader.GetString(reader.GetOrdinal("confidence")))),
                    Enum.Parse<AssumptionStatus>(reader.GetString(reader.GetOrdinal("status"))),
                    reader.GetDateTime(reader.GetOrdinal("created_at_utc")),
                    reader.GetDateTime(reader.GetOrdinal("updated_at_utc"))));
        }

        return Task.FromResult<IReadOnlyCollection<Assumption>>(assumptions);
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
        foreach (var assumption in ventureCase.Assumptions)
        {
            using var command = connection.CreateCommand();

            command.CommandText = """
                INSERT INTO assumptions (
                    id,
                    case_id,
                    statement,
                    rationale,
                    confidence,
                    status,
                    created_at_utc,
                    updated_at_utc
                )
                VALUES (
                    $id,
                    $case_id,
                    $statement,
                    $rationale,
                    $confidence,
                    $status,
                    $created_at_utc,
                    $updated_at_utc
                );
                """;

            AddParameter(command, "id", assumption.Id);
            AddParameter(command, "case_id", assumption.CaseId);
            AddParameter(command, "statement", assumption.Statement);
            AddParameter(command, "rationale", assumption.Rationale);
            AddParameter(command, "confidence", assumption.Confidence.Value);
            AddParameter(command, "status", assumption.Status.ToString());
            AddParameter(command, "created_at_utc", assumption.CreatedAtUtc);
            AddParameter(command, "updated_at_utc", assumption.UpdatedAtUtc);

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
            DELETE FROM assumptions
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
