using Stock_Analysis.Engine.DSA;
using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Tests;

public class DsaTests
{
    private static SevenDayStockData Stock(string symbol, decimal change = 6m) =>
        new()
        {
            Symbol = symbol,
            TradingDays = 7,
            AverageDailyVolume = 2_000_000m,
            AverageOpenPrice = 100m,
            AverageClosePrice = 104m,
            HighestPrice = 110m,
            LowestPrice = 98m,
            TotalTradedValue = 50_000_000m,
            PriceChangePercentage = change,
            VolumeConsistency = 80m
        };

    [Fact]
    public void MergeSort_SortsAscending()
    {
        var result = MergeSorter.Sort(new List<int> { 5, 2, 9, 1, 5, 6, 0 }, (a, b) => a.CompareTo(b));

        Assert.Equal(new[] { 0, 1, 2, 5, 5, 6, 9 }, result);
    }

    [Fact]
    public void MergeSort_HandlesEmptySingleAndDoesNotChangeInput()
    {
        var input = new List<int> { 3, 1, 2 };

        MergeSorter.Sort(input, (a, b) => a.CompareTo(b));

        Assert.Equal(new[] { 3, 1, 2 }, input);
        Assert.Empty(MergeSorter.Sort(new List<int>(), (a, b) => a.CompareTo(b)));
        Assert.Single(MergeSorter.Sort(new List<int> { 7 }, (a, b) => a.CompareTo(b)));
    }

    [Fact]
    public void MergeSort_IsStable()
    {
        var items = new List<(int Key, string Tag)> { (1, "a"), (0, "x"), (1, "b"), (0, "y") };

        var result = MergeSorter.Sort(items, (p, q) => p.Key.CompareTo(q.Key));

        Assert.Equal(new[] { "x", "y", "a", "b" }, result.Select(r => r.Tag));
    }

    [Fact]
    public void MergeSort_MatchesBuiltInSortOnRandomData()
    {
        var random = new Random(42);
        var data = Enumerable.Range(0, 500).Select(_ => random.Next(1000)).ToList();

        var result = MergeSorter.Sort(data, (a, b) => a.CompareTo(b));

        Assert.Equal(data.OrderBy(x => x), result);
    }

    [Fact]
    public void BinarySearch_FindsExistingSymbolCaseInsensitively()
    {
        var sorted = SymbolSearch.SortBySymbol(
            new List<SevenDayStockData> { Stock("TCS"), Stock("INFY"), Stock("SBIN"), Stock("HFCL") });

        Assert.Equal("SBIN", SymbolSearch.BinarySearch(sorted, " sbin ")?.Symbol);
        Assert.Equal("HFCL", SymbolSearch.BinarySearch(sorted, "HFCL")?.Symbol);
    }

    [Fact]
    public void BinarySearch_ReturnsNullWhenMissingOrEmpty()
    {
        var sorted = SymbolSearch.SortBySymbol(new List<SevenDayStockData> { Stock("TCS") });

        Assert.Null(SymbolSearch.BinarySearch(sorted, "NOPE"));
        Assert.Null(SymbolSearch.BinarySearch(sorted, ""));
        Assert.Null(SymbolSearch.BinarySearch(new List<SevenDayStockData>(), "TCS"));
    }

    [Fact]
    public void RankAllWithMergeSort_MatchesPriorityQueueOrdering()
    {
        var stocks = Enumerable.Range(0, 40)
            .Select(i => Stock($"S{i:00}", change: (i * 7) % 13))
            .ToList();

        var service = new StockRankingService();
        var fromHeap = service.RankStocks(stocks, stocks.Count).Select(r => r.Symbol);
        var fromSort = service.RankAllWithMergeSort(stocks).Select(r => r.Symbol);

        Assert.Equal(fromHeap, fromSort);
    }
}
