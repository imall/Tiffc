namespace Tiffc.Common.Models;

public class ExchangeRateModel
{
    public string Source { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime CrawledAt { get; set; }
}