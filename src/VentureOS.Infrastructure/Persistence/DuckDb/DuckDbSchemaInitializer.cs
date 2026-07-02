namespace VentureOS.Infrastructure.Persistence.DuckDb;

public sealed class DuckDbSchemaInitializer
{
    private readonly DuckDbConnectionFactory _connectionFactory;

    public DuckDbSchemaInitializer(DuckDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = """
            CREATE TABLE IF NOT EXISTS cases (
                id UUID PRIMARY KEY,
                title VARCHAR NOT NULL,
                mission VARCHAR NOT NULL,
                status VARCHAR NOT NULL,
                created_at_utc TIMESTAMP NOT NULL,
                updated_at_utc TIMESTAMP NOT NULL
            );

            CREATE TABLE IF NOT EXISTS observations (
                id UUID PRIMARY KEY,
                case_id UUID NOT NULL,
                observation_text VARCHAR NOT NULL,
                summary VARCHAR NOT NULL,
                source_reference VARCHAR NOT NULL,
                observation_source VARCHAR NOT NULL,
                confidence VARCHAR NOT NULL,
                created_at_utc TIMESTAMP NOT NULL
            );

            CREATE TABLE IF NOT EXISTS evidence (
                id UUID PRIMARY KEY,
                case_id UUID NOT NULL,
                summary VARCHAR NOT NULL,
                interpretation VARCHAR NOT NULL,
                direction VARCHAR NOT NULL,
                observation_ids VARCHAR NOT NULL,
                created_at_utc TIMESTAMP NOT NULL
            );

            CREATE TABLE IF NOT EXISTS assumptions (
                id UUID PRIMARY KEY,
                case_id UUID NOT NULL,
                statement VARCHAR NOT NULL,
                rationale VARCHAR NOT NULL,
                confidence VARCHAR NOT NULL,
                status VARCHAR NOT NULL,
                created_at_utc TIMESTAMP NOT NULL,
                updated_at_utc TIMESTAMP NOT NULL
            );

            CREATE TABLE IF NOT EXISTS opportunities (
                id UUID PRIMARY KEY,
                case_id UUID NOT NULL,
                statement VARCHAR NOT NULL,
                customer_value VARCHAR NOT NULL,
                commercial_value VARCHAR NOT NULL,
                differentiation VARCHAR NOT NULL,
                timing VARCHAR NOT NULL,
                confidence VARCHAR NOT NULL,
                evidence_ids VARCHAR NOT NULL,
                assumption_ids VARCHAR NOT NULL,
                status VARCHAR NOT NULL,
                created_at_utc TIMESTAMP NOT NULL,
                updated_at_utc TIMESTAMP NOT NULL
            );

            CREATE TABLE IF NOT EXISTS hypotheses (
                id UUID PRIMARY KEY,
                case_id UUID NOT NULL,
                statement VARCHAR NOT NULL,
                reasoning VARCHAR NOT NULL,
                expected_outcome VARCHAR NOT NULL,
                success_criteria VARCHAR NOT NULL,
                confidence VARCHAR NOT NULL,
                evidence_ids VARCHAR NOT NULL,
                assumption_ids VARCHAR NOT NULL,
                status VARCHAR NOT NULL,
                created_at_utc TIMESTAMP NOT NULL,
                updated_at_utc TIMESTAMP NOT NULL
            );

            CREATE TABLE IF NOT EXISTS challenges (
                id UUID PRIMARY KEY,
                case_id UUID NOT NULL,
                target VARCHAR NOT NULL,
                target_id UUID NOT NULL,
                statement VARCHAR NOT NULL,
                reasoning VARCHAR NOT NULL,
                confidence VARCHAR NOT NULL,
                created_at_utc TIMESTAMP NOT NULL
            );

            CREATE TABLE IF NOT EXISTS decisions (
                id UUID PRIMARY KEY,
                case_id UUID NOT NULL,
                question VARCHAR NOT NULL,
                outcome VARCHAR NOT NULL,
                rationale VARCHAR NOT NULL,
                expected_outcome VARCHAR NOT NULL,
                confidence VARCHAR NOT NULL,
                evidence_ids VARCHAR NOT NULL,
                assumption_ids VARCHAR NOT NULL,
                hypothesis_ids VARCHAR NOT NULL,
                challenge_ids VARCHAR NOT NULL,
                created_at_utc TIMESTAMP NOT NULL
            );

            CREATE TABLE IF NOT EXISTS lessons (
                id UUID PRIMARY KEY,
                case_id UUID NOT NULL,
                summary VARCHAR NOT NULL,
                detail VARCHAR NOT NULL,
                confidence VARCHAR NOT NULL,
                decision_ids VARCHAR NOT NULL,
                created_at_utc TIMESTAMP NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
