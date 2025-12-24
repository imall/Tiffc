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
    /// 查詢全部商品(含規格)
    /// </summary>
    public async Task<IEnumerable<ProductModel>> GetAllProductsAsync()
    {
        // 1. 查詢所有商品
        var productsResponse = await supabaseClient
            .From<Product>()
            .Get();

        if (!productsResponse.Models.Any())
            return new List<ProductModel>();

        // 2. 查詢所有規格
        var variantsResponse = await supabaseClient
            .From<ProductVariant>()
            .Get();

        // 3. 將規格按 ProductId 分組
        var variantsByProductId = variantsResponse.Models
            .GroupBy(v => v.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 4. 組合商品和規格
        return productsResponse.Models.Select(product =>
            {
                var productModel = MapToModel(product);
                productModel.Variants = variantsByProductId.TryGetValue(product.Id, out var variants)
                    ? variants.Select(MapToProductVariantModel).ToList()
                    : [];
                return productModel;
            })
            .ToList();
    }

    /// <summary>
    /// 根據 ID 查詢單一商品(含規格)
    /// </summary>
    public async Task<ProductModel?> GetByIdAsync(Guid productId)
    {
        // 1. 查詢商品
        var productResponse = await supabaseClient
            .From<Product>()
            .Where(x => x.Id == productId)
            .Single();

        if (productResponse == null)
            return null;

        // 2. 查詢該商品的規格
        var variantsResponse = await supabaseClient
            .From<ProductVariant>()
            .Where(v => v.ProductId == productId)
            .Get();

        // 3. 組合商品和規格
        var productModel = MapToModel(productResponse);
        productModel.Variants = variantsResponse.Models.Any()
            ? variantsResponse.Models.Select(MapToProductVariantModel).ToList()
            : [];

        return productModel;
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
            Notes = createDto.Notes
        };

        var response = await supabaseClient
            .From<Product>()
            .Insert(product);

        var createdProduct = response.Models.FirstOrDefault();
        return createdProduct != null ? MapToModel(createdProduct) : null;
    }

    /// <summary>
    /// 新增商品及其規格(一次完成)
    /// </summary>
    public async Task<ProductModel?> CreateProductWithVariantsAsync(CreateProductParameter parameter)
    {
        // 1. 先新增商品
        var product = new Product
        {
            Title = parameter.Title,
            PriceJpyOriginal = parameter.PriceJpyOriginal,
            PriceJpySale = parameter.PriceJpySale,
            PriceTwd = parameter.PriceTwd,
            Description = parameter.Description,
            Url = parameter.Url,
            ImageUrls = parameter.ImageUrls,
            ShopName = parameter.ShopName,
            Category = parameter.Category,
            Notes = parameter.Notes
        };

        var productResponse = await supabaseClient
            .From<Product>()
            .Insert(product);

        var createdProduct = productResponse.Models.FirstOrDefault();
        if (createdProduct == null)
            return null;

        if (parameter.Variants == null || !parameter.Variants.Any()) return MapToProductModel(createdProduct);


        var variants = parameter.Variants.Select(v => new ProductVariant
            {
                ProductId = createdProduct.Id,
                VariantName = v.VariantName,
                VariantValue = v.VariantValue
            })
            .ToList();

        await supabaseClient
            .From<ProductVariant>()
            .Insert(variants);

        // 3. 回傳建立的商品
        return MapToProductModel(createdProduct);
    }

    /// <summary>
    /// 為現有商品新增規格
    /// </summary>
    public async Task<IEnumerable<ProductVariantModel>?> AddProductVariantsAsync(
        Guid productId,
        IEnumerable<CreateProductVariantParameter> variants)
    {
        // 1. 確認商品是否存在
        var productExists = await supabaseClient
            .From<Product>()
            .Where(x => x.Id == productId)
            .Single();

        if (productExists == null)
            return null;

        // 2. 將參數轉換成 Entity
        var variantEntities = variants.Select(v => new ProductVariant
            {
                ProductId = productId,
                VariantName = v.VariantName,
                VariantValue = v.VariantValue
            })
            .ToList();

        // 3. 批次新增規格
        var response = await supabaseClient
            .From<ProductVariant>()
            .Insert(variantEntities);

        // 4. 轉換成 Model 回傳
        return response.Models.Select(MapToProductVariantModel);
    }


    /// <summary>
    /// 更新商品及其規格(完整替換)
    /// </summary>
    public async Task<ProductModel?> UpdateProductAsync(Guid productId, UpdateProductParameter parameter)
    {
        try
        {
            // 1. 確認商品是否存在
            var existingProduct = await supabaseClient
                .From<Product>()
                .Where(x => x.Id == productId)
                .Single();

            if (existingProduct == null)
                return null;

            // 2. 更新商品基本資訊
            var updatedProduct = new Product
            {
                Id = productId,
                Title = parameter.Title,
                PriceJpyOriginal = parameter.PriceJpyOriginal,
                PriceJpySale = parameter.PriceJpySale,
                PriceTwd = parameter.PriceTwd,
                Description = parameter.Description,
                Url = parameter.Url,
                ImageUrls = parameter.ImageUrls,
                ShopName = parameter.ShopName,
                Category = parameter.Category,
                Notes = parameter.Notes
            };

            var productResponse = await supabaseClient
                .From<Product>()
                .Update(updatedProduct);

            var result = productResponse.Models.FirstOrDefault();
            if (result == null)
                return null;

            // 3. 完整替換規格：先刪除所有舊規格
            await supabaseClient
                .From<ProductVariant>()
                .Where(v => v.ProductId == productId)
                .Delete();

          
            if (parameter.Variants == null || parameter.Variants.Count == 0) return MapToProductModel(result);

            // 4. 如果有新規格，則新增
            var variants = parameter.Variants.Select(v => new ProductVariant
                {
                    ProductId = productId,
                    VariantName = v.VariantName,
                    VariantValue = v.VariantValue
                })
                .ToList();

            await supabaseClient
                .From<ProductVariant>()
                .Insert(variants);


            return MapToProductModel(result);
        }
        catch
        {
            return null;
        }
    }


    /// <summary>
    /// 刪除商品(會連同規格一起刪除)
    /// </summary>
    public async Task<bool> DeleteProductAsync(Guid productId)
    {
        try
        {
            // 1. 先刪除該商品的所有規格
            await supabaseClient
                .From<ProductVariant>()
                .Where(v => v.ProductId == productId)
                .Delete();

            // 2. 刪除商品本身
            await supabaseClient
                .From<Product>()
                .Where(p => p.Id == productId)
                .Delete();

            return true;
        }
        catch (Supabase.Postgrest.Exceptions.PostgrestException ex) when (ex.Message.Contains("23503"))
        {
            // 外鍵約束錯誤 - 有訂單引用此商品
            throw new InvalidOperationException("此商品已被訂單使用，無法刪除", ex);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    /// <summary>
    /// 刪除單一商品規格
    /// </summary>
    public async Task<bool> DeleteProductVariantAsync(Guid variantId)
    {
        try
        {
            await supabaseClient
                .From<ProductVariant>()
                .Where(v => v.Id == variantId)
                .Delete();

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 刪除商品的所有規格
    /// </summary>
    public async Task<bool> DeleteAllProductVariantsAsync(Guid productId)
    {
        try
        {
            await supabaseClient
                .From<ProductVariant>()
                .Where(v => v.ProductId == productId)
                .Delete();

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Entity 轉 ProductModel
    /// </summary>
    private static ProductModel MapToProductModel(Product product)
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

    /// <summary>
    /// Entity 轉 ProductVariantModel
    /// </summary>
    private static ProductVariantModel MapToProductVariantModel(ProductVariant variant)
    {
        return new ProductVariantModel
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            VariantName = variant.VariantName,
            VariantValue = variant.VariantValue,
            CreatedAt = variant.CreatedAt.ToLocalTime()
        };
    }
}
