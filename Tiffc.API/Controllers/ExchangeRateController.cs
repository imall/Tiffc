// Controllers/ExchangeRateController.cs
using Microsoft.AspNetCore.Mvc;
using Tiffc.Repository;
using Tiffc.Service;
using Tiffc.Service.Crawlers;

namespace Tiffc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangeRateController : ControllerBase
{
    private readonly ExchangeRateCrawlerService _crawlerService;
    private readonly ExchangeRateRepository _repository;
    
    public ExchangeRateController(
        ExchangeRateCrawlerService crawlerService,
        ExchangeRateRepository repository)
    {
        _crawlerService = crawlerService;
        _repository = repository;
    }
    
    /// <summary>
    /// 手動觸發爬取所有來源匯率
    /// </summary>
    [HttpPost("crawl")]
    public async Task<IActionResult> CrawlAllSources()
    {
        var results = await _crawlerService.CrawlAllSourcesAsync();
        return Ok(new { message = "爬取完成", results });
    }
    
    /// <summary>
    /// 手動觸發爬取特定來源匯率
    /// </summary>
    [HttpPost("crawl/{source}")]
    public async Task<IActionResult> CrawlSource(ExchangeRateSourceEnum source)
    {
        try
        {
            var count = await _crawlerService.CrawlSourceAsync(source);
            return Ok(new { message = $"成功爬取 {count} 筆匯率" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// 取得所有最新匯率
    /// </summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestRates()
    {
        var rates = await _repository.GetAllLatestRatesAsync();
        return Ok(rates);
    }
    
    /// <summary>
    /// 取得特定來源的最新匯率
    /// </summary>
    [HttpGet("latest/{source}")]
    public async Task<IActionResult> GetLatestRatesBySource(ExchangeRateSourceEnum source)
    {
        var rates = await _repository.GetLatestRatesBySourceAsync(source);
        return Ok(rates);
    }
    
    /// <summary>
    /// 取得歷史匯率
    /// </summary>
    [HttpGet("history/{source}/{currency}")]
    public async Task<IActionResult> GetHistory(ExchangeRateSourceEnum source, string currency, [FromQuery] int days = 30)
    {
        var rates = await _repository.GetHistoryAsync(source, currency, days);
        return Ok(rates);
    }
}