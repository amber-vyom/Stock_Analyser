using Stock_Analysis.Engine.NSE;
using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Tests;

public class NseParserTest
{
    public void Run()
    {
        string csvFile =
            @"A:\01_Vyom\03_Projects\01_Stock\Stock_Data\NSE\Extracted\BhavCopy_NSE_CM_0_0_0_20260814_F_0000.csv";

        Console.WriteLine("====================================");
        Console.WriteLine("       NSE PARSER TEST");
        Console.WriteLine("====================================");

        if (!File.Exists(csvFile))
        {
            Console.WriteLine("CSV file not found:");
            Console.WriteLine(csvFile);
            return;
        }

        var parser = new NseParser();

        List<NseMarketRecord> records =
            parser.ReadData(csvFile);

        Console.WriteLine();
        Console.WriteLine(
            $"Total parsed records: {records.Count}");

        int equityRecords =
            records.Count(x => x.Series == "EQ");

        Console.WriteLine(
            $"EQ records: {equityRecords}");

        int nonEquityRecords =
            records.Count(x => x.Series != "EQ");

        Console.WriteLine(
            $"Non-EQ records: {nonEquityRecords}");

        Console.WriteLine();
        Console.WriteLine("First 10 EQ stocks:");
        Console.WriteLine("------------------------------------");

        foreach (var record in records
                     .Where(x => x.Series == "EQ")
                     .Take(10))
        {
            Console.WriteLine(
                $"{record.Symbol,-15}" +
                $" Open={record.OpenPrice,-10:F2}" +
                $" Close={record.ClosePrice,-10:F2}" +
                $" Volume={record.TradingVolume}");
        }

        Console.WriteLine();
        Console.WriteLine("====================================");
        Console.WriteLine("Parser test completed.");
        Console.WriteLine("====================================");
    }
}