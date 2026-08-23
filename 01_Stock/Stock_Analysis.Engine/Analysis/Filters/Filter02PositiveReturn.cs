using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis.Filters;

public class Filter02PositiveReturn
{
    public bool Passes(
        SevenDayStockData stock)
    {
        if (stock == null)
        {
            return false;
        }

        // Stock must have a positive
        // 7-day price change.
        return stock.PriceChangePercentage > 0;
    }
}