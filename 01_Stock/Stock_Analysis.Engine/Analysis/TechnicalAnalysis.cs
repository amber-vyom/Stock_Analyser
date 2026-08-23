using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis;

public class TechnicalAnalysis
{
    public bool Passes(
        SevenDayStockData stock)
    {
        if (stock == null)
        {
            return false;
        }

        // Stock must have at least 7 trading days
        if (stock.TradingDays < 7)
        {
            return false;
        }

        // Average prices must be valid
        if (stock.AverageOpenPrice <= 0 ||
            stock.AverageClosePrice <= 0)
        {
            return false;
        }

        // Closing price should be above opening price
        if (stock.AverageClosePrice <=
            stock.AverageOpenPrice)
        {
            return false;
        }

        // Stock must have positive price movement
        if (stock.PriceChangePercentage <= 0)
        {
            return false;
        }

        // Stock must have trading activity
        if (stock.TotalVolume <= 0 ||
            stock.TotalTrades <= 0)
        {
            return false;
        }

        return true;
    }
}