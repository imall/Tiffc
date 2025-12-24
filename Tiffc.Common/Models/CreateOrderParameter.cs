namespace Tiffc.Common.Models;

/// <summary>
/// 建立訂單參數
/// </summary>
public class CreateOrderParameter
{
    /// <summary>
    /// 顧客姓名
    /// </summary>
    /// <example>頁大雄</example>
    public string CustomerName { get; set; } = string.Empty;
    
    /// <summary>
    /// 顧客信箱
    /// </summary>
    /// <example>ya@gmail.com</example>
    public string? CustomerEmail { get; set; }
    
    /// <summary>
    /// 顧客電話
    /// </summary>
    /// <example>0987654321</example>
    public string? CustomerPhone { get; set; }
    
    /// <summary>
    /// 訂單狀態
    /// </summary>
    /// <example>待付款</example>
    public StatusEnum Status { get; set; } = StatusEnum.待付款;

    public IEnumerable<CreateOrderItemParameter> Items { get; set; } = [];
}

/// <summary>
/// 建立訂單明細參數
/// </summary>
public class CreateOrderItemParameter
{
    /// <summary>
    /// 商品 Id
    /// </summary>
    /// <example>e78ad45d-a3fc-4d28-9f54-f9dbe6b4080d</example>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// 訂購數量
    /// </summary>
    /// <example>1</example>
    public int Quantity { get; set; }
    
    /// <summary>
    /// 商品單價
    /// </summary>
    public decimal UnitPrice { get; set; }

    public IEnumerable<CreateOrderItemVariantParameter>? Variants { get; set; }
}

/// <summary>
/// 建立訂單商品規格參數
/// </summary>
public class CreateOrderItemVariantParameter
{
    /// <summary>
    /// 規格名稱
    /// </summary>
    /// <example>顏色</example>
    public string VariantName { get; set; } = string.Empty;
    
    /// <summary>
    /// 規格資料
    /// </summary>
    /// <example>藍色</example>
    public string VariantValue { get; set; } = string.Empty;
}
