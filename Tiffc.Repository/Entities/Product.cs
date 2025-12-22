using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Tiffc.Repository.Entities;

[Table("products")]
public class Product : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("price_jpy_original")]
    public decimal PriceJpyOriginal { get; set; }

    [Column("price_jpy_sale")]
    public decimal? PriceJpySale { get; set; }

    [Column("price_twd")]
    public decimal? PriceTwd { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("url")]
    public string Url { get; set; } = string.Empty;

    [Column("image_urls")]
    public List<string>? ImageUrls { get; set; }

    [Column("shop_name")]
    public string? ShopName { get; set; }

    [Column("category")]
    public string? Category { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
