using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis.Filters;

public class Filter04CloseAboveOpen
{
    public bool Passes(
        SevenDayStockData stock)
    {
        if (stock == null)
        {
            return false;
        }

        // Average closing price must be
        // greater than average opening price.
        return stock.AverageClosePrice >
               stock.AverageOpenPrice;
    }
}