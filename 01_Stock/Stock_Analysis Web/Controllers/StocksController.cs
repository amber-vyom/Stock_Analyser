using Microsoft.AspNetCore.Mvc;
using Stock_Analysis.Engine.Analysis;
using Stock_Analysis.Engine.Database;
using Stock_Analysis.Engine.DSA;
using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Web.Controllers;

[ApiController]
[Route("api")]
public class StocksController : ControllerBase
{
    private readonly ProcessedDataRepository repository = new();
    private readonly StockRankingService rankingService = new();
    private readonly StockFilterEngine filterEngine = new();
    private static readonly StockScorer Scorer = new();

    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        try
        {
            var stocks = await repository.GetAllRecordsAsync();
            return Ok(new { status = "ok", stockCount = stocks.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new { status = "database unavailable", error = ex.Message });
        }
    }

    [HttpGet("stocks")]
    public async Task<IActionResult> GetStocks([FromQuery] string? search)
    {
        var stocks = await repository.GetAllRecordsAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            stocks = stocks
                .Where(s => s.Symbol.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return Ok(stocks);
    }

    [HttpGet("stocks/top")]
    public async Task<IActionResult> GetTop([FromQuery] int count = 10)
    {
        if (count < 1 || count > 100)
        {
            return BadRequest(new { error = "count must be between 1 and 100." });
        }

        var stocks = await repository.GetAllRecordsAsync();
        var bySymbol = stocks.ToDictionary(s => s.Symbol);
        var ranked = rankingService.RankStocks(stocks, count);

        return Ok(ranked.Select((r, i) => ToDto(r, bySymbol[r.Symbol], i + 1)));
    }

    [HttpGet("snapshot")]
    public async Task<IActionResult> GetSnapshot([FromQuery] int count = 200)
    {
        if (count < 1 || count > 1000)
        {
            return BadRequest(new { error = "count must be between 1 and 1000." });
        }

        var stocks = await repository.GetAllRecordsAsync();
        var bySymbol = stocks.ToDictionary(s => s.Symbol);
        var ranked = rankingService.RankAllWithMergeSort(stocks).Take(count).ToList();

        return Ok(new
        {
            generatedAt = DateTime.UtcNow,
            stocks = ranked.Select((r, i) => ToDto(r, bySymbol[r.Symbol], i + 1))
        });
    }

    [HttpGet("stocks/{symbol}")]
    public async Task<IActionResult> GetStock(string symbol)
    {
        var stocks = await repository.GetAllRecordsAsync();
        var sorted = SymbolSearch.SortBySymbol(stocks);
        var stock = SymbolSearch.BinarySearch(sorted, symbol);

        if (stock == null)
        {
            return NotFound(new { error = $"Symbol '{symbol}' not found." });
        }

        return Ok(ToDto(filterEngine.Analyse(stock), stock, 0));
    }

    private static object ToDto(StockFilterResult r, SevenDayStockData s, int rank)
    {
        return new
        {
            rank,
            symbol = r.Symbol,
            score = r.Score,
            passedFilters = r.PassedFilters,
            filters = new[]
            {
                new { name = "Completeness (7 days)", passed = r.Filter01Passed },
                new { name = "Positive return", passed = r.Filter02Passed },
                new { name = "Strong return (>=3%)", passed = r.Filter03Passed },
                new { name = "Close above open", passed = r.Filter04Passed },
                new { name = "Price stability", passed = r.Filter05Passed },
                new { name = "Volume activity", passed = r.Filter06Passed },
                new { name = "Volume consistency", passed = r.Filter07Passed },
                new { name = "Strong momentum (>=5%)", passed = r.Filter08Passed },
                new { name = "High traded value", passed = r.Filter09Passed },
                new { name = "Overall quality", passed = r.Filter10Passed }
            },
            breakdown = Scorer.Breakdown(s, r.PassedFilters)
                .Select(c => new { name = c.Name, points = Math.Round(c.Points, 2), max = c.Max }),
            metrics = s
        };
    }
}
