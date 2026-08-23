namespace Stock_Analysis.Engine.Models;

public class SevenDayStockData
{
    public string Symbol { get; set; } = "";

    public int TradingDays { get; set; }

    public long TotalVolume { get; set; }

    public decimal AverageDailyVolume { get; set; }

    public decimal AverageOpenPrice { get; set; }

    public decimal AverageClosePrice { get; set; }

    public decimal HighestPrice { get; set; }

    public decimal LowestPrice { get; set; }


    public long TotalTrades { get; set; }

    public decimal TotalTradedValue { get; set; }

    public decimal PriceChangePercentage { get; set; }

    public decimal VolumeConsistency { get; set; }
}