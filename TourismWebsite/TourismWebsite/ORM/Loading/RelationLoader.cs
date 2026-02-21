using Npgsql;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using TourismServer.Orm.Attributes;
using TourismServer.Orm.Core;
using TourismServer.Orm.Metadata;


namespace TourismServer.Orm.Loading;

using ForeignKeyAttribute = TourismServer.Orm.Attributes.ForeignKeyAttribute;
internal static class RelationLoader
{
    public static async Task LoadManyToOneAsync<T, TNav>(
        OrmDbContext ctx,
        T entity,
        Expression<Func<T, TNav?>> navExpr,
        CancellationToken ct)
        where TNav : class, new()
    {
        var navProp = GetProperty(navExpr);
        var fkProp = FindForeignKeyProperty(typeof(T), navProp.Name);

        var fkValue = fkProp.GetValue(entity);
        if (fkValue is null)
        {
            navProp.SetValue(entity, null);
            return;
        }

        if (fkValue is not int fkId)
            throw new InvalidOperationException("Only int FK is supported in this step.");

        // загрузим связанную сущность через FindAsync
        var nav = await ctx.Set<TNav>().FindAsync(fkId, ct);
        navProp.SetValue(entity, nav);
    }

    public static async Task LoadManyToOneAsync<T, TNav>(
        OrmDbContext ctx,
        IReadOnlyList<T> entities,
        Expression<Func<T, TNav?>> navExpr,
        CancellationToken ct)
        where TNav : class, new()
    {
        if (entities.Count == 0) return;

        var navProp = GetProperty(navExpr);
        var fkProp = FindForeignKeyProperty(typeof(T), navProp.Name);

        // собрать все FK
        var ids = new HashSet<int>();
        var fkByEntity = new Dictionary<T, int>();

        foreach (var e in entities)
        {
            var v = fkProp.GetValue(e);
            if (v is int id)
            {
                ids.Add(id);
                fkByEntity[e] = id;
            }
            else
            {
                // null FK → null nav
                navProp.SetValue(e, null);
            }
        }

        if (ids.Count == 0) return;

        // вытаскиваем все категории одним запросом: WHERE id IN (...)
        var navMap = EntityMapCache.Get<TNav>();
        if (navMap.KeyColumnName is null)
            throw new InvalidOperationException($"{typeof(TNav).Name} must have [Key].");

        var selectList = string.Join(", ", navMap.Columns.Select(c => c.ColumnName));

        var idList = ids.ToArray();
        var paramNames = new string[idList.Length];
        for (int i = 0; i < idList.Length; i++)
            paramNames[i] = "@p" + i;

        var sql = $"SELECT {selectList} FROM {navMap.TableName} WHERE {navMap.KeyColumnName} IN ({string.Join(", ", paramNames)});";

        await using var conn = await ctx.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);

        for (int i = 0; i < idList.Length; i++)
            cmd.Parameters.AddWithValue("p" + i, idList[i]);

        var loaded = new Dictionary<int, TNav>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var nav = Materializer.ReadEntity<TNav>(reader, navMap);
            // key всегда int на этом шаге
            var keyVal = (int)(navMap.KeyProperty!.GetValue(nav)!);
            loaded[keyVal] = nav;
        }

        // разложить по сущностям
        foreach (var kv in fkByEntity)
        {
            if (loaded.TryGetValue(kv.Value, out var nav))
                navProp.SetValue(kv.Key, nav);
            else
                navProp.SetValue(kv.Key, null);
        }
    }

    private static PropertyInfo GetProperty<T, TProp>(Expression<Func<T, TProp>> expr)
    {
        if (expr.Body is MemberExpression me && me.Member is PropertyInfo pi)
            return pi;

        throw new InvalidOperationException("Navigation expression must be a property access, e.g. x => x.Category");
    }

    private static PropertyInfo FindForeignKeyProperty(Type entityType, string navigationName)
    {
        // ищем свойство с [ForeignKey(nameof(Navigation))]
        var props = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (var p in props)
        {
            var fk = p.GetCustomAttribute<ForeignKeyAttribute>();

            if (fk is not null && string.Equals(fk.NavigationProperty, navigationName, StringComparison.Ordinal))
                return p;
        }

        // fallback по конвенции: NavigationName + "Id"
        var byConvention = props.FirstOrDefault(p => p.Name == navigationName + "Id");
        if (byConvention is not null)
            return byConvention;

        throw new InvalidOperationException($"Foreign key property for navigation '{navigationName}' not found in {entityType.Name}.");
    }
}