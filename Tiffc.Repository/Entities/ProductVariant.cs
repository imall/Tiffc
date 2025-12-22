using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Tiffc.Repository.Entities;

[Table("product_variants")]
public class ProductVariant : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Column("variant_name")]
    public string VariantName { get; set; } = string.Empty;

    [Column("variant_value")]
    public string VariantValue { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

}
