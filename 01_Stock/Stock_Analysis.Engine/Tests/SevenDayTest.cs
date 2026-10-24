using System.Text;
using Stock_Analysis.Engine.Database;
using Stock_Analysis.Engine.DSA;
using Stock_Analysis.Engine.Models;
using Stock_Analysis.Engine.NSE;

namespace Stock_Analysis.Engine.Tests;

public class SevenDayTest
{
    private readonly string extractedFolder =
        NseDataPaths.ExtractedFolder;

    // ==================================================
    // EXISTING 7 DAY TEST
    // ==================================================

    public string Run()
    {
        try
        {
            List<SevenDayStockData> stocks =
                GetAggregatedStocks(
                    out int totalRecords,
                    out int eqRecords,
                    out int csvFileCount);

            var output =
                new StringBuilder();

            output.AppendLine(
                "================================");

            output.AppendLine(
                "       7 DAY NSE TEST");

            output.AppendLine(
                "================================");

            output.AppendLine();

            output.AppendLine(
                $"CSV files selected: " +
                $"{csvFileCount}");

            output.AppendLine();

            output.AppendLine(
                $"Total records: " +
                $"{totalRecords}");

            output.AppendLine(
                $"Total EQ records: " +
                $"{eqRecords}");

            output.AppendLine();

            // ==========================================
            // AGGREGATED STOCK COUNT
            // ==========================================

            output.AppendLine(
                $"Unique EQ stocks: " +
                $"{stocks.Count}");

            // ==========================================
            // TRADING DAY COUNTS
            // ==========================================

            int sevenDayStocks =
                stocks.Count(
                    x => x.TradingDays == 7);

            int lessThanSeven =
                stocks.Count(
                    x => x.TradingDays < 7);

            int moreThanSeven =
                stocks.Count(
                    x => x.TradingDays > 7);

            output.AppendLine(
                $"Stocks with 7 trading days: " +
                $"{sevenDayStocks}");

            output.AppendLine(
                $"Stocks with less than 7 days: " +
                $"{lessThanSeven}");

            output.AppendLine(
                $"Stocks with more than 7 days: " +
                $"{moreThanSeven}");

            // ==========================================
            // DISPLAY FIRST 10
            // ==========================================

            output.AppendLine();

            output.AppendLine(
                "First 10 aggregated stocks:");

            output.AppendLine(
                "------------------------------------------------");

            foreach (SevenDayStockData stock in
                     stocks.Take(10))
            {
                output.AppendLine(
                    $"{stock.Symbol,-15}" +
                    $" Days={stock.TradingDays,-3}" +
                    $" Volume={stock.TotalVolume,-15}" +
                    $" AvgOpen={stock.AverageOpenPrice:F2}" +
                    $" AvgClose={stock.AverageClosePrice:F2}" +
                    $" Change={stock.PriceChangePercentage:F2}%");
            }

            output.AppendLine();

            output.AppendLine(
                "================================");

            output.AppendLine(
                "7 DAY TEST COMPLETED");

            output.AppendLine(
                "================================");

            return output.ToString();
        }
        catch (Exception ex)
        {
            return
                "ERROR:\n\n" +
                ex.Message;
        }
    }

    // ==================================================
    // SAVE 7 DAY DATA TO PROCESSED DATABASE
    // ==================================================

    public async Task<int> SaveToDatabaseAsync()
    {
        List<SevenDayStockData> stocks =
            GetAggregatedStocks(
                out _,
                out _,
                out _);

        if (stocks.Count == 0)
        {
            return 0;
        }

        var repository =
            new ProcessedDataRepository();

        int inserted =
            await repository.InsertRecordsAsync(
                stocks);

        return inserted;
    }

    // ==================================================
    // GET AGGREGATED STOCKS
    // ==================================================

    private List<SevenDayStockData> GetAggregatedStocks(
        out int totalRecords,
        out int eqRecords,
        out int csvFileCount)
    {
        totalRecords = 0;
        eqRecords = 0;
        csvFileCount = 0;

        // ==========================================
        // CHECK EXTRACTED FOLDER
        // ==========================================

        if (!Directory.Exists(extractedFolder))
        {
            throw new DirectoryNotFoundException(
                "Extracted folder does not exist.");
        }

        // ==========================================
        // USE THE LATEST 7 DOWNLOADED REPORTS
        // (holiday-safe: based on files that exist)
        // ==========================================

        List<string> csvFiles =
            NseReportFiles.GetLatestCsvFiles(
                Directory.GetFiles(
                    extractedFolder,
                    "*.csv"),
                7);

        csvFileCount =
            csvFiles.Count;

        if (csvFiles.Count == 0)
        {
            throw new FileNotFoundException(
                "No NSE CSV files were found.\n\n" +
                "Click 'Download Latest 7 Days' first.");
        }

        // ==========================================
        // PARSE ALL FILES
        // ==========================================

        var parser =
            new NseParser();

        var allRecords =
            new List<NseMarketRecord>();

        foreach (string csvFile in csvFiles)
        {
            List<NseMarketRecord> records =
                parser.ReadData(csvFile);

            allRecords.AddRange(
                records);
        }

        totalRecords =
            allRecords.Count;

        // ==========================================
        // COUNT EQ RECORDS
        // ==========================================

        eqRecords =
            allRecords.Count(
                x => x.Series == "EQ");

        // ==========================================
        // AGGREGATE
        // ==========================================

        var aggregator =
            new SevenDayAggregator();

        List<SevenDayStockData> stocks =
            aggregator.Aggregate(
                allRecords);

        return stocks;
    }
}
