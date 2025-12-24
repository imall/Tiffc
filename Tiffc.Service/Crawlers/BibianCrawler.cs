using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Tiffc.Service.Models;

namespace Tiffc.Service.Crawlers;

public class BibianCrawler : IExchangeRateCrawler
{
    private readonly HttpClient _httpClient;

    public ExchangeRateSourceEnum SourceEnum => ExchangeRateSourceEnum.Bibian;

    public BibianCrawler(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-TW,zh;q=0.9,en-US;q=0.8,en;q=0.7,ja;q=0.6");
    }

    public async Task<List<CrawledExchangeRate>> CrawlAsync()
    {
        const string url = "https://www.bibian.co.jp/buy/";
        var html = await _httpClient.GetStringAsync(url);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var rateData = new List<CrawledExchangeRate>();

        // 選擇目標元素: <div class="auction-exchange-rate-usa" id="pp3">
        var rateDiv = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='pp3']");

        var rateText = rateDiv.InnerText.Trim();
        var rate = ParseExchangeRate(rateText);

        if (rate != null)
        {
            rateData.Add(rate);
        }

        return rateData;
    }

    /// <summary>
    /// 解析匯率文字: "1日円=0.2127元"
    /// </summary>
    private static CrawledExchangeRate? ParseExchangeRate(string rateText)
    {
        try
        {
            // 正則表達式: 1日円=0.2127元
            var pattern = @"(\d+)\s*(日円|円)\s*=\s*([\d.]+)\s*元";
            var match = Regex.Match(rateText, pattern);

            if (match.Success)
            {
                var rateValue = match.Groups[3].Value;

                if (decimal.TryParse(rateValue, out var rate))
                {
                    return new CrawledExchangeRate
                    {
                        Currency = "JPY",
                        Rate = rate
                    };
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
