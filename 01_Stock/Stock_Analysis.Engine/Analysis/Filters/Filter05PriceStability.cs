using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis.Filters;

public class Filter05PriceStability
{
    public bool Passes(
        SevenDayStockData stock)
    {
        if (stock == null)
        {
            return false;
        }

        if (stock.LowestPrice <= 0)
        {
            return false;
        }

        decimal priceRangePercentage =
            ((stock.HighestPrice - stock.LowestPrice)
            / stock.LowestPrice) * 100;

        // Stock should have a price range
        // of 15% or less over the 7-day period.
        return priceRangePercentage <= 15;
    }
}