// Repository/ExchangeRateRepository.cs

using Supabase.Postgrest;
using Tiffc.Common.Models;
using Tiffc.Repository.Entities;
using Tiffc.Service.Crawlers;
using Client = Supabase.Client;

namespace Tiffc.Repository;

public class ExchangeRateRepository(Client supabaseClient)
{
    /// <summary>
    /// 批次新增匯率記錄
    /// </summary>
    public async Task<bool> BulkCreateAsync(List<CreateExchangeRateParameter> rates)
    {
        try
        {
            var entities = rates.Select(r => new ExchangeRate
            {
                Source = r.Source.ToString(),
                Currency = r.Currency,
                Rate = r.Rate,
            }).ToList();
            
            await supabaseClient
                .From<ExchangeRate>()
                .Upsert(entities);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 取得最新匯率 (依來源)
    /// </summary>
    public async Task<List<ExchangeRateModel>> GetLatestRatesBySourceAsync(ExchangeRateSourceEnum source)
    {
        var response = await supabaseClient
            .From<ExchangeRate>()
            .Where(x => x.Source == source.ToString())
            .Order(x => x.CrawledAt, Constants.Ordering.Descending)
            .Limit(10)
            .Get();
        
        return response.Models.Select(MapToModel).ToList();
    }
    
    /// <summary>
    /// 取得所有來源的最新匯率
    /// </summary>
    public async Task<List<ExchangeRateModel>> GetAllLatestRatesAsync()
    {
        // 這個查詢比較複雜,需要取得每個 source + currency 組合的最新一筆
        var allRates = await supabaseClient
            .From<ExchangeRate>()
            .Order(x => x.CrawledAt, Constants.Ordering.Descending)
            .Limit(100)
            .Get();
        
        // 在記憶體中分組取最新
        var latestRates = allRates.Models
            .GroupBy(x => new { x.Source, x.Currency })
            .Select(g => g.OrderByDescending(x => x.CrawledAt).First())
            .ToList();
        
        return latestRates.Select(MapToModel).ToList();
    }
    
    /// <summary>
    /// 取得特定幣別的歷史匯率
    /// </summary>
    public async Task<List<ExchangeRateModel>> GetHistoryAsync(ExchangeRateSourceEnum source, string currency, int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        
        var response = await supabaseClient
            .From<ExchangeRate>()
            .Where(x => x.Source == source.ToString() && x.Currency == currency)
            .Filter("crawled_at", Constants.Operator.GreaterThanOrEqual, startDate.ToString("o"))
            .Order(x => x.CrawledAt, Constants.Ordering.Descending)
            .Get();
        
        return response.Models.Select(MapToModel).ToList();
    }
    
    private static ExchangeRateModel MapToModel(ExchangeRate entity)
    {
        return new ExchangeRateModel
        {
            Source = entity.Source,
            Currency = entity.Currency,
            Rate = entity.Rate,
            CrawledAt = entity.CrawledAt.ToLocalTime(),
        };
    }
}