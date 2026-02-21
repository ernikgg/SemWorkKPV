using System.Reflection;
using Npgsql;

namespace TourismServer.Orm.Core;

internal static class ParamBinder
{
    public static void Bind(NpgsqlCommand cmd, object? parameters)
    {
        if (parameters is null) return;

        // поддержим простое: new { p = "%rome%" }
        var t = parameters.GetType();
        foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var name = p.Name;
            var value = p.GetValue(parameters) ?? DBNull.Value;
            cmd.Parameters.AddWithValue(name, value);
        }
    }
}