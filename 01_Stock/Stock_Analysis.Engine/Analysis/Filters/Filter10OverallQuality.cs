using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis.Filters;

public class Filter10OverallQuality
{
    private const decimal MinimumPriceChange = 2m;
    private const decimal MinimumVolumeConsistency = 50m;
    private const decimal MinimumAverageDailyVolume = 500_000m;

    public bool Passes(
        SevenDayStockData stock)
    {
        if (stock == null)
        {
            return false;
        }

        bool hasPositiveGrowth =
            stock.PriceChangePercentage >=
            MinimumPriceChange;

        bool hasConsistentVolume =
            stock.VolumeConsistency >=
            MinimumVolumeConsistency;

        bool hasEnoughVolume =
            stock.AverageDailyVolume >=
            MinimumAverageDailyVolume;

        return hasPositiveGrowth &&
               hasConsistentVolume &&
               hasEnoughVolume;
    }
}