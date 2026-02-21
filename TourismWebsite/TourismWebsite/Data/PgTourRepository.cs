using Npgsql;
using TourismServer.Models;
using TourismServer.Orm;
using TourismServer.Orm.Core;

namespace TourismServer.Data;

public sealed class PgTourRepository : ITourRepository
{
    private readonly string _connectionString;

    public PgTourRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<DestinationCard>> GetTopAsync(int count, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, title, price_text, duration_text, image_url, is_top
            FROM tours
            ORDER BY is_top DESC, id ASC
            LIMIT @count;
            """;

        var result = new List<DestinationCard>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("count", count);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new DestinationCard
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                PriceText = reader.GetString(2),
                DurationText = reader.GetString(3),
                ImageUrl = reader.GetString(4),
                IsTop = reader.GetBoolean(5)
            });
        }

        return result;
    }
    public async Task<DestinationCard?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        const string sql = """
        SELECT id, title, price_text, duration_text, image_url, is_top
        FROM tours
        WHERE id = @id;
        """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        return new DestinationCard
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            PriceText = reader.GetString(2),
            DurationText = reader.GetString(3),
            ImageUrl = reader.GetString(4),
            IsTop = reader.GetBoolean(5)
        };
    }
    public async Task<IReadOnlyList<Tour>> GetTopOrmAsync()
    {
        await using var ctx = new OrmDbContext(_connectionString);
        return await ctx.Set<Tour>().WhereAsync("is_top = true");
    }
    public async Task<IReadOnlyList<DestinationCard>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = """
        SELECT id, title, price_text, duration_text, image_url, is_top
        FROM tours
        ORDER BY is_top DESC, id ASC;
        """;

        var result = new List<DestinationCard>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new DestinationCard
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                PriceText = reader.GetString(2),
                DurationText = reader.GetString(3),
                ImageUrl = reader.GetString(4),
                IsTop = reader.GetBoolean(5)
            });
        }

        return result;
    }
}