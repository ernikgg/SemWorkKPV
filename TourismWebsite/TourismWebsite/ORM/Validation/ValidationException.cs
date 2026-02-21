namespace TourismServer.Orm.Validation;

public sealed class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors)
        : base("Entity validation failed: " + string.Join("; ", errors))
    {
        Errors = errors.ToList();
    }
}