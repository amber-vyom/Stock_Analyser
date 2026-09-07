using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis.Filters;

public class Filter09HighTradedValue
{
    private const decimal MinimumTotalTradedValue = 10_000_000m;

    public bool Passes(
        SevenDayStockData stock)
    {
        if (stock == null)
        {
            return false;
        }

        return stock.TotalTradedValue >=
               MinimumTotalTradedValue;
    }
}