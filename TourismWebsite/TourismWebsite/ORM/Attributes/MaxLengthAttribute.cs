[AttributeUsage(AttributeTargets.Property)]
public sealed class MaxLengthAttribute : Attribute
{
    public int Length { get; }
    public MaxLengthAttribute(int length) => Length = length;
}