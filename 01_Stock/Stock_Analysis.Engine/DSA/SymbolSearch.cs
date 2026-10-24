using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.DSA;

public static class SymbolSearch
{
    public static List<SevenDayStockData> SortBySymbol(
        IList<SevenDayStockData> stocks)
    {
        return MergeSorter.Sort(
            stocks,
            (a, b) => string.CompareOrdinal(a.Symbol, b.Symbol));
    }

    public static SevenDayStockData? BinarySearch(
        IList<SevenDayStockData> sortedBySymbol,
        string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return null;
        }

        string target = symbol.Trim().ToUpperInvariant();

        int low = 0;
        int high = sortedBySymbol.Count - 1;

        while (low <= high)
        {
            int middle = low + (high - low) / 2;

            int comparison =
                string.CompareOrdinal(
                    sortedBySymbol[middle].Symbol,
                    target);

            if (comparison == 0)
            {
                return sortedBySymbol[middle];
            }

            if (comparison < 0)
            {
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }

        return null;
    }
}
