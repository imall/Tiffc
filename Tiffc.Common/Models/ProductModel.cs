namespace Tiffc.Common.Models;

public class ProductModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal PriceJpyOriginal { get; set; }
    public decimal? PriceJpySale { get; set; }
    public decimal? PriceTwd { get; set; }
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public List<string>? ImageUrls { get; set; }
    public string? ShopName { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProductParameter
{
    public string Title { get; set; } = string.Empty;
    public decimal PriceJpyOriginal { get; set; }
    public decimal? PriceJpySale { get; set; }
    public decimal? PriceTwd { get; set; }
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public List<string>? ImageUrls { get; set; }
    public string? ShopName { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
}
