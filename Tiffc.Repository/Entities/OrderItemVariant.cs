using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Tiffc.Repository.Entities;

/// <summary>
/// 訂單商品規格表
/// </summary>
[Table("order_item_variants")]
public class OrderItemVariant : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("order_item_id")]
    public Guid OrderItemId { get; set; }

    [Column("variant_name")]
    public string VariantName { get; set; } = string.Empty;

    [Column("variant_value")]
    public string VariantValue { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
