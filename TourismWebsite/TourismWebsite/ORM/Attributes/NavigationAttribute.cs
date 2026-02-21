namespace TourismServer.Orm.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class NavigationAttribute : Attribute
{
    public bool Collection { get; }
    public NavigationAttribute(bool collection = false) => Collection = collection;
}