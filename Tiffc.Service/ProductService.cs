using Tiffc.Common.Models;
using Tiffc.Repository;

namespace Tiffc.Service;

public class ProductService(ProductRepository repository)
{
    /// <summary>
    /// 查詢全部商品
    /// </summary>
    public async Task<IEnumerable<ProductModel>> GetAllProductsAsync()
    {
        return await repository.GetAllProductsAsync();
    }


    /// <summary>
    /// 新增商品(包含規格)
    /// </summary>
    public async Task<ProductModel?> CreateProductAsync(CreateProductParameter parameter)
    {
        // 如果有帶規格,一起新增
        if (parameter.Variants != null && parameter.Variants.Any())
        {
            return await repository.CreateProductWithVariantsAsync(parameter);
        }
        
        // 沒有規格,只新增商品
        return await repository.CreateProductAsync(parameter);
    }
    
    /// <summary>
    /// 為現有商品新增規格
    /// </summary>
    public async Task<IEnumerable<ProductVariantModel>?> AddProductVariantsAsync(
        Guid productId, 
        IEnumerable<CreateProductVariantParameter> variants)
    {
        return await repository.AddProductVariantsAsync(productId, variants);
    }
    
    /// <summary>
    /// 刪除商品(會連同規格一起刪除)
    /// </summary>
    public async Task<bool> DeleteProductAsync(Guid productId)
    {
   
        return await repository.DeleteProductAsync(productId);
    }

    /// <summary>
    /// 刪除單一商品規格
    /// </summary>
    public async Task<bool> DeleteProductVariantAsync(Guid variantId)
    {
        return await repository.DeleteProductVariantAsync(variantId);
    }

    /// <summary>
    /// 刪除商品的所有規格
    /// </summary>
    public async Task<bool> DeleteAllProductVariantsAsync(Guid productId)
    {
        return await repository.DeleteAllProductVariantsAsync(productId);
    }
}
