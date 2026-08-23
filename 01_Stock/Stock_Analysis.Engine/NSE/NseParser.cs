using CsvHelper;
using CsvHelper.Configuration;
using Stock_Analysis.Engine.Models;
using System.Globalization;

namespace Stock_Analysis.Engine.NSE;

public class NseParser
{
    public List<NseMarketRecord> ReadData(string filePath)
    {
        var records = new List<NseMarketRecord>();

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "NSE CSV file was not found.",
                filePath);
        }

        using var reader =
            new StreamReader(filePath);

        var config =
            new CsvConfiguration(
                CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null
            };

        using var csv =
            new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            string tradeDateText =
                csv.GetField("TradDt") ?? "";

            if (!DateTime.TryParse(
                tradeDateText,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime tradeDate))
            {
                continue;
            }

            string symbol =
                csv.GetField("TckrSymb") ?? "";

            string series =
                csv.GetField("SctySrs") ?? "";

            // Ignore records without a symbol.
            if (string.IsNullOrWhiteSpace(symbol))
            {
                continue;
            }

            var record =
                new NseMarketRecord
                {
                    TradeDate = tradeDate,

                    Symbol = symbol,

                    Series = series,

                    Isin =
                        csv.GetField("ISIN") ?? "",

                    InstrumentType =
                        csv.GetField("FinInstrmTp") ?? "",

                    OpenPrice =
                        GetDecimal(
                            csv,
                            "OpnPric"),

                    HighPrice =
                        GetDecimal(
                            csv,
                            "HghPric"),

                    LowPrice =
                        GetDecimal(
                            csv,
                            "LwPric"),

                    ClosePrice =
                        GetDecimal(
                            csv,
                            "ClsPric"),

                    LastPrice =
                        GetDecimal(
                            csv,
                            "LastPric"),

                    PreviousClose =
                        GetDecimal(
                            csv,
                            "PrvsClsgPric"),

                    SettlementPrice =
                        GetDecimal(
                            csv,
                            "SttlmPric"),

                    TradingVolume =
                        GetLong(
                            csv,
                            "TtlTradgVol"),

                    TradedValue =
                        GetDecimal(
                            csv,
                            "TtlTrfVal"),

                    NumberOfTrades =
                        GetLong(
                            csv,
                            "TtlNbOfTxsExctd")
                };

            records.Add(record);
        }

        return records;
    }

    private static decimal GetDecimal(
        CsvReader csv,
        string columnName)
    {
        string value =
            csv.GetField(columnName) ?? "";

        if (decimal.TryParse(
            value,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out decimal result))
        {
            return result;
        }

        return 0;
    }

    private static long GetLong(
        CsvReader csv,
        string columnName)
    {
        string value =
            csv.GetField(columnName) ?? "";

        if (long.TryParse(
            value,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out long result))
        {
            return result;
        }

        return 0;
    }
}