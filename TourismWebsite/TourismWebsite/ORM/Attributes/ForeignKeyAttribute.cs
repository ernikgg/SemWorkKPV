namespace TourismServer.Orm.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class ForeignKeyAttribute : Attribute
{
    public string NavigationProperty { get; }
    public ForeignKeyAttribute(string navigationProperty) => NavigationProperty = navigationProperty;
}
