using TourismServer.Orm.Attributes;

namespace TourismServer.Models;

[Table("categories")]
public sealed class Category
{
    [Key]
    [Column("id", IsNullable = false)]
    public int Id { get; set; }

    [Required, MaxLength(64)]
    [Column("name", IsNullable = false)]
    public string Name { get; set; } = "";
}