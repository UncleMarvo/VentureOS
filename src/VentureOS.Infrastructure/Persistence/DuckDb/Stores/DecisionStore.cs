using System.Data;
using System.Text.Json;
using VentureOS.Domain.Cases;
using VentureOS.Domain.Common;
using VentureOS.Domain.Decisions;

namespace VentureOS.Infrastructure.Persistence.DuckDb.Stores;

public sealed class DecisionStore
{
    public Task<IReadOnlyCollection<Decision>> LoadAsync(
        IDbConnection connection,
        Guid caseId,
        CancellationToken cancellationToken = default)
    {
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                id,
                case_id,
                question,
                outcome,
                rationale,
                expected_outcome,
                confidence,
                evidence_ids,
                assumption_ids,
                hypothesis_ids,
                challenge_ids,
                created_at_utc
            FROM decisions
            WHERE case_id = $case_id
            ORDER BY created_at_utc;
            """;

        command.AddParameter("case_id", caseId);

        using var reader = command.ExecuteReader();

        var decisions = new List<Decision>();

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

            var hypothesisIds =
                JsonSerializer.Deserialize<List<Guid>>(
                    reader.GetString(reader.GetOrdinal("hypothesis_ids")))
                ?? new List<Guid>();

            var challengeIds =
                JsonSerializer.Deserialize<List<Guid>>(
                    reader.GetString(reader.GetOrdinal("challenge_ids")))
                ?? new List<Guid>();

            decisions.Add(
                Decision.Restore(
                    reader.GetGuid(reader.GetOrdinal("id")),
                    reader.GetGuid(reader.GetOrdinal("case_id")),
                    reader.GetString(reader.GetOrdinal("question")),
                    Enum.Parse<DecisionOutcome>(reader.GetString(reader.GetOrdinal("outcome"))),
                    reader.GetString(reader.GetOrdinal("rationale")),
                    reader.GetString(reader.GetOrdinal("expected_outcome")),
                    Confidence.FromPercentage(int.Parse(reader.GetString(reader.GetOrdinal("confidence")))),
                    evidenceIds,
                    assumptionIds,
                    hypothesisIds,
                    challengeIds,
                    reader.GetDateTime(reader.GetOrdinal("created_at_utc"))));
        }

        return Task.FromResult<IReadOnlyCollection<Decision>>(decisions);
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
        foreach (var decision in ventureCase.Decisions)
        {
            using var command = connection.CreateCommand();

            command.CommandText = """
                INSERT INTO decisions (
                    id,
                    case_id,
                    question,
                    outcome,
                    rationale,
                    expected_outcome,
                    confidence,
                    evidence_ids,
                    assumption_ids,
                    hypothesis_ids,
                    challenge_ids,
                    created_at_utc
                )
                VALUES (
                    $id,
                    $case_id,
                    $question,
                    $outcome,
                    $rationale,
                    $expected_outcome,
                    $confidence,
                    $evidence_ids,
                    $assumption_ids,
                    $hypothesis_ids,
                    $challenge_ids,
                    $created_at_utc
                );
                """;

            command.AddParameter("id", decision.Id);
            command.AddParameter("case_id", decision.CaseId);
            command.AddParameter("question", decision.Question);
            command.AddParameter("outcome", decision.Outcome.ToString());
            command.AddParameter("rationale", decision.Rationale);
            command.AddParameter("expected_outcome", decision.ExpectedOutcome);
            command.AddParameter("confidence", decision.Confidence.Value);
            command.AddParameter("evidence_ids", JsonSerializer.Serialize(decision.EvidenceIds));
            command.AddParameter("assumption_ids", JsonSerializer.Serialize(decision.AssumptionIds));
            command.AddParameter("hypothesis_ids", JsonSerializer.Serialize(decision.HypothesisIds));
            command.AddParameter("challenge_ids", JsonSerializer.Serialize(decision.ChallengeIds));
            command.AddParameter("created_at_utc", decision.CreatedAtUtc);

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
            DELETE FROM decisions
            WHERE case_id = $case_id;
            """;

        command.AddParameter("case_id", caseId);

        command.ExecuteNonQuery();

        return Task.CompletedTask;
    }
}
