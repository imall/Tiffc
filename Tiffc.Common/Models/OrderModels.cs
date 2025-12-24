namespace Tiffc.Common.Models;

/// <summary>
/// 訂單 Model
/// </summary>
public class OrderModel
{
    /// <summary>
    /// id
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 訂單編號
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// 顧客姓名
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;
    
    /// <summary>
    /// 顧客 email
    /// </summary>
    public string? CustomerEmail { get; set; }
    
    /// <summary>
    /// 顧客電話
    /// </summary>
    public string? CustomerPhone { get; set; }
    
    /// <summary>
    /// 總金額
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// 狀態
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    
    public List<OrderItemModel>? Items { get; set; }
}

/// <summary>
/// 訂單明細 Model
/// </summary>
public class OrderItemModel
{
    /// <summary>
    /// 訂單明細 ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 訂單 ID
    /// </summary>
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// 商品 ID
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// 數量
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// 單價
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// 小計
    /// </summary>
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 商品規格
    /// </summary>
    public List<OrderItemVariantModel>? Variants { get; set; }
}

/// <summary>
/// 訂單商品規格 Model
/// </summary>
public class OrderItemVariantModel
{
    /// <summary>
    /// 訂單規格 ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 訂單明細 ID
    /// </summary>
    public Guid OrderItemId { get; set; }
    
    /// <summary>
    /// 規格名稱
    /// </summary>
    public string VariantName { get; set; } = string.Empty;
    
    /// <summary>
    /// 規格值 
    /// </summary>
    public string VariantValue { get; set; } = string.Empty;
    
    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
