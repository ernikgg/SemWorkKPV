using Npgsql;
using TourismServer.Models;
using TourismServer.Models.Admin;
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
    public async Task<DestinationCard?> GetByTitleAsync(string title, CancellationToken ct = default)
    {
        const string sql = """
        SELECT id, title, price_text, duration_text, image_url, is_top
        FROM tours
        WHERE title = @title
        ORDER BY id DESC
        LIMIT 1;
        """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("title", title);

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
    public async Task<int> CreateAsync(TourEditModel model, CancellationToken ct = default)
    {
        const string sql = """
    INSERT INTO tours (title, price_text, duration_text, image_url, is_top)
    VALUES (@title, @price, @duration, @img, @is_top)
    RETURNING id;
    """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("title", model.Title);
        cmd.Parameters.AddWithValue("price", model.PriceText);
        cmd.Parameters.AddWithValue("duration", model.DurationText);
        cmd.Parameters.AddWithValue("img", model.ImageUrl);
        cmd.Parameters.AddWithValue("is_top", model.IsTop);

        var idObj = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(idObj);
    }
    public async Task<TourEditModel?> GetEditModelByIdAsync(int id, CancellationToken ct = default)
    {
        const string sql = """
    SELECT title, price_text, duration_text, image_url, is_top
    FROM tours
    WHERE id = @id;
    """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;

        return new TourEditModel
        {
            Title = r.GetString(0),
            PriceText = r.GetString(1),
            DurationText = r.GetString(2),
            ImageUrl = r.GetString(3),
            IsTop = r.GetBoolean(4),
        };
    }

    public async Task<bool> UpdateAsync(int id, TourEditModel model, CancellationToken ct = default)
    {
        const string sql = """
    UPDATE tours
    SET title=@title, price_text=@price, duration_text=@duration, image_url=@img, is_top=@is_top
    WHERE id=@id;
    """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("title", model.Title);
        cmd.Parameters.AddWithValue("price", model.PriceText);
        cmd.Parameters.AddWithValue("duration", model.DurationText);
        cmd.Parameters.AddWithValue("img", model.ImageUrl);
        cmd.Parameters.AddWithValue("is_top", model.IsTop);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows == 1;
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM tours WHERE id = @id;";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows == 1;
    }
}