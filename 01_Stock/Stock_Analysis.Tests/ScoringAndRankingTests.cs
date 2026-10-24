using Stock_Analysis.Engine.Analysis;
using Stock_Analysis.Engine.DSA;
using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Tests;

public class ScoringAndRankingTests
{
    private static SevenDayStockData Stock(
        string symbol,
        int days = 7,
        decimal change = 6m,
        decimal consistency = 80m,
        decimal avgVolume = 2_000_000m,
        decimal tradedValue = 50_000_000m) =>
        new()
        {
            Symbol = symbol,
            TradingDays = days,
            AverageDailyVolume = avgVolume,
            AverageOpenPrice = 100m,
            AverageClosePrice = 104m,
            HighestPrice = 110m,
            LowestPrice = 98m,
            TotalTradedValue = tradedValue,
            PriceChangePercentage = change,
            VolumeConsistency = consistency
        };

    [Fact]
    public void Score_StaysBetweenZeroAndHundred()
    {
        var engine = new StockFilterEngine();
        var extreme = Stock("X", change: 500m, consistency: 1000m,
            avgVolume: 1e12m, tradedValue: 1e15m);

        decimal score = engine.Analyse(extreme).Score;

        Assert.InRange(score, 0m, 100m);
    }

    [Fact]
    public void Score_DiffersWhenMetricsDiffer_EvenIfSameFiltersPass()
    {
        var engine = new StockFilterEngine();
        var a = engine.Analyse(Stock("A", change: 5.5m));
        var b = engine.Analyse(Stock("B", change: 12m));

        Assert.Equal(a.PassedFilters, b.PassedFilters);
        Assert.True(b.Score > a.Score);
    }

    [Fact]
    public void Score_HandlesZeroNegativeAndEmptyValuesSafely()
    {
        var engine = new StockFilterEngine();
        var bad = new SevenDayStockData
        {
            Symbol = "BAD",
            AverageOpenPrice = 0m,
            LowestPrice = -5m,
            AverageDailyVolume = -1m,
            TotalTradedValue = 0m,
            PriceChangePercentage = -40m,
            VolumeConsistency = -10m
        };

        decimal score = engine.Analyse(bad).Score;

        Assert.InRange(score, 0m, 100m);
    }

    [Fact]
    public void Breakdown_SumsToScoreAndMaxPointsTotalHundred()
    {
        var stock = Stock("A", change: 7m);
        var scorer = new StockScorer();
        int passed = new StockFilterEngine().Analyse(stock).PassedFilters;

        var breakdown = scorer.Breakdown(stock, passed);

        Assert.Equal(10, breakdown.Count);
        Assert.Equal(100m, breakdown.Sum(c => c.Max));
        Assert.Equal(scorer.Score(stock, passed), Math.Round(breakdown.Sum(c => c.Points), 2));
        Assert.All(breakdown, c => Assert.InRange(c.Points, 0m, c.Max));
    }

    [Fact]
    public void Score_WeightsAreConfigurable()
    {
        var options = new ScoringOptions
        {
            CompletenessWeight = 0m, PositiveReturnWeight = 0m, StrongReturnWeight = 0m,
            CloseVersusOpenWeight = 0m, PriceStabilityWeight = 0m, VolumeWeight = 0m,
            VolumeConsistencyWeight = 0m, MomentumWeight = 0m, TradedValueWeight = 0m,
            OverallQualityWeight = 0m
        };

        decimal score = new StockFilterEngine(options).Analyse(Stock("A")).Score;

        Assert.Equal(0m, score);
    }

    [Fact]
    public void Ranking_OrdersByScoreDescending()
    {
        var ranked = new StockRankingService().RankStocks(
            new List<SevenDayStockData>
            {
                Stock("LOW", change: 3.5m),
                Stock("HIGH", change: 15m),
                Stock("MID", change: 7m)
            }, 3);

        Assert.Equal(new[] { "HIGH", "MID", "LOW" }, ranked.Select(r => r.Symbol));
    }

    [Fact]
    public void Ranking_TiesBreakBySymbolDeterministically()
    {
        var ranked = new StockRankingService().RankStocks(
            new List<SevenDayStockData> { Stock("ZED"), Stock("ABC"), Stock("MID") }, 3);

        Assert.Equal(new[] { "ABC", "MID", "ZED" }, ranked.Select(r => r.Symbol));
    }

    [Fact]
    public void Ranking_RespectsTopCount()
    {
        var stocks = Enumerable.Range(0, 20).Select(i => Stock($"S{i:00}", change: i)).ToList();

        var ranked = new StockRankingService().RankStocks(stocks, 5);

        Assert.Equal(5, ranked.Count);
    }

    [Fact]
    public void Ranking_ExcludesStocksWithIncompleteWindow()
    {
        var ranked = new StockRankingService().RankStocks(
            new List<SevenDayStockData>
            {
                Stock("PARTIAL", days: 1, change: 50m),
                Stock("FULL", days: 7, change: 5m)
            }, 10);

        Assert.Equal(new[] { "FULL" }, ranked.Select(r => r.Symbol));
    }

    [Fact]
    public void Ranking_CanIncludePartialStocksWhenConfigured()
    {
        var service = new StockRankingService(
            new ScoringOptions { MinimumTradingDaysToRank = 1 });

        var ranked = service.RankStocks(
            new List<SevenDayStockData> { Stock("PARTIAL", days: 1) }, 10);

        Assert.Single(ranked);
    }

    [Fact]
    public void Ranking_EmptyInputReturnsEmptyList()
    {
        Assert.Empty(new StockRankingService().RankStocks(new List<SevenDayStockData>(), 10));
    }
}
