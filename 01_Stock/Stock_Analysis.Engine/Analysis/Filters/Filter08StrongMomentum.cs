using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis.Filters;

public class Filter08StrongMomentum
{
    private const decimal MinimumPriceChangePercentage = 5m;

    public bool Passes(
        SevenDayStockData stock)
    {
        if (stock == null)
        {
            return false;
        }

        return stock.PriceChangePercentage >=
               MinimumPriceChangePercentage;
    }
}