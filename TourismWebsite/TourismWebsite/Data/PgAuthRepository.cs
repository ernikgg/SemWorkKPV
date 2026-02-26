using Npgsql;

namespace TourismServer.Data;

public sealed class PgAuthRepository
{
    private readonly string _cs;

    public PgAuthRepository(string connectionString) => _cs = connectionString;

    public async Task<string?> GetUserRoleAsync(int userId, CancellationToken ct = default)
    {
        const string sql = "SELECT role FROM users WHERE id = @id;";

        await using var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", userId);

        var obj = await cmd.ExecuteScalarAsync(ct);
        return obj?.ToString();
    }
}
