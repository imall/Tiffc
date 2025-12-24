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
        var sourceString = source.ToString();
        var response = await supabaseClient
            .From<ExchangeRate>()
            .Where(x => x.Source == sourceString)
            .Get();
        
        return response.Models.Select(MapToModel).ToList();
    }
    
    /// <summary>
    /// 取得所有來源的最新匯率
    /// </summary>
    public async Task<List<ExchangeRateModel>> GetAllLatestRatesAsync()
    {
        var allRates = await supabaseClient
            .From<ExchangeRate>()
            .Get();
     
        return allRates.Models.Select(MapToModel).ToList();
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