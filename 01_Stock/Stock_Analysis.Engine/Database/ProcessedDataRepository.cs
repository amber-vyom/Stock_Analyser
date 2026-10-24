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

    // =========================================================
    // INSERT PROCESSED DATA
    // =========================================================

    public async Task<int> InsertRecordsAsync(
        List<SevenDayStockData> records)
    {
        if (records == null || records.Count == 0)
        {
            return 0;
        }

        const string sql = @"
            UPDATE dbo.ProcessedData
            SET
                TradingDays = @TradingDays,
                TotalVolume = @TotalVolume,
                AverageDailyVolume = @AverageDailyVolume,
                AverageOpenPrice = @AverageOpenPrice,
                AverageClosePrice = @AverageClosePrice,
                HighestPrice = @HighestPrice,
                LowestPrice = @LowestPrice,
                TotalTrades = @TotalTrades,
                TotalTradedValue = @TotalTradedValue,
                PriceChangePercentage = @PriceChangePercentage,
                VolumeConsistency = @VolumeConsistency
            WHERE Symbol = @Symbol;

            IF @@ROWCOUNT = 0
            BEGIN
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
                VALUES
                (
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
                );
            END";

        int savedCount = 0;

        using SqlConnection connection =
            new SqlConnection(
                databaseConnection.ConnectionString);

        await connection.OpenAsync();

        using SqlTransaction transaction =
            connection.BeginTransaction();

        try
        {
            using SqlCommand command =
                new SqlCommand(sql, connection, transaction);

            command.Parameters.Add("@Symbol", System.Data.SqlDbType.NVarChar, 50);
            command.Parameters.Add("@TradingDays", System.Data.SqlDbType.Int);
            command.Parameters.Add("@TotalVolume", System.Data.SqlDbType.BigInt);
            command.Parameters.Add("@AverageDailyVolume", System.Data.SqlDbType.Decimal);
            command.Parameters.Add("@AverageOpenPrice", System.Data.SqlDbType.Decimal);
            command.Parameters.Add("@AverageClosePrice", System.Data.SqlDbType.Decimal);
            command.Parameters.Add("@HighestPrice", System.Data.SqlDbType.Decimal);
            command.Parameters.Add("@LowestPrice", System.Data.SqlDbType.Decimal);
            command.Parameters.Add("@TotalTrades", System.Data.SqlDbType.BigInt);
            command.Parameters.Add("@TotalTradedValue", System.Data.SqlDbType.Decimal);
            command.Parameters.Add("@PriceChangePercentage", System.Data.SqlDbType.Decimal);
            command.Parameters.Add("@VolumeConsistency", System.Data.SqlDbType.Decimal);

            foreach (var parameter in command.Parameters.Cast<SqlParameter>())
            {
                if (parameter.SqlDbType == System.Data.SqlDbType.Decimal)
                {
                    parameter.Precision = 28;
                    parameter.Scale = 6;
                }
            }

            foreach (SevenDayStockData record in records)
            {
                command.Parameters["@Symbol"].Value = record.Symbol;
                command.Parameters["@TradingDays"].Value = record.TradingDays;
                command.Parameters["@TotalVolume"].Value = record.TotalVolume;
                command.Parameters["@AverageDailyVolume"].Value = record.AverageDailyVolume;
                command.Parameters["@AverageOpenPrice"].Value = record.AverageOpenPrice;
                command.Parameters["@AverageClosePrice"].Value = record.AverageClosePrice;
                command.Parameters["@HighestPrice"].Value = record.HighestPrice;
                command.Parameters["@LowestPrice"].Value = record.LowestPrice;
                command.Parameters["@TotalTrades"].Value = record.TotalTrades;
                command.Parameters["@TotalTradedValue"].Value = record.TotalTradedValue;
                command.Parameters["@PriceChangePercentage"].Value = record.PriceChangePercentage;
                command.Parameters["@VolumeConsistency"].Value = record.VolumeConsistency;

                await command.ExecuteNonQueryAsync();
                savedCount++;
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return savedCount;
    }


    // =========================================================
    // READ ALL PROCESSED STOCK DATA
    // =========================================================

    public async Task<List<SevenDayStockData>>
        GetAllRecordsAsync()
    {
        List<SevenDayStockData> records =
            new List<SevenDayStockData>();

        using SqlConnection connection =
            new SqlConnection(
                databaseConnection.ConnectionString);

        await connection.OpenAsync();

        string sql = @"
            SELECT
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
            FROM dbo.ProcessedData
            ORDER BY Symbol;";

        using SqlCommand command =
            new SqlCommand(sql, connection);

        using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            SevenDayStockData stock =
                new SevenDayStockData();

            stock.Symbol =
                reader["Symbol"].ToString() ?? "";

            stock.TradingDays =
                Convert.ToInt32(
                    reader["TradingDays"]);

            stock.TotalVolume =
                Convert.ToInt64(
                    reader["TotalVolume"]);

            stock.AverageDailyVolume =
                Convert.ToInt64(
                    reader["AverageDailyVolume"]);

            stock.AverageOpenPrice =
                Convert.ToDecimal(
                    reader["AverageOpenPrice"]);

            stock.AverageClosePrice =
                Convert.ToDecimal(
                    reader["AverageClosePrice"]);

            stock.HighestPrice =
                Convert.ToDecimal(
                    reader["HighestPrice"]);

            stock.LowestPrice =
                Convert.ToDecimal(
                    reader["LowestPrice"]);

            stock.TotalTrades =
                Convert.ToInt64(
                    reader["TotalTrades"]);

            stock.TotalTradedValue =
                Convert.ToDecimal(
                    reader["TotalTradedValue"]);

            stock.PriceChangePercentage =
                Convert.ToDecimal(
                    reader["PriceChangePercentage"]);

            stock.VolumeConsistency =
                Convert.ToDecimal(
                    reader["VolumeConsistency"]);

            records.Add(stock);
        }

        return records;
    }
}