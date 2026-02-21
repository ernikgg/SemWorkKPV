using System.Reflection;

namespace TourismServer.Orm.Metadata;

public sealed class EntityMap
{
    public required Type EntityType { get; init; }
    public required string TableName { get; init; }

    public required PropertyInfo? KeyProperty { get; init; }
    public required string? KeyColumnName { get; init; }

    public required IReadOnlyList<EntityColumn> Columns { get; init; }
    public required IReadOnlyDictionary<string, EntityColumn> ColumnsByName { get; init; }
}