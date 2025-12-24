namespace Tiffc.Common.Models;

/// <summary>
/// 建立訂單參數
/// </summary>
public class CreateOrderParameter
{
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.待付款;

    public IEnumerable<CreateOrderItemParameter> Items { get; set; } = [];
}

/// <summary>
/// 建立訂單明細參數
/// </summary>
public class CreateOrderItemParameter
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public IEnumerable<CreateOrderItemVariantParameter>? Variants { get; set; }
}

/// <summary>
/// 建立訂單商品規格參數
/// </summary>
public class CreateOrderItemVariantParameter
{
    public string VariantName { get; set; } = string.Empty;
    public string VariantValue { get; set; } = string.Empty;
}
