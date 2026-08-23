using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis.Filters;

public class Filter06VolumeActivity
{
    private const long MinimumAverageDailyVolume = 1_000_000;

    public bool Passes(
        SevenDayStockData stock)
    {
        if (stock == null)
        {
            return false;
        }

        return stock.AverageDailyVolume >=
               MinimumAverageDailyVolume;
    }
}