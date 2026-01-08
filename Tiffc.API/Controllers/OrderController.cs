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
    /// 編輯訂單資料
    /// </summary>
    /// <param name="orderId">訂單 ID</param>
    /// <param name="parameter">訂單更新參數</param>
    /// <returns>更新後的訂單資料</returns>
    /// <response code="200">訂單更新成功</response>
    /// <response code="400">請求參數錯誤</response>
    /// <response code="404">找不到訂單</response>
    /// <response code="500">伺服器內部錯誤</response>
    [HttpPut("{orderId}")]
    [ProducesResponseType(typeof(OrderModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderModel>> UpdateOrder(Guid orderId, [FromBody] UpdateOrderParameter parameter)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await orderService.UpdateOrderAsync(orderId, parameter);
            return Ok(order);
        }
        catch (Exception ex) when (ex.Message.Contains("找不到訂單"))
        {
            return NotFound(new
            {
                message = $"找不到訂單 ID: {orderId}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "更新訂單失敗",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// 更新訂單狀態
    /// </summary>
    /// <param name="orderId">訂單 ID</param>
    /// <param name="status">新狀態 (1:待付款, 2:已付款, 3:處理中, 4:已出貨, 5:已取消)</param>
    /// <returns>更新後的訂單資料</returns>
    /// <response code="200">狀態更新成功</response>
    /// <response code="400">請求參數錯誤</response>
    /// <response code="404">找不到訂單</response>
    /// <response code="500">伺服器內部錯誤</response>
    [HttpPatch("{orderId}/status/{status}")]
    [ProducesResponseType(typeof(OrderModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderModel>> UpdateOrderStatus(Guid orderId, StatusEnum status)
    {
        try
        {
            var order = await orderService.UpdateOrderStatusAsync(orderId, status);
            return Ok(order);
        }
        catch (Exception ex) when (ex.Message.Contains("找不到訂單"))
        {
            return NotFound(new
            {
                message = $"找不到訂單 ID: {orderId}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "更新訂單狀態失敗",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// 刪除訂單
    /// </summary>
    /// <param name="orderId">訂單 ID</param>
    /// <returns></returns>
    /// <response code="204">刪除成功</response>
    /// <response code="404">找不到訂單</response>
    /// <response code="500">伺服器內部錯誤</response>
    [HttpDelete("{orderId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteOrder(Guid orderId)
    {
        try
        {
            var result = await orderService.DeleteOrderAsync(orderId);
            if (!result)
            {
                return NotFound(new
                {
                    message = $"找不到訂單 ID: {orderId}"
                });
            }

            return NoContent();
        }
        catch (Exception ex) when (ex.Message.Contains("找不到訂單"))
        {
            return NotFound(new
            {
                message = $"找不到訂單 ID: {orderId}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "刪除訂單失敗",
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