using Stock_Analysis.Engine.DSA;
using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Tests;

public class AggregatorTests
{
    private static NseMarketRecord Rec(
        string symbol, string date, decimal open, decimal close,
        long volume = 1000, string series = "EQ") =>
        new()
        {
            Symbol = symbol,
            Series = series,
            TradeDate = DateTime.Parse(date),
            OpenPrice = open,
            HighPrice = Math.Max(open, close),
            LowPrice = Math.Min(open, close),
            ClosePrice = close,
            TradingVolume = volume,
            TradedValue = volume * close,
            NumberOfTrades = 10
        };

    [Fact]
    public void PriceChange_UsesChronologicalOrderNotInputOrder()
    {
        var records = new List<NseMarketRecord>
        {
            Rec("AAA", "2026-08-12", 120, 130),
            Rec("AAA", "2026-08-10", 100, 110),
            Rec("AAA", "2026-08-11", 110, 120)
        };

        var result = new SevenDayAggregator().Aggregate(records).Single();

        Assert.Equal(30m, result.PriceChangePercentage);
        Assert.Equal(3, result.TradingDays);
    }

    [Fact]
    public void Aggregate_IgnoresNonEquitySeries()
    {
        var records = new List<NseMarketRecord>
        {
            Rec("AAA", "2026-08-10", 100, 110),
            Rec("BBB", "2026-08-10", 100, 110, series: "BE")
        };

        var result = new SevenDayAggregator().Aggregate(records);

        Assert.Equal(new[] { "AAA" }, result.Select(r => r.Symbol));
    }

    [Fact]
    public void Aggregate_GroupsBySymbolAndSumsVolume()
    {
        var records = new List<NseMarketRecord>
        {
            Rec("AAA", "2026-08-10", 100, 110, 1000),
            Rec("AAA", "2026-08-11", 110, 120, 3000),
            Rec("BBB", "2026-08-10", 50, 55, 500)
        };

        var result = new SevenDayAggregator().Aggregate(records);

        Assert.Equal(2, result.Count);
        var aaa = result.Single(r => r.Symbol == "AAA");
        Assert.Equal(4000, aaa.TotalVolume);
        Assert.Equal(2000m, aaa.AverageDailyVolume);
    }

    [Fact]
    public void VolumeConsistency_IsHundredForConstantVolume_AndStaysInRange()
    {
        var steady = new List<NseMarketRecord>
        {
            Rec("STD", "2026-08-10", 100, 101, 1000),
            Rec("STD", "2026-08-11", 100, 101, 1000)
        };
        var spiky = new List<NseMarketRecord>
        {
            Rec("SPK", "2026-08-10", 100, 101, 1),
            Rec("SPK", "2026-08-11", 100, 101, 100000)
        };

        var agg = new SevenDayAggregator();

        Assert.Equal(100m, agg.Aggregate(steady).Single().VolumeConsistency);
        Assert.InRange(agg.Aggregate(spiky).Single().VolumeConsistency, 0m, 100m);
    }

    [Fact]
    public void PriceChange_IsZeroWhenFirstOpenIsZero()
    {
        var records = new List<NseMarketRecord> { Rec("ZRO", "2026-08-10", 0, 10) };

        Assert.Equal(0m, new SevenDayAggregator().Aggregate(records).Single().PriceChangePercentage);
    }
}
