using Supabase;
using Tiffc.Common.Models;
using Tiffc.Repository.Entities;

namespace Tiffc.Repository;

/// <summary>
/// 商品儲存庫
/// </summary>
/// <param name="supabaseClient"></param>
public class ProductRepository(Client supabaseClient)
{
    /// <summary>
    /// 查詢全部商品
    /// </summary>
    public async Task<IEnumerable<ProductModel>> GetAllProductsAsync()
    {
        var response = await supabaseClient
            .From<Product>()
            .Get();

        // 轉換成 DTO
        return response.Models.Select(MapToModel).ToList();
    }

    /// <summary>
    /// 新增一筆商品
    /// </summary>
    public async Task<ProductModel?> CreateProductAsync(CreateProductParameter createDto)
    {
        // 轉換成 Entity
        var product = new Product
        {
            Title = createDto.Title,
            PriceJpyOriginal = createDto.PriceJpyOriginal,
            PriceJpySale = createDto.PriceJpySale,
            PriceTwd = createDto.PriceTwd,
            Description = createDto.Description,
            Url = createDto.Url,
            ImageUrls = createDto.ImageUrls,
            ShopName = createDto.ShopName,
            Category = createDto.Category,
            Notes = createDto.Notes,
            CreatedAt = DateTime.Now
        };

        var response = await supabaseClient
            .From<Product>()
            .Insert(product);

        var createdProduct = response.Models.FirstOrDefault();
        return createdProduct != null ? MapToModel(createdProduct) : null;
    }

    /// <summary>
    /// Entity 轉 DTO
    /// </summary>
    private static ProductModel MapToModel(Product product)
    {
        return new ProductModel
        {
            Id = product.Id,
            Title = product.Title,
            PriceJpyOriginal = product.PriceJpyOriginal,
            PriceJpySale = product.PriceJpySale,
            PriceTwd = product.PriceTwd,
            Description = product.Description,
            Url = product.Url,
            ImageUrls = product.ImageUrls,
            ShopName = product.ShopName,
            Category = product.Category,
            Notes = product.Notes,
            CreatedAt = product.CreatedAt.ToLocalTime()
        };
    }
}
