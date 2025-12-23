using Microsoft.AspNetCore.Mvc;
using Tiffc.Common.Models;
using Tiffc.Service;

namespace Tiffc.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController(ProductService productService) : ControllerBase
{
    /// <summary>
    /// 取得所有商品
    /// </summary>
    /// <returns>商品列表</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductModel>>> GetAll()
    {
        var products = await productService.GetAllProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// 新增商品
    /// </summary>
    /// <param name="createProduct">商品資料</param>
    /// <returns>新增的商品</returns>
    [HttpPost]
    public async Task<ActionResult<ProductModel>> Create([FromBody] CreateProductParameter createProduct)
    {
        var product = await productService.CreateProductAsync(createProduct);

        if (product == null)
            return BadRequest("無法新增商品");

        return CreatedAtAction(nameof(GetAll), new { id = product.Id }, product);
    }


    /// <summary>
    /// 為指定商品新增規格
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="variants">規格列表</param>
    /// <returns>新增的規格</returns>
    [HttpPost("{productId:guid}/variants")]
    public async Task<ActionResult<IEnumerable<ProductVariantModel>>> AddVariants(
        Guid productId,
        [FromBody] IEnumerable<CreateProductVariantParameter> variants)
    {
        if (!variants.Any())
            return BadRequest("規格資料不可為空");

        var createdVariants = await productService.AddProductVariantsAsync(productId, variants);

        if (createdVariants == null)
            return NotFound($"找不到商品 ID: {productId}");

        if (!createdVariants.Any())
            return BadRequest("無法新增規格");

        return Ok(createdVariants);
    }
    
    /// <summary>
    /// 更新商品及其規格
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="updateProduct"></param>
    /// <returns></returns>
    [HttpPut("{productId:guid}")]
    public async Task<ActionResult<ProductModel>> UpdateProduct(Guid productId, [FromBody] UpdateProductParameter updateProduct)
    {
        var updatedProduct = await productService.UpdateProductAsync(productId, updateProduct);
        if (updatedProduct == null)
            return NotFound($"找不到商品 ID: {productId}");
        return Ok(updatedProduct);
    }
    
    /// <summary>
    /// 刪除商品及其規格
    /// </summary>
    /// <param name="productId">商品 ID</param>
    /// <returns></returns>
    [HttpDelete("{productId:guid}")]
    public async Task<ActionResult> DeleteProduct(Guid productId)
    {
        var result = await productService.DeleteProductAsync(productId);
        if (!result)
            return NotFound($"找不到商品 ID: {productId}");

        return NoContent();
    }
    
    /// <summary>
    /// 刪除單一商品規格
    /// </summary>
    /// <param name="variantId"></param>
    /// <returns></returns>
    [HttpDelete("variants/{variantId:guid}")]
    public async Task<ActionResult> DeleteProductVariant(Guid variantId)
    {
        var result = await productService.DeleteProductVariantAsync(variantId);
        if (!result)
            return NotFound($"找不到規格 ID: {variantId}");

        return NoContent();
    }
    
    // 刪除商品的所有規格
    [HttpDelete("{productId:guid}/variants")]
    public async Task<ActionResult> DeleteAllProductVariants(Guid productId)
    {
        var result = await productService.DeleteAllProductVariantsAsync(productId);
        if (!result)
            return NotFound($"找不到商品 ID: {productId}");

        return NoContent();
    }
}
