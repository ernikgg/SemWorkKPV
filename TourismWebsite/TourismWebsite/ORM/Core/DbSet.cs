using Npgsql;
using TourismServer.Orm.Metadata;
using TourismServer.Orm.Validation;

namespace TourismServer.Orm.Core;

public sealed class DbSet<T> where T : new()
{
    private readonly OrmDbContext _ctx;
    private readonly EntityMap _map;

    public DbSet(OrmDbContext ctx)
    {
        _ctx = ctx;
        _map = EntityMapCache.Get<T>();
    }

    public async Task<T?> FindAsync(int id, CancellationToken ct = default)
    {
        if (_map.KeyProperty is null || _map.KeyColumnName is null)
            throw new InvalidOperationException($"{typeof(T).Name} has no [Key].");

        var selectList = string.Join(", ", _map.Columns.Select(c => c.ColumnName));
        var sql = $"SELECT {selectList} FROM {_map.TableName} WHERE {_map.KeyColumnName} = @id LIMIT 1;";

        await using var conn = await _ctx.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return default;

        return Materializer.ReadEntity<T>(reader, _map);
    }
    public async Task<int> AddAsync(T entity, CancellationToken ct = default)
    {
        EntityValidator.Validate(entity);

        if (_map.KeyProperty is null || _map.KeyColumnName is null)
            throw new InvalidOperationException($"{typeof(T).Name} has no [Key].");

        // INSERT только по не-key колонкам (обычно id serial)
        var insertCols = _map.Columns.Where(c => !c.IsKey).ToList();
        var colList = string.Join(", ", insertCols.Select(c => c.ColumnName));
        var paramList = string.Join(", ", insertCols.Select((c, i) => "@p" + i));

        var sql = $"INSERT INTO {_map.TableName} ({colList}) VALUES ({paramList}) RETURNING {_map.KeyColumnName};";

        await using var conn = await _ctx.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);

        for (int i = 0; i < insertCols.Count; i++)
        {
            var val = insertCols[i].Property.GetValue(entity) ?? DBNull.Value;
            cmd.Parameters.AddWithValue("p" + i, val);
        }

        var newIdObj = await cmd.ExecuteScalarAsync(ct);
        var newId = Convert.ToInt32(newIdObj);

        _map.KeyProperty.SetValue(entity, newId);
        return newId;
    }

    /// <summary>
    /// WhereSql — это кусок после WHERE, например: "is_top = true" или "title ILIKE @p"
    /// </summary>
    public async Task<IReadOnlyList<T>> WhereAsync(string whereSql, object? parameters = null, CancellationToken ct = default)
    {
        var selectList = string.Join(", ", _map.Columns.Select(c => c.ColumnName));
        var sql = $"SELECT {selectList} FROM {_map.TableName} WHERE {whereSql};";

        await using var conn = await _ctx.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);

        ParamBinder.Bind(cmd, parameters);

        var result = new List<T>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(Materializer.ReadEntity<T>(reader, _map));
        }

        return result;
    }

    public async Task<IReadOnlyList<T>> ToListAsync(CancellationToken ct = default)
    {
        var selectList = string.Join(", ", _map.Columns.Select(c => c.ColumnName));
        var sql = $"SELECT {selectList} FROM {_map.TableName};";

        await using var conn = await _ctx.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);

        var result = new List<T>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(Materializer.ReadEntity<T>(reader, _map));
        }

        return result;
    }
}