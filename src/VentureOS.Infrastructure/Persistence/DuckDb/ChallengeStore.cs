using System.Data;
using VentureOS.Domain.Cases;
using VentureOS.Domain.Challenges;
using VentureOS.Domain.Common;

namespace VentureOS.Infrastructure.Persistence.DuckDb;

public sealed class ChallengeStore
{
    public Task<IReadOnlyCollection<Challenge>> LoadAsync(
        IDbConnection connection,
        Guid caseId,
        CancellationToken cancellationToken = default)
    {
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                id,
                case_id,
                target,
                target_id,
                statement,
                reasoning,
                confidence,
                created_at_utc
            FROM challenges
            WHERE case_id = $case_id
            ORDER BY created_at_utc;
            """;

        AddParameter(command, "case_id", caseId);

        using var reader = command.ExecuteReader();

        var challenges = new List<Challenge>();

        while (reader.Read())
        {
            challenges.Add(
                Challenge.Restore(
                    reader.GetGuid(reader.GetOrdinal("id")),
                    reader.GetGuid(reader.GetOrdinal("case_id")),
                    Enum.Parse<ChallengeTarget>(reader.GetString(reader.GetOrdinal("target"))),
                    reader.GetGuid(reader.GetOrdinal("target_id")),
                    reader.GetString(reader.GetOrdinal("statement")),
                    reader.GetString(reader.GetOrdinal("reasoning")),
                    Confidence.FromPercentage(int.Parse(reader.GetString(reader.GetOrdinal("confidence")))),
                    reader.GetDateTime(reader.GetOrdinal("created_at_utc"))));
        }

        return Task.FromResult<IReadOnlyCollection<Challenge>>(challenges);
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
        foreach (var challenge in ventureCase.Challenges)
        {
            using var command = connection.CreateCommand();

            command.CommandText = """
                INSERT INTO challenges (
                    id,
                    case_id,
                    target,
                    target_id,
                    statement,
                    reasoning,
                    confidence,
                    created_at_utc
                )
                VALUES (
                    $id,
                    $case_id,
                    $target,
                    $target_id,
                    $statement,
                    $reasoning,
                    $confidence,
                    $created_at_utc
                );
                """;

            AddParameter(command, "id", challenge.Id);
            AddParameter(command, "case_id", challenge.CaseId);
            AddParameter(command, "target", challenge.Target.ToString());
            AddParameter(command, "target_id", challenge.TargetId);
            AddParameter(command, "statement", challenge.Statement);
            AddParameter(command, "reasoning", challenge.Reasoning);
            AddParameter(command, "confidence", challenge.Confidence.Value);
            AddParameter(command, "created_at_utc", challenge.CreatedAtUtc);

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
            DELETE FROM challenges
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
