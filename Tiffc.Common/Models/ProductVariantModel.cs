namespace Tiffc.Common.Models;

public class ProductVariantModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public string VariantValue { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
