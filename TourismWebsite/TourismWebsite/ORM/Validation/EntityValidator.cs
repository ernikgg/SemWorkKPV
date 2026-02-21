using TourismServer.Orm.Metadata;

namespace TourismServer.Orm.Validation;

public static class EntityValidator
{
    public static void Validate(object entity)
    {
        var map = EntityMapCache.Get(entity.GetType());
        var errors = new List<string>();

        foreach (var c in map.Columns)
        {
            var value = c.Property.GetValue(entity);

            // Required
            if (c.Required)
            {
                if (value is null)
                    errors.Add($"{c.Property.Name} is required.");
                else if (value is string s && string.IsNullOrWhiteSpace(s))
                    errors.Add($"{c.Property.Name} is required.");
            }

            // Column IsNullable=false
            if (!c.IsNullable && value is null)
                errors.Add($"{c.Property.Name} cannot be null.");

            // MaxLength
            if (c.MaxLength is not null && value is string str && str.Length > c.MaxLength.Value)
                errors.Add($"{c.Property.Name} max length is {c.MaxLength.Value}.");
        }

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}