using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.DSA;

public class SevenDayAggregator
{
    public List<SevenDayStockData> Aggregate(
        List<NseMarketRecord> records)
    {
        // Dictionary:
        // Symbol -> List of daily records
        var stocksBySymbol =
            new Dictionary<string, List<NseMarketRecord>>();

        // ------------------------------------------
        // STEP 1: Keep only normal equity stocks
        // ------------------------------------------

        foreach (var record in records)
        {
            if (string.IsNullOrWhiteSpace(record.Symbol))
            {
                continue;
            }

            // Only equity securities
            if (record.Series != "EQ")
            {
                continue;
            }

            if (!stocksBySymbol.ContainsKey(record.Symbol))
            {
                stocksBySymbol[record.Symbol] =
                    new List<NseMarketRecord>();
            }

            stocksBySymbol[record.Symbol].Add(record);
        }

        // ------------------------------------------
        // STEP 2: Create aggregated stock list
        // ------------------------------------------

        var result =
            new List<SevenDayStockData>();

        foreach (var entry in stocksBySymbol)
        {
            string symbol =
                entry.Key;

            List<NseMarketRecord> stockRecords =
                entry.Value;

            if (stockRecords.Count == 0)
            {
                continue;
            }

            // Sort by trading date
            var orderedRecords =
                stockRecords
                    .OrderBy(x => x.TradeDate)
                    .ToList();

            // --------------------------------------
            // STEP 3: Calculate statistics
            // --------------------------------------

            long totalVolume =
                orderedRecords.Sum(
                    x => x.TradingVolume);

            decimal averageDailyVolume =
                totalVolume /
                (decimal)orderedRecords.Count;

            decimal averageOpenPrice =
                orderedRecords.Average(
                    x => x.OpenPrice);

            decimal averageClosePrice =
                orderedRecords.Average(
                    x => x.ClosePrice);

            decimal highestPrice =
                orderedRecords.Max(
                    x => x.HighPrice);

            decimal lowestPrice =
                orderedRecords.Min(
                    x => x.LowPrice);

            long totalTrades =
                orderedRecords.Sum(
                    x => x.NumberOfTrades);

            decimal totalTradedValue =
                orderedRecords.Sum(
                    x => x.TradedValue);

            // --------------------------------------
            // STEP 4: Price change
            // --------------------------------------

            decimal firstOpenPrice =
                orderedRecords.First().OpenPrice;

            decimal lastClosePrice =
                orderedRecords.Last().ClosePrice;

            decimal priceChangePercentage = 0;

            if (firstOpenPrice != 0)
            {
                priceChangePercentage =
                    ((lastClosePrice -
                      firstOpenPrice)
                     / firstOpenPrice) * 100;
            }

            // --------------------------------------
            // STEP 5: Volume consistency
            // --------------------------------------

            decimal volumeConsistency =
                CalculateVolumeConsistency(
                    orderedRecords);

            // --------------------------------------
            // STEP 6: Create result object
            // --------------------------------------

            result.Add(
                new SevenDayStockData
                {
                    Symbol =
                        symbol,

                    TradingDays =
                        orderedRecords.Count,

                    TotalVolume =
                        totalVolume,

                    AverageDailyVolume =
                        averageDailyVolume,

                    AverageOpenPrice =
                        averageOpenPrice,

                    AverageClosePrice =
                        averageClosePrice,

                    HighestPrice =
                        highestPrice,

                    LowestPrice =
                        lowestPrice,

                    TotalTrades =
                        totalTrades,

                    TotalTradedValue =
                        totalTradedValue,

                    PriceChangePercentage =
                        priceChangePercentage,

                    VolumeConsistency =
                        volumeConsistency
                });
        }

        return result;
    }

    private decimal CalculateVolumeConsistency(
        List<NseMarketRecord> records)
    {
        if (records.Count == 0)
        {
            return 0;
        }

        decimal averageVolume =
            records.Average(
                x => (decimal)x.TradingVolume);

        if (averageVolume == 0)
        {
            return 0;
        }

        decimal totalDifference = 0;

        foreach (var record in records)
        {
            decimal difference =
                Math.Abs(
                    record.TradingVolume -
                    averageVolume);

            totalDifference += difference;
        }

        decimal averageDifference =
            totalDifference /
            records.Count;

        decimal consistency =
            100 -
            ((averageDifference /
              averageVolume) * 100);

        if (consistency < 0)
        {
            consistency = 0;
        }

        if (consistency > 100)
        {
            consistency = 100;
        }

        return consistency;
    }
}