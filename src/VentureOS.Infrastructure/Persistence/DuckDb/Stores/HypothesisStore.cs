using System.Data;
using System.Text.Json;
using VentureOS.Domain.Cases;
using VentureOS.Domain.Common;
using VentureOS.Domain.Hypotheses;

namespace VentureOS.Infrastructure.Persistence.DuckDb.Stores;

public sealed class HypothesisStore
{
    public Task<IReadOnlyCollection<Hypothesis>> LoadAsync(
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
                reasoning,
                expected_outcome,
                success_criteria,
                confidence,
                evidence_ids,
                assumption_ids,
                status,
                created_at_utc,
                updated_at_utc
            FROM hypotheses
            WHERE case_id = $case_id
            ORDER BY created_at_utc;
            """;

        command.AddParameter("case_id", caseId);

        using var reader = command.ExecuteReader();

        var hypotheses = new List<Hypothesis>();

        while (reader.Read())
        {
            var evidenceIds =
                JsonSerializer.Deserialize<List<Guid>>(
                    reader.GetString(reader.GetOrdinal("evidence_ids")))
                ?? new List<Guid>();

            var assumptionIds =
                JsonSerializer.Deserialize<List<Guid>>(
                    reader.GetString(reader.GetOrdinal("assumption_ids")))
                ?? new List<Guid>();

            hypotheses.Add(
                Hypothesis.Restore(
                    reader.GetGuid(reader.GetOrdinal("id")),
                    reader.GetGuid(reader.GetOrdinal("case_id")),
                    reader.GetString(reader.GetOrdinal("statement")),
                    reader.GetString(reader.GetOrdinal("reasoning")),
                    reader.GetString(reader.GetOrdinal("expected_outcome")),
                    reader.GetString(reader.GetOrdinal("success_criteria")),
                    Confidence.FromPercentage(int.Parse(reader.GetString(reader.GetOrdinal("confidence")))),
                    evidenceIds,
                    assumptionIds,
                    Enum.Parse<HypothesisStatus>(reader.GetString(reader.GetOrdinal("status"))),
                    reader.GetDateTime(reader.GetOrdinal("created_at_utc")),
                    reader.GetDateTime(reader.GetOrdinal("updated_at_utc"))));
        }

        return Task.FromResult<IReadOnlyCollection<Hypothesis>>(hypotheses);
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
        foreach (var hypothesis in ventureCase.Hypotheses)
        {
            using var command = connection.CreateCommand();

            command.CommandText = """
                INSERT INTO hypotheses (
                    id,
                    case_id,
                    statement,
                    reasoning,
                    expected_outcome,
                    success_criteria,
                    confidence,
                    evidence_ids,
                    assumption_ids,
                    status,
                    created_at_utc,
                    updated_at_utc
                )
                VALUES (
                    $id,
                    $case_id,
                    $statement,
                    $reasoning,
                    $expected_outcome,
                    $success_criteria,
                    $confidence,
                    $evidence_ids,
                    $assumption_ids,
                    $status,
                    $created_at_utc,
                    $updated_at_utc
                );
                """;

            command.AddParameter("id", hypothesis.Id);
            command.AddParameter("case_id", hypothesis.CaseId);
            command.AddParameter("statement", hypothesis.Statement);
            command.AddParameter("reasoning", hypothesis.Reasoning);
            command.AddParameter("expected_outcome", hypothesis.ExpectedOutcome);
            command.AddParameter("success_criteria", hypothesis.SuccessCriteria);
            command.AddParameter("confidence", hypothesis.Confidence.Value);
            command.AddParameter("evidence_ids", JsonSerializer.Serialize(hypothesis.EvidenceIds));
            command.AddParameter("assumption_ids", JsonSerializer.Serialize(hypothesis.AssumptionIds));
            command.AddParameter("status", hypothesis.Status.ToString());
            command.AddParameter("created_at_utc", hypothesis.CreatedAtUtc);
            command.AddParameter("updated_at_utc", hypothesis.UpdatedAtUtc);

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
            DELETE FROM hypotheses
            WHERE case_id = $case_id;
            """;

        command.AddParameter("case_id", caseId);

        command.ExecuteNonQuery();

        return Task.CompletedTask;
    }
}
