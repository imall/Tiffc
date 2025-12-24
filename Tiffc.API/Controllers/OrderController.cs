using Microsoft.AspNetCore.Mvc;
using Tiffc.Common.Models;
using Tiffc.Service;

namespace Tiffc.Api.Controllers;

/// <summary>
/// 訂單管理 API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderController(OrderService orderService) : ControllerBase
{
    /// <summary>
    /// 建立訂單
    /// </summary>
    /// <param name="parameter">訂單建立參數</param>
    /// <returns>建立的訂單資料</returns>
    /// <response code="200">訂單建立成功</response>
    /// <response code="400">請求參數錯誤</response>
    /// <response code="500">伺服器內部錯誤</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderModel>> CreateOrder([FromBody] CreateOrderParameter parameter)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await orderService.CreateOrderAsync(parameter);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "建立訂單失敗",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// 根據訂單 ID 查詢訂單
    /// </summary>
    /// <param name="orderNumber">訂單 Number</param>
    /// <returns>訂單詳細資料</returns>
    /// <response code="200">查詢成功</response>
    /// <response code="404">找不到訂單</response>
    /// <response code="500">伺服器內部錯誤</response>
    [HttpGet("{orderNumber}")]
    [ProducesResponseType(typeof(OrderModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderModel>> GetOrderById(string orderNumber)
    {
        try
        {
            var order = await orderService.GetOrderByIdAsync(orderNumber);
            return Ok(order);
        }
        catch (Exception ex) when (ex.Message.Contains("找不到訂單"))
        {
            return NotFound(new
            {
                message = $"找不到訂單 ID: {orderNumber}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "查詢訂單失敗",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// 查詢所有訂單
    /// </summary>
    /// <returns>訂單列表</returns>
    /// <response code="200">查詢成功</response>
    /// <response code="500">伺服器內部錯誤</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderModel>>> GetAllOrders()
    {
        try
        {
            var orders = await orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "查詢訂單列表失敗",
                error = ex.Message
            });
        }
    }
}