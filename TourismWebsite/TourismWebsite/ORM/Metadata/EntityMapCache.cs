using System.Collections.Concurrent;
using System.Reflection;
using TourismServer.Orm.Attributes;

namespace TourismServer.Orm.Metadata;

public static class EntityMapCache
{
    private static readonly ConcurrentDictionary<Type, EntityMap> Cache = new();

    public static EntityMap Get<T>() => Get(typeof(T));

    public static EntityMap Get(Type t) => Cache.GetOrAdd(t, Build);

    private static EntityMap Build(Type t)
    {
        var tableAttr = t.GetCustomAttribute<TableAttribute>()
            ?? throw new InvalidOperationException($"Entity {t.Name} must have [Table].");

        var cols = new List<EntityColumn>();
        PropertyInfo? keyProp = null;
        string? keyCol = null;

        foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            // навигации не считаем колонками
            if (p.GetCustomAttribute<NavigationAttribute>() is not null)
                continue;

            var colAttr = p.GetCustomAttribute<ColumnAttribute>();
            if (colAttr is null)
                continue; // только явно размеченные свойства — меньше магии, меньше багов

            var isKey = p.GetCustomAttribute<KeyAttribute>() is not null;
            if (isKey)
            {
                keyProp = p;
                keyCol = colAttr.Name;
            }

            var required = p.GetCustomAttribute<RequiredAttribute>() is not null;
            var maxLenAttr = p.GetCustomAttribute<MaxLengthAttribute>();

            cols.Add(new EntityColumn
            {
                Property = p,
                ColumnName = colAttr.Name,
                IsNullable = colAttr.IsNullable,
                Required = required,
                MaxLength = maxLenAttr?.Length,
                IsKey = isKey
            });
        }

        if (cols.Count == 0)
            throw new InvalidOperationException($"Entity {t.Name} has no [Column] properties.");

        var byName = cols.ToDictionary(c => c.ColumnName, c => c, StringComparer.OrdinalIgnoreCase);

        return new EntityMap
        {
            EntityType = t,
            TableName = tableAttr.Name,
            KeyProperty = keyProp,
            KeyColumnName = keyCol,
            Columns = cols,
            ColumnsByName = byName
        };
    }
}