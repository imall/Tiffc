using Supabase;
using Tiffc.Common.Models;
using Tiffc.Repository.Entities;

namespace Tiffc.Repository;

/// <summary>
/// 訂單儲存庫
/// </summary>
/// <param name="supabaseClient"></param>
public class OrderRepository(Client supabaseClient)
{
    /// <summary>
    /// 建立訂單
    /// </summary>
    public async Task<OrderModel> CreateOrderAsync(CreateOrderParameter parameter)
    {
        // 1. 生成訂單編號
        var orderNumber = GenerateOrderNumber();

        // 2. 計算總金額
        var totalAmount = parameter.Items.Sum(item => item.UnitPrice * item.Quantity);

        // 3. 建立訂單主表
        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerName = parameter.CustomerName,
            CustomerEmail = parameter.CustomerEmail,
            CustomerPhone = parameter.CustomerPhone,
            TotalAmount = totalAmount,
            Status = parameter.Status.ToString()
        };

        var orderResponse = await supabaseClient
            .From<Order>()
            .Insert(order);

        var createdOrder = orderResponse.Models.First();

        // 4. 建立訂單明細
        var orderItems = parameter.Items.Select(item => new OrderItem
            {
                OrderId = createdOrder.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Subtotal = item.UnitPrice * item.Quantity
            })
            .ToList();

        var orderItemsResponse = await supabaseClient
            .From<OrderItem>()
            .Insert(orderItems);

        var createdOrderItems = orderItemsResponse.Models;

        // 5. 建立訂單商品規格
        var allVariants = new List<OrderItemVariant>();
        for (int i = 0; i < parameter.Items.Count(); i++)
        {
            var item = parameter.Items.ElementAt(i);
            var createdOrderItem = createdOrderItems.ElementAt(i);

            if (item.Variants != null && item.Variants.Any())
            {
                var variants = item.Variants.Select(v => new OrderItemVariant
                {
                    OrderItemId = createdOrderItem.Id,
                    VariantName = v.VariantName,
                    VariantValue = v.VariantValue
                }).ToList();

                allVariants.AddRange(variants);
            }
        }

        if (allVariants.Any())
        {
            await supabaseClient
                .From<OrderItemVariant>()
                .Insert(allVariants);
        }

        // 6. 查詢完整訂單資料並回傳
        return await GetOrderByIdAsync(createdOrder.OrderNumber);
    }

    /// <summary>
    /// 根據 ID 查詢訂單(含明細和規格)
    /// </summary>
    public async Task<OrderModel> GetOrderByIdAsync(string orderNumber)
    {
        // 1. 查詢訂單
        var orderResponse = await supabaseClient
            .From<Order>()
            .Where(o => o.OrderNumber == orderNumber)
            .Get();

        var order = orderResponse.Models.FirstOrDefault();
        if (order == null)
            throw new Exception($"找不到訂單 ID: {orderNumber}");

        // 2. 查詢訂單明細
        var orderItemsResponse = await supabaseClient
            .From<OrderItem>()
            .Where(oi => oi.OrderId == order.Id)
            .Get();

        var orderItems = orderItemsResponse.Models;

        // 3. 查詢所有規格
        var orderItemIds = orderItems.Select(oi => oi.Id).ToList();
        var variantsResponse = await supabaseClient
            .From<OrderItemVariant>()
            .Filter("order_item_id", Supabase.Postgrest.Constants.Operator.In, orderItemIds)
            .Get();

        // 4. 將規格按 OrderItemId 分組
        var variantsByOrderItemId = variantsResponse.Models
            .GroupBy(v => v.OrderItemId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 5. 組合訂單模型
        var orderModel = MapToOrderModel(order);
        orderModel.Items = orderItems.Select(orderItem =>
        {
            var orderItemModel = MapToOrderItemModel(orderItem);
            orderItemModel.Variants = variantsByOrderItemId.TryGetValue(orderItem.Id, out var variants)
                ? variants.Select(MapToOrderItemVariantModel).ToList()
                : new List<OrderItemVariantModel>();
            return orderItemModel;
        }).ToList();

        return orderModel;
    }

    /// <summary>
    /// 查詢全部訂單(含明細和規格)
    /// </summary>
    public async Task<IEnumerable<OrderModel>> GetAllOrdersAsync()
    {
        // 1. 查詢所有訂單
        var ordersResponse = await supabaseClient
            .From<Order>()
            .Get();

        if (!ordersResponse.Models.Any())
            return new List<OrderModel>();

        var orderIds = ordersResponse.Models.Select(o => o.Id).ToList();

        // 2. 查詢所有訂單明細
        var orderItemsResponse = await supabaseClient
            .From<OrderItem>()
            .Filter("order_id", Supabase.Postgrest.Constants.Operator.In, orderIds)
            .Get();

        // 3. 查詢所有規格
        var orderItemIds = orderItemsResponse.Models.Select(oi => oi.Id).ToList();
        var variantsResponse = await supabaseClient
            .From<OrderItemVariant>()
            .Filter("order_item_id", Supabase.Postgrest.Constants.Operator.In, orderItemIds)
            .Get();

        // 4. 將明細按 OrderId 分組
        var itemsByOrderId = orderItemsResponse.Models
            .GroupBy(oi => oi.OrderId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 5. 將規格按 OrderItemId 分組
        var variantsByOrderItemId = variantsResponse.Models
            .GroupBy(v => v.OrderItemId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 6. 組合資料
        return ordersResponse.Models.Select(order =>
        {
            var orderModel = MapToOrderModel(order);
            
            if (itemsByOrderId.TryGetValue(order.Id, out var items))
            {
                orderModel.Items = items.Select(item =>
                {
                    var itemModel = MapToOrderItemModel(item);
                    itemModel.Variants = variantsByOrderItemId.TryGetValue(item.Id, out var variants)
                        ? variants.Select(MapToOrderItemVariantModel).ToList()
                        : new List<OrderItemVariantModel>();
                    return itemModel;
                }).ToList();
            }
            else
            {
                orderModel.Items = new List<OrderItemModel>();
            }

            return orderModel;
        }).ToList();
    }

    /// <summary>
    /// 生成訂單編號
    /// </summary>
    private static string GenerateOrderNumber()
    {
        var now = DateTime.Now;
        var random = new Random().Next(1000, 9999);
        return $"ORD-{now:yyyyMMdd}-{random}";
    }

    #region Mapping Methods

    private static OrderModel MapToOrderModel(Order order)
    {
        return new OrderModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt.ToLocalTime(),
            UpdatedAt = order.UpdatedAt.ToLocalTime()
        };
    }

    private static OrderItemModel MapToOrderItemModel(OrderItem orderItem)
    {
        return new OrderItemModel
        {
            Id = orderItem.Id,
            OrderId = orderItem.OrderId,
            ProductId = orderItem.ProductId,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.UnitPrice,
            Subtotal = orderItem.Subtotal,
            CreatedAt = orderItem.CreatedAt.ToLocalTime()
        };
    }

    private static OrderItemVariantModel MapToOrderItemVariantModel(OrderItemVariant variant)
    {
        return new OrderItemVariantModel
        {
            Id = variant.Id,
            OrderItemId = variant.OrderItemId,
            VariantName = variant.VariantName,
            VariantValue = variant.VariantValue,
            CreatedAt = variant.CreatedAt.ToLocalTime()
        };
    }

    #endregion
}
