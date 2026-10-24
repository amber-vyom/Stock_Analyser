using Stock_Analysis.Engine.Analysis;
using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.DSA;

public class StockRankingService
{
    private readonly StockFilterEngine filterEngine;

    private readonly ScoringOptions options;

    public StockRankingService(ScoringOptions? scoringOptions = null)
    {
        options = scoringOptions ?? new ScoringOptions();
        filterEngine = new StockFilterEngine(options);
    }

    public List<StockFilterResult> RankStocks(
        List<SevenDayStockData> stocks,
        int topCount)
    {
        var priorityQueue =
            new PriorityQueue<
                StockFilterResult,
                (decimal ScorePriority, int FilterPriority, string Symbol)>(
                new RankComparer());

        foreach (SevenDayStockData stock in stocks)
        {
            if (stock == null ||
                stock.TradingDays < options.MinimumTradingDaysToRank)
            {
                continue;
            }

            StockFilterResult result =
                filterEngine.Analyse(stock);

            priorityQueue.Enqueue(
                result,
                (-result.Score, -result.PassedFilters, result.Symbol));
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

    private sealed class RankComparer :
        IComparer<(decimal ScorePriority, int FilterPriority, string Symbol)>
    {
        public int Compare(
            (decimal ScorePriority, int FilterPriority, string Symbol) x,
            (decimal ScorePriority, int FilterPriority, string Symbol) y)
        {
            int byScore = x.ScorePriority.CompareTo(y.ScorePriority);
            if (byScore != 0) return byScore;

            int byFilters = x.FilterPriority.CompareTo(y.FilterPriority);
            if (byFilters != 0) return byFilters;

            return string.CompareOrdinal(x.Symbol, y.Symbol);
        }
    }
}
