using Tiffc.Common.Models;
using Tiffc.Repository;

namespace Tiffc.Service;

public class OrderService(OrderRepository repository)
{
    /// <summary>
    /// 建立訂單
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public async Task<OrderModel> CreateOrderAsync(CreateOrderParameter parameter)
    {
        return await repository.CreateOrderAsync(parameter);
    }

    /// <summary>
    /// 根據訂單 ID 查詢訂單資料
    /// </summary>
    /// <param name="orderNumber"></param>
    /// <returns></returns>
    public async Task<OrderModel> GetOrderByIdAsync(string orderNumber)
    {
        return await repository.GetOrderByIdAsync(orderNumber);
    }


    /// <summary>
    /// 查詢所有訂單資料
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<OrderModel>> GetAllOrdersAsync()
    {
        return await repository.GetAllOrdersAsync();
    }
}
