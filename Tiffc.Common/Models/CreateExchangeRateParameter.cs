using Tiffc.Service.Crawlers;

namespace Tiffc.Common.Models;

public class CreateExchangeRateParameter
{
    public ExchangeRateSourceEnum Source { get; set; } 
    public string Currency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}