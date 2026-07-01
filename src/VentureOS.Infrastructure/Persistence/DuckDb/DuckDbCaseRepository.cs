using System.Data;
using VentureOS.Application.Cases;
using VentureOS.Domain.Cases;

namespace VentureOS.Infrastructure.Persistence.DuckDb;

public sealed class DuckDbCaseRepository : ICaseRepository
{
    private readonly DuckDbConnectionFactory _connectionFactory;

    private readonly ObservationStore _observationStore;
    private readonly EvidenceStore _evidenceStore;
    private readonly AssumptionStore _assumptionStore;
    private readonly HypothesisStore _hypothesisStore;
    private readonly ChallengeStore _challengeStore;
    private readonly DecisionStore _decisionStore;
    private readonly LessonStore _lessonStore;

    public DuckDbCaseRepository(
        DuckDbConnectionFactory connectionFactory,
        ObservationStore observationStore,
        EvidenceStore evidenceStore,
        AssumptionStore assumptionStore,
        HypothesisStore hypothesisStore,
        ChallengeStore challengeStore,
        DecisionStore decisionStore,
        LessonStore lessonStore)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _observationStore = observationStore ?? throw new ArgumentNullException(nameof(observationStore));
        _evidenceStore = evidenceStore ?? throw new ArgumentNullException(nameof(evidenceStore));
        _assumptionStore = assumptionStore ?? throw new ArgumentNullException(nameof(assumptionStore));
        _hypothesisStore = hypothesisStore ?? throw new ArgumentNullException(nameof(hypothesisStore));
        _challengeStore = challengeStore ?? throw new ArgumentNullException(nameof(challengeStore));
        _decisionStore = decisionStore ?? throw new ArgumentNullException(nameof(decisionStore));
        _lessonStore = lessonStore ?? throw new ArgumentNullException(nameof(lessonStore));
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

        // ==========================================
        // OBSERVATION
        // ==========================================
        await _observationStore.InsertAsync(
            connection,
            ventureCase,
            cancellationToken);

        // ==========================================
        // EVIDENCE
        // ==========================================
        await _evidenceStore.InsertAsync(
            connection,
            ventureCase,
            cancellationToken);

        // ==========================================
        // ASSUMPTION
        // ==========================================
        await _assumptionStore.InsertAsync(
            connection,
            ventureCase,
            cancellationToken);

        // ==========================================
        // HYPOTHESIS
        // ==========================================
        await _hypothesisStore.InsertAsync(
            connection,
            ventureCase,
            cancellationToken);

        // ==========================================
        // CHALLENGE
        // ==========================================
        await _challengeStore.InsertAsync(
            connection,
            ventureCase,
            cancellationToken);

        // ==========================================
        // DECISION
        // ==========================================
        await _decisionStore.InsertAsync(
            connection,
            ventureCase,
            cancellationToken);

        // ==========================================
        // LESSON
        // ==========================================
        await _lessonStore.InsertAsync(
            connection,
            ventureCase,
            cancellationToken);
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

        // ==========================================
        // OBSERVATION
        // ==========================================
        var observations = await _observationStore.LoadAsync(
            connection,
            caseId,
            cancellationToken);

        // ==========================================
        // EVIDENCE
        // ==========================================
        var evidence = await _evidenceStore.LoadAsync(
            connection,
            caseId,
            cancellationToken);

        // ==========================================
        // ASSUMPTION
        // ==========================================
        var assumptions = await _assumptionStore.LoadAsync(
            connection,
            caseId,
            cancellationToken);

        // ==========================================
        // HYPOTHESIS
        // ==========================================
        var hypotheses = await _hypothesisStore.LoadAsync(
            connection,
            caseId,
            cancellationToken);

        // ==========================================
        // CHALLENGE
        // ==========================================
        var challenges = await _challengeStore.LoadAsync(
            connection,
            caseId,
            cancellationToken);

        // ==========================================
        // DECISION
        // ==========================================
        var decisions = await _decisionStore.LoadAsync(
            connection,
            caseId,
            cancellationToken);

        // ==========================================
        // LESSON
        // ==========================================
        var lessons = await _lessonStore.LoadAsync(
            connection,
            caseId,
            cancellationToken);

        return Case.Restore(
            reader.GetGuid(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("title")),
            reader.GetString(reader.GetOrdinal("mission")),
            Enum.Parse<CaseStatus>(reader.GetString(reader.GetOrdinal("status"))),
            reader.GetDateTime(reader.GetOrdinal("created_at_utc")),
            reader.GetDateTime(reader.GetOrdinal("updated_at_utc")),
            observations,
            evidence,
            assumptions,
            hypotheses,
            challenges,
            decisions,
            lessons);
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

        // ==========================================
        // OBSERVATION
        // ==========================================
        await _observationStore.ReplaceAsync(
            connection,
            ventureCase,
            cancellationToken);

        // ==========================================
        // EVIDENCE
        // ==========================================
        await _evidenceStore.ReplaceAsync(
            connection,
            ventureCase,
            cancellationToken);

        // ==========================================
        // ASSUMPTION
        // ==========================================
        await _assumptionStore.ReplaceAsync(
            connection,
            ventureCase,
            cancellationToken);

        // ==========================================
        // HYPOTHESIS
        // ==========================================
        await _hypothesisStore.ReplaceAsync(
            connection,
            ventureCase,
            cancellationToken);

        // ==========================================
        // CHALLENGE
        // ==========================================
        await _challengeStore.ReplaceAsync(
            connection,
            ventureCase,
            cancellationToken);

        // ==========================================
        // DECISION
        // ==========================================
        await _decisionStore.ReplaceAsync(
            connection,
            ventureCase,
            cancellationToken);

        // ==========================================
        // LESSON
        // ==========================================
        await _lessonStore.ReplaceAsync(
            connection,
            ventureCase,
            cancellationToken);
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
