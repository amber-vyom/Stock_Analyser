using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis;

public class StatisticalAnalysis
{
    public bool Passes(
        SevenDayStockData stock)
    {
        if (stock == null)
        {
            return false;
        }

        // Stock must have enough trading days
        if (stock.TradingDays < 7)
        {
            return false;
        }

        // Average daily volume must be positive
        if (stock.AverageDailyVolume <= 0)
        {
            return false;
        }

        // There must be a valid price range
        if (stock.HighestPrice <= 0 ||
            stock.LowestPrice <= 0)
        {
            return false;
        }

        // Highest price should not be below lowest price
        if (stock.HighestPrice <
            stock.LowestPrice)
        {
            return false;
        }

        // Average close price must be positive
        if (stock.AverageClosePrice <= 0)
        {
            return false;
        }

        return true;
    }
}