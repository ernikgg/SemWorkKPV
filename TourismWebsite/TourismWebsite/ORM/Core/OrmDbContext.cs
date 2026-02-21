using Npgsql;
using System.Collections.Generic;
using System.Linq.Expressions;
using TourismServer.Orm.Loading;

namespace TourismServer.Orm.Core;

public sealed class OrmDbContext : IAsyncDisposable
{
    private readonly string _connectionString;

    public OrmDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    internal async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken ct)
    {
        var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
    public async Task LoadAsync<T, TNav>(T entity, Expression<Func<T, TNav?>> navExpr, CancellationToken ct = default)
    where TNav : class, new()
    {
        await RelationLoader.LoadManyToOneAsync(this, entity, navExpr, ct);
    }

    public async Task LoadAsync<T, TNav>(IReadOnlyList<T> entities, Expression<Func<T, TNav?>> navExpr, CancellationToken ct = default)
        where TNav : class, new()
    {
        await RelationLoader.LoadManyToOneAsync(this, entities, navExpr, ct);
    }

    public DbSet<T> Set<T>() where T : new() => new(this);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}