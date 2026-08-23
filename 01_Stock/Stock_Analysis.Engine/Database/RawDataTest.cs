using Stock_Analysis.Engine.Models;
using Stock_Analysis.Engine.NSE;

namespace Stock_Analysis.Engine.Database;

public class RawDataTest
{
    public async Task<int> RunAsync(string csvFilePath)
    {
        // ==========================================
        // CHECK FILE
        // ==========================================

        if (!File.Exists(csvFilePath))
        {
            throw new FileNotFoundException(
                "NSE CSV file was not found.",
                csvFilePath);
        }

        // ==========================================
        // PARSE CSV
        // ==========================================

        var parser =
            new NseParser();

        List<NseMarketRecord> records =
            parser.ReadData(csvFilePath);

        // ==========================================
        // KEEP ONLY EQ RECORDS
        // ==========================================

        List<NseMarketRecord> eqRecords =
            records
                .Where(x => x.Series == "EQ")
                .ToList();

        // ==========================================
        // INSERT INTO DATABASE
        // ==========================================

        var repository =
            new RawDataRepository();

        int inserted =
            await repository.InsertRecordsAsync(
                eqRecords);

        return inserted;
    }
}