// Services/ExchangeRateCrawlerService.cs

using Microsoft.Extensions.Logging;
using Tiffc.Common.Models;
using Tiffc.Repository;
using Tiffc.Service.Crawlers;

namespace Tiffc.Service;

public class ExchangeRateCrawlerService(
    IEnumerable<IExchangeRateCrawler> crawlers,
    ExchangeRateRepository repository,
    ILogger<ExchangeRateCrawlerService> logger)
{
    /// <summary>
    /// 爬取所有來源的匯率
    /// </summary>
    public async Task<Dictionary<ExchangeRateSourceEnum, int>> CrawlAllSourcesAsync()
    {
        var results = new Dictionary<ExchangeRateSourceEnum, int>();
        var crawledAt = DateTime.UtcNow;
        
        foreach (var crawler in crawlers)
        {
            try
            {
                logger.LogInformation("開始爬取 {SourceEnum} 匯率", crawler.SourceEnum);
                
                var rates = await crawler.CrawlAsync();
                
                if (rates.Any())
                {
                    var parameters = rates.Select(r => new CreateExchangeRateParameter
                    {
                        Source = crawler.SourceEnum,
                        Currency = r.Currency,
                        Rate = r.Rate
                    }).ToList();
                    
                    await repository.BulkCreateAsync(parameters, crawledAt);
                    
                    results[crawler.SourceEnum] = rates.Count;
                    logger.LogInformation("成功爬取 {SourceEnum} 匯率: {Count} 筆", crawler.SourceEnum, rates.Count);
                }
                else
                {
                    results[crawler.SourceEnum] = 0;
                    logger.LogWarning("{SourceEnum} 沒有爬取到任何匯率資料", crawler.SourceEnum);
                }
            }
            catch (Exception ex)
            {
                results[crawler.SourceEnum] = -1;
                logger.LogError(ex, "爬取 {SourceEnum} 匯率時發生錯誤", crawler.SourceEnum);
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// 爬取特定來源的匯率
    /// </summary>
    public async Task<int> CrawlSourceAsync(ExchangeRateSourceEnum source)
    {
        var crawler = crawlers.FirstOrDefault(c => c.SourceEnum == source);
        if (crawler == null)
        {
            throw new ArgumentException($"找不到來源 {source} 的爬蟲");
        }
        
        var rates = await crawler.CrawlAsync();
        var crawledAt = DateTime.UtcNow;
        
        var parameters = rates.Select(r => new CreateExchangeRateParameter
        {
            Source = crawler.SourceEnum,
            Currency = r.Currency,
            Rate = r.Rate
        }).ToList();
        
        await repository.BulkCreateAsync(parameters, crawledAt);
        
        return rates.Count;
    }
}