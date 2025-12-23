using Tiffc.Service.Models;

namespace Tiffc.Service.Crawlers;

public interface IExchangeRateCrawler
{
    /// <summary>
    /// 爬蟲來源名稱
    /// </summary>
    ExchangeRateSourceEnum SourceEnum { get; }
    
    /// <summary>
    /// 爬取匯率資料
    /// </summary>
    Task<List<CrawledExchangeRate>> CrawlAsync();
}