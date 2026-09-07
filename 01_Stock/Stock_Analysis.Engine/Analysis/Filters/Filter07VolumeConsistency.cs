using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis.Filters;

public class Filter07VolumeConsistency
{
    private const decimal MinimumVolumeConsistency = 60m;

    public bool Passes(
        SevenDayStockData stock)
    {
        if (stock == null)
        {
            return false;
        }

        return stock.VolumeConsistency >=
               MinimumVolumeConsistency;
    }
}