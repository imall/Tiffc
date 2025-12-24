using HtmlAgilityPack;
using Tiffc.Service.Models;

namespace Tiffc.Service.Crawlers;

public class LetaoCrawler : IExchangeRateCrawler
{
    private readonly HttpClient _httpClient;
    private static readonly Dictionary<string, string> CurrencyMapping = new()
    {
        { "日幣", "JPY" },
        { "美金", "USD" },
    };
    public ExchangeRateSourceEnum SourceEnum => ExchangeRateSourceEnum.Letao;

    public LetaoCrawler(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-TW,zh;q=0.9,en-US;q=0.8,en;q=0.7");
    }

    public async Task<List<CrawledExchangeRate>> CrawlAsync()
    {
        const string url = "https://www.letao.com.tw/";
        var html = await _httpClient.GetStringAsync(url);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var rateData = new List<CrawledExchangeRate>();
        var rows = htmlDoc.DocumentNode.SelectNodes("//div[@class='index-aside-column indexBannerExRate']//tr");
        
        foreach (var row in rows)
        {
            var currencyCell = row.SelectSingleNode(".//td[@class='k']");
            var rateCell = row.SelectSingleNode(".//td[@class='v']");


            var currency = currencyCell.InnerText.Trim();
            var rateText = rateCell.InnerText.Trim();

            var normalizedCurrency = CurrencyMapping
                .FirstOrDefault(kvp => currency.Contains(kvp.Key))
                .Value ?? currency;
            
            if (decimal.TryParse(rateText, out var rate))
            {
                rateData.Add(new CrawledExchangeRate
                {
                    Currency = normalizedCurrency,
                    Rate = rate
                });
            }
        }

        return rateData;
    }
}
