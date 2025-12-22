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
}