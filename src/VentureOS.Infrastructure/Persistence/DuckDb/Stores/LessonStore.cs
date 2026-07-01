using System.Data;
using System.Text.Json;
using VentureOS.Domain.Cases;
using VentureOS.Domain.Common;
using VentureOS.Domain.Lessons;

namespace VentureOS.Infrastructure.Persistence.DuckDb.Stores;

public sealed class LessonStore
{
    public Task<IReadOnlyCollection<Lesson>> LoadAsync(
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
                detail,
                confidence,
                decision_ids,
                created_at_utc
            FROM lessons
            WHERE case_id = $case_id
            ORDER BY created_at_utc;
            """;

        command.AddParameter("case_id", caseId);

        using var reader = command.ExecuteReader();

        var lessons = new List<Lesson>();

        while (reader.Read())
        {
            var decisionIds =
                JsonSerializer.Deserialize<List<Guid>>(
                    reader.GetString(reader.GetOrdinal("decision_ids")))
                ?? new List<Guid>();

            lessons.Add(
                Lesson.Restore(
                    reader.GetGuid(reader.GetOrdinal("id")),
                    reader.GetGuid(reader.GetOrdinal("case_id")),
                    reader.GetString(reader.GetOrdinal("summary")),
                    reader.GetString(reader.GetOrdinal("detail")),
                    Confidence.FromPercentage(int.Parse(reader.GetString(reader.GetOrdinal("confidence")))),
                    decisionIds,
                    reader.GetDateTime(reader.GetOrdinal("created_at_utc"))));
        }

        return Task.FromResult<IReadOnlyCollection<Lesson>>(lessons);
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
        foreach (var lesson in ventureCase.Lessons)
        {
            using var command = connection.CreateCommand();

            command.CommandText = """
                INSERT INTO lessons (
                    id,
                    case_id,
                    summary,
                    detail,
                    confidence,
                    decision_ids,
                    created_at_utc
                )
                VALUES (
                    $id,
                    $case_id,
                    $summary,
                    $detail,
                    $confidence,
                    $decision_ids,
                    $created_at_utc
                );
                """;

            command.AddParameter("id", lesson.Id);
            command.AddParameter("case_id", lesson.CaseId);
            command.AddParameter("summary", lesson.Summary);
            command.AddParameter("detail", lesson.Detail);
            command.AddParameter("confidence", lesson.Confidence.Value);
            command.AddParameter("decision_ids", JsonSerializer.Serialize(lesson.DecisionIds));
            command.AddParameter("created_at_utc", lesson.CreatedAtUtc);

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
            DELETE FROM lessons
            WHERE case_id = $case_id;
            """;

        command.AddParameter("case_id", caseId);

        command.ExecuteNonQuery();

        return Task.CompletedTask;
    }
}
