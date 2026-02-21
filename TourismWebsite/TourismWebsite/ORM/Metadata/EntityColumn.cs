using System.Reflection;

namespace TourismServer.Orm.Metadata;

public sealed class EntityColumn
{
    public required PropertyInfo Property { get; init; }
    public required string ColumnName { get; init; }
    public required bool IsNullable { get; init; }
    public int? MaxLength { get; init; }
    public bool Required { get; init; }
    public bool IsKey { get; init; }
}