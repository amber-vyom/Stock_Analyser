using Microsoft.Data.SqlClient;
using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Database;

public class ProcessedDataRepository
{
    private readonly DatabaseConnection databaseConnection;

    public ProcessedDataRepository()
    {
        databaseConnection =
            new DatabaseConnection();
    }

    public async Task<int> InsertRecordsAsync(
        List<SevenDayStockData> records)
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

        foreach (SevenDayStockData record in records)
        {
            string sql = @"
                INSERT INTO dbo.ProcessedData
                (
                    Symbol,
                    TradingDays,
                    TotalVolume,
                    AverageDailyVolume,
                    AverageOpenPrice,
                    AverageClosePrice,
                    HighestPrice,
                    LowestPrice,
                    TotalTrades,
                    TotalTradedValue,
                    PriceChangePercentage,
                    VolumeConsistency
                )
                SELECT
                    @Symbol,
                    @TradingDays,
                    @TotalVolume,
                    @AverageDailyVolume,
                    @AverageOpenPrice,
                    @AverageClosePrice,
                    @HighestPrice,
                    @LowestPrice,
                    @TotalTrades,
                    @TotalTradedValue,
                    @PriceChangePercentage,
                    @VolumeConsistency
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM dbo.ProcessedData
                    WHERE Symbol = @Symbol
                );";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@Symbol",
                record.Symbol);

            command.Parameters.AddWithValue(
                "@TradingDays",
                record.TradingDays);

            command.Parameters.AddWithValue(
                "@TotalVolume",
                record.TotalVolume);

            command.Parameters.AddWithValue(
                "@AverageDailyVolume",
                record.AverageDailyVolume);

            command.Parameters.AddWithValue(
                "@AverageOpenPrice",
                record.AverageOpenPrice);

            command.Parameters.AddWithValue(
                "@AverageClosePrice",
                record.AverageClosePrice);

            command.Parameters.AddWithValue(
                "@HighestPrice",
                record.HighestPrice);

            command.Parameters.AddWithValue(
                "@LowestPrice",
                record.LowestPrice);

            command.Parameters.AddWithValue(
                "@TotalTrades",
                record.TotalTrades);

            command.Parameters.AddWithValue(
                "@TotalTradedValue",
                record.TotalTradedValue);

            command.Parameters.AddWithValue(
                "@PriceChangePercentage",
                record.PriceChangePercentage);

            command.Parameters.AddWithValue(
                "@VolumeConsistency",
                record.VolumeConsistency);

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