using System.ComponentModel.DataAnnotations.Schema;
using TourismServer.Orm.Attributes;

namespace TourismServer.Models;
using Table = TourismServer.Orm.Attributes.TableAttribute;
using Column = TourismServer.Orm.Attributes.ColumnAttribute;
using ForeignKey = TourismServer.Orm.Attributes.ForeignKeyAttribute;

[Table("tours")]
public sealed class Tour
{
    [Key]
    [Column("id", IsNullable = false)]
    public int Id { get; set; }

    [Required, MaxLength(120)]
    [Column("title", IsNullable = false)]
    public string Title { get; set; } = "";

    [Required]
    [Column("price_text", IsNullable = false)]
    public string PriceText { get; set; } = "";

    [Required]
    [Column("duration_text", IsNullable = false)]
    public string DurationText { get; set; } = "";

    [Required]
    [Column("image_url", IsNullable = false)]
    public string ImageUrl { get; set; } = "";

    [Column("is_top", IsNullable = false)]
    public bool IsTop { get; set; }

    [Column("category_id", IsNullable = true)]
    [ForeignKey(nameof(Category))]
    public int? CategoryId { get; set; }

    [Navigation]
    public Category? Category { get; set; }
}