using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis.Filters;

public class Filter03StrongReturn
{
    public bool Passes(
        SevenDayStockData stock)
    {
        if (stock == null)
        {
            return false;
        }

        // Stock must have at least
        // 3% positive price change.
        return stock.PriceChangePercentage >= 3;
    }
}