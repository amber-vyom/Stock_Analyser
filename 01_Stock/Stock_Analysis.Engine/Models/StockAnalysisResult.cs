namespace Stock_Analysis.Engine.Models;

public class StockAnalysisResult
{
    public string Symbol { get; set; } = "";

    public decimal Score { get; set; }

    public int FilterCount { get; set; }

    public string Prediction { get; set; } = "";

    public SevenDayStockData StockData { get; set; } = new();
}