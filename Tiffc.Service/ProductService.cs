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
    /// 新增一筆商品
    /// </summary>
    public async Task<ProductModel?> CreateProductAsync(CreateProductParameter product)
    {
        return await repository.CreateProductAsync(product);
    }
}
