using System.Globalization;
using TourismServer.Orm.Metadata;

namespace TourismServer.Orm.Core;

internal static class Materializer
{
    public static T ReadEntity<T>(System.Data.IDataRecord r, EntityMap map) where T : new()
    {
        var entity = new T();

        // порядок SELECT = порядок map.Columns
        for (int i = 0; i < map.Columns.Count; i++)
        {
            var col = map.Columns[i];
            object? raw = r.IsDBNull(i) ? null : r.GetValue(i);

            if (raw is null)
            {
                col.Property.SetValue(entity, null);
                continue;
            }

            var targetType = Nullable.GetUnderlyingType(col.Property.PropertyType) ?? col.Property.PropertyType;

            if (targetType.IsAssignableFrom(raw.GetType()))
            {
                col.Property.SetValue(entity, raw);
                continue;
            }

            // базовые преобразования
            var converted = Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture);
            col.Property.SetValue(entity, converted);
        }

        return entity;
    }
}