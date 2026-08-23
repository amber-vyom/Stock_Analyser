using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis;

public class FundamentalAnalysis
{
    public bool Passes(
        SevenDayStockData stock)
    {
        if (stock == null)
        {
            return false;
        }

        // Basic liquidity check
        if (stock.TotalVolume <= 0)
        {
            return false;
        }

        // Stock must have sufficient trading history
        if (stock.TradingDays < 7)
        {
            return false;
        }

        // Stock must have actual trading activity
        if (stock.TotalTrades <= 0)
        {
            return false;
        }

        return true;
    }
}