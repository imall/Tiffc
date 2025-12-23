namespace Tiffc.Common.Models;

public class UpdateProductParameter
{
    public string Title { get; set; } = string.Empty;
    public decimal PriceJpyOriginal { get; set; }
    public decimal PriceJpySale { get; set; }
    public decimal PriceTwd { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = [];
    public string ShopName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<CreateProductVariantParameter>? Variants { get; set; }
}