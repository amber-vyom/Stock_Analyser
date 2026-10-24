using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis;

public record ScoreComponent(string Name, decimal Points, decimal Max);

public class StockScorer
{
    private readonly ScoringOptions options;

    public StockScorer(ScoringOptions? options = null)
    {
        this.options = options ?? new ScoringOptions();
    }

    public decimal Score(SevenDayStockData stock, int passedFilters)
    {
        decimal total = Breakdown(stock, passedFilters).Sum(c => c.Points);

        return Math.Round(total, 2);
    }

    public List<ScoreComponent> Breakdown(SevenDayStockData stock, int passedFilters)
    {
        if (stock == null)
        {
            return new List<ScoreComponent>();
        }

        decimal consistency = Clamp01(stock.VolumeConsistency / 100m);
        decimal returnStrength = Ratio(stock.PriceChangePercentage, options.StrongReturnFullPercent);

        return new List<ScoreComponent>
        {
            new("Data completeness", options.CompletenessWeight * Ratio(stock.TradingDays, options.ExpectedTradingDays), options.CompletenessWeight),
            new("Positive return", options.PositiveReturnWeight * Ratio(stock.PriceChangePercentage, options.PositiveReturnFullPercent), options.PositiveReturnWeight),
            new("Strong return", options.StrongReturnWeight * returnStrength, options.StrongReturnWeight),
            new("Close vs open", options.CloseVersusOpenWeight * Ratio(CloseVersusOpenPercent(stock), options.CloseVersusOpenFullPercent), options.CloseVersusOpenWeight),
            new("Price stability", options.PriceStabilityWeight * Stability(stock), options.PriceStabilityWeight),
            new("Trading volume", options.VolumeWeight * LogScale(stock.AverageDailyVolume, options.VolumeFloor, options.VolumeFull), options.VolumeWeight),
            new("Volume consistency", options.VolumeConsistencyWeight * consistency, options.VolumeConsistencyWeight),
            new("Momentum with volume", options.MomentumWeight * returnStrength * consistency, options.MomentumWeight),
            new("Traded value", options.TradedValueWeight * LogScale(stock.TotalTradedValue, options.TradedValueFloor, options.TradedValueFull), options.TradedValueWeight),
            new("Overall quality", options.OverallQualityWeight * Clamp01(passedFilters / 10m), options.OverallQualityWeight)
        };
    }

    private static decimal Clamp01(decimal value)
    {
        if (value < 0m) return 0m;
        if (value > 1m) return 1m;
        return value;
    }

    private static decimal Ratio(decimal value, decimal full)
    {
        if (full <= 0m) return 0m;
        return Clamp01(value / full);
    }

    private static decimal CloseVersusOpenPercent(SevenDayStockData stock)
    {
        if (stock.AverageOpenPrice <= 0m) return 0m;
        return (stock.AverageClosePrice - stock.AverageOpenPrice) / stock.AverageOpenPrice * 100m;
    }

    private decimal Stability(SevenDayStockData stock)
    {
        if (stock.LowestPrice <= 0m || stock.HighestPrice < stock.LowestPrice) return 0m;
        decimal range = (stock.HighestPrice - stock.LowestPrice) / stock.LowestPrice * 100m;
        if (options.StabilityZeroRangePercent <= 0m) return 0m;
        return Clamp01(1m - range / options.StabilityZeroRangePercent);
    }

    private static decimal LogScale(decimal value, decimal floor, decimal full)
    {
        if (value <= 0m || floor <= 0m || full <= floor) return 0m;
        double position = (Math.Log10((double)value) - Math.Log10((double)floor))
                          / (Math.Log10((double)full) - Math.Log10((double)floor));
        return Clamp01((decimal)position);
    }
}