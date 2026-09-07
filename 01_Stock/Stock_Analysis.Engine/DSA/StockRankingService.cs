using Stock_Analysis.Engine.Analysis;
using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.DSA;

public class StockRankingService
{
    private readonly StockFilterEngine filterEngine;

    public StockRankingService()
    {
        filterEngine = new StockFilterEngine();
    }

    public List<StockFilterResult> RankStocks(
        List<SevenDayStockData> stocks,
        int topCount)
    {
        var priorityQueue =
            new PriorityQueue<
                StockFilterResult,
                int>();

        foreach (SevenDayStockData stock in stocks)
        {
            StockFilterResult result =
                filterEngine.Analyse(stock);

            // Negative priority means:
            // Higher score comes out first.
            int priority =
                -result.PassedFilters;

            priorityQueue.Enqueue(
                result,
                priority);
        }

        var rankedStocks =
            new List<StockFilterResult>();

        while (priorityQueue.Count > 0 &&
               rankedStocks.Count < topCount)
        {
            rankedStocks.Add(
                priorityQueue.Dequeue());
        }

        return rankedStocks;
    }
}