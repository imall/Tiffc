using Tiffc.Common.Models;
using Tiffc.Repository;

namespace Tiffc.Service;

public class OrderService(OrderRepository orderRepository, ProductRepository productRepository)
{
    /// <summary>
    /// 建立訂單
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public async Task<OrderModel> CreateOrderAsync(CreateOrderParameter parameter)
    {
        var order = await orderRepository.CreateOrderAsync(parameter);
        await EnrichOrderWithProductInfo(order);
        return order;
    }

    /// <summary>
    /// 根據訂單 ID 查詢訂單資料
    /// </summary>
    /// <param name="orderNumber"></param>
    /// <returns></returns>
    public async Task<OrderModel> GetOrderByIdAsync(string orderNumber)
    {
        var order = await orderRepository.GetOrderByIdAsync(orderNumber);
        await EnrichOrderWithProductInfo(order);
        return order;
    }


    /// <summary>
    /// 查詢所有訂單資料
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<OrderModel>> GetAllOrdersAsync()
    {
        var orders = await orderRepository.GetAllOrdersAsync();
        foreach (var order in orders)
        {
            await EnrichOrderWithProductInfo(order);
        }
        return orders;
    }

    /// <summary>
    /// 更新訂單狀態
    /// </summary>
    /// <param name="orderId">訂單 ID</param>
    /// <param name="status">新狀態</param>
    /// <returns>更新後的訂單</returns>
    public async Task<OrderModel> UpdateOrderStatusAsync(Guid orderId, StatusEnum status)
    {
        var order = await orderRepository.UpdateOrderStatusAsync(orderId, status);
        await EnrichOrderWithProductInfo(order);
        return order;
    }

    /// <summary>
    /// 刪除訂單
    /// </summary>
    /// <param name="orderId">訂單 ID</param>
    /// <returns>是否刪除成功</returns>
    public async Task<bool> DeleteOrderAsync(Guid orderId)
    {
        return await orderRepository.DeleteOrderAsync(orderId);
    }

    /// <summary>
    /// 為訂單項目填充商品資訊
    /// </summary>
    private async Task EnrichOrderWithProductInfo(OrderModel order)
    {
        if (order.Items == null || !order.Items.Any())
            return;

        // 收集所有商品 ID
        var productIds = order.Items.Select(item => item.ProductId).Distinct().ToList();

        // 批量查詢商品資訊
        var products = new Dictionary<Guid, ProductModel>();
        foreach (var productId in productIds)
        {
            try
            {
                var product = await productRepository.GetByIdAsync(productId);
                if (product != null)
                {
                    products[productId] = product;
                }
            }
            catch
            {
                // 如果查詢失敗，繼續處理其他商品
                continue;
            }
        }

        // 為每個訂單項目填充商品資訊
        foreach (var item in order.Items)
        {
            if (products.TryGetValue(item.ProductId, out var product))
            {
                item.ProductInfo = new ProductInfoModel
                {
                    Title = product.Title,
                    ImageUrl = product.ImageUrls?.FirstOrDefault(),
                    Url = product.Url
                };
            }
        }
    }
}
