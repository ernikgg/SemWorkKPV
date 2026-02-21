namespace TourismServer.Orm.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ColumnAttribute : Attribute
{
    public string Name { get; }
    public bool IsNullable { get; set; } = true;
    public ColumnAttribute(string name) => Name = name;
}