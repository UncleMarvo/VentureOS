using System.Data;

namespace VentureOS.Infrastructure.Persistence.DuckDb;

internal static class DuckDbParameterExtensions
{
    public static void AddParameter(
        this IDbCommand command,
        string name,
        object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
