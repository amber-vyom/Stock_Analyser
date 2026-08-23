using Microsoft.Data.SqlClient;
using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Database;

public class RawDataRepository
{
    private readonly DatabaseConnection databaseConnection;

    public RawDataRepository()
    {
        databaseConnection =
            new DatabaseConnection();
    }

    public async Task<int> InsertRecordsAsync(
        List<NseMarketRecord> records)
    {
        if (records == null || records.Count == 0)
        {
            return 0;
        }

        int insertedCount = 0;

        using SqlConnection connection =
            new SqlConnection(
                databaseConnection.ConnectionString);

        await connection.OpenAsync();

        foreach (NseMarketRecord record in records)
        {
            string sql = @"
                INSERT INTO dbo.RawData
                (
                    TradeDate,
                    Symbol,
                    Series,
                    Isin,
                    InstrumentType,
                    OpenPrice,
                    HighPrice,
                    LowPrice,
                    ClosePrice,
                    LastPrice,
                    PreviousClose,
                    SettlementPrice,
                    TradingVolume,
                    TradedValue,
                    NumberOfTrades
                )
                SELECT
                    @TradeDate,
                    @Symbol,
                    @Series,
                    @Isin,
                    @InstrumentType,
                    @OpenPrice,
                    @HighPrice,
                    @LowPrice,
                    @ClosePrice,
                    @LastPrice,
                    @PreviousClose,
                    @SettlementPrice,
                    @TradingVolume,
                    @TradedValue,
                    @NumberOfTrades
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM dbo.RawData
                    WHERE TradeDate = @TradeDate
                      AND Symbol = @Symbol
                );";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@TradeDate",
                record.TradeDate);

            command.Parameters.AddWithValue(
                "@Symbol",
                record.Symbol);

            command.Parameters.AddWithValue(
                "@Series",
                record.Series);

            command.Parameters.AddWithValue(
                "@Isin",
                record.Isin);

            command.Parameters.AddWithValue(
                "@InstrumentType",
                record.InstrumentType);

            command.Parameters.AddWithValue(
                "@OpenPrice",
                record.OpenPrice);

            command.Parameters.AddWithValue(
                "@HighPrice",
                record.HighPrice);

            command.Parameters.AddWithValue(
                "@LowPrice",
                record.LowPrice);

            command.Parameters.AddWithValue(
                "@ClosePrice",
                record.ClosePrice);

            command.Parameters.AddWithValue(
                "@LastPrice",
                record.LastPrice);

            command.Parameters.AddWithValue(
                "@PreviousClose",
                record.PreviousClose);

            command.Parameters.AddWithValue(
                "@SettlementPrice",
                record.SettlementPrice);

            command.Parameters.AddWithValue(
                "@TradingVolume",
                record.TradingVolume);

            command.Parameters.AddWithValue(
                "@TradedValue",
                record.TradedValue);

            command.Parameters.AddWithValue(
                "@NumberOfTrades",
                record.NumberOfTrades);

            int rowsAffected =
                await command.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                insertedCount++;
            }
        }

        return insertedCount;
    }
}