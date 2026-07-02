using System.Data;
using System.Text.Json;
using VentureOS.Domain.Cases;
using VentureOS.Domain.Common;
using VentureOS.Domain.Opportunities;

namespace VentureOS.Infrastructure.Persistence.DuckDb.Stores;

public sealed class OpportunityStore
{
    public Task<IReadOnlyCollection<Opportunity>> LoadAsync(
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
                customer_value,
                commercial_value,
                differentiation,
                timing,
                confidence,
                evidence_ids,
                assumption_ids,
                status,
                created_at_utc,
                updated_at_utc
            FROM opportunities
            WHERE case_id = $case_id
            ORDER BY created_at_utc;
            """;

        command.AddParameter("case_id", caseId);

        using var reader = command.ExecuteReader();

        var opportunities = new List<Opportunity>();

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

            opportunities.Add(
                Opportunity.Restore(
                    reader.GetGuid(reader.GetOrdinal("id")),
                    reader.GetGuid(reader.GetOrdinal("case_id")),
                    reader.GetString(reader.GetOrdinal("statement")),
                    reader.GetString(reader.GetOrdinal("customer_value")),
                    reader.GetString(reader.GetOrdinal("commercial_value")),
                    reader.GetString(reader.GetOrdinal("differentiation")),
                    reader.GetString(reader.GetOrdinal("timing")),
                    Confidence.FromPercentage(int.Parse(reader.GetString(reader.GetOrdinal("confidence")))),
                    evidenceIds,
                    assumptionIds,
                    Enum.Parse<OpportunityStatus>(reader.GetString(reader.GetOrdinal("status"))),
                    reader.GetDateTime(reader.GetOrdinal("created_at_utc")),
                    reader.GetDateTime(reader.GetOrdinal("updated_at_utc"))));
        }

        return Task.FromResult<IReadOnlyCollection<Opportunity>>(opportunities);
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
        foreach (var opportunity in ventureCase.Opportunities)
        {
            using var command = connection.CreateCommand();

            command.CommandText = """
                INSERT INTO opportunities (
                    id,
                    case_id,
                    statement,
                    customer_value,
                    commercial_value,
                    differentiation,
                    timing,
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
                    $customer_value,
                    $commercial_value,
                    $differentiation,
                    $timing,
                    $confidence,
                    $evidence_ids,
                    $assumption_ids,
                    $status,
                    $created_at_utc,
                    $updated_at_utc
                );
                """;

            command.AddParameter("id", opportunity.Id);
            command.AddParameter("case_id", opportunity.CaseId);
            command.AddParameter("statement", opportunity.Statement);
            command.AddParameter("customer_value", opportunity.CustomerValue);
            command.AddParameter("commercial_value", opportunity.CommercialValue);
            command.AddParameter("differentiation", opportunity.Differentiation);
            command.AddParameter("timing", opportunity.Timing);
            command.AddParameter("confidence", opportunity.Confidence.Value);
            command.AddParameter("evidence_ids", JsonSerializer.Serialize(opportunity.EvidenceIds));
            command.AddParameter("assumption_ids", JsonSerializer.Serialize(opportunity.AssumptionIds));
            command.AddParameter("status", opportunity.Status.ToString());
            command.AddParameter("created_at_utc", opportunity.CreatedAtUtc);
            command.AddParameter("updated_at_utc", opportunity.UpdatedAtUtc);

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
            DELETE FROM opportunities
            WHERE case_id = $case_id;
            """;

        command.AddParameter("case_id", caseId);

        command.ExecuteNonQuery();

        return Task.CompletedTask;
    }
}
