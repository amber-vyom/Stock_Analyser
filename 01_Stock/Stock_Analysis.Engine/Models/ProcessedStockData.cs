namespace Stock_Analysis.Engine.Models;

public class ProcessedStockData
{
    public int Id { get; set; }

    public string Symbol { get; set; } = "";

    public DateTime AnalysisDate { get; set; }

    public decimal AverageVolume { get; set; }

    public decimal AverageOpenPrice { get; set; }

    public decimal AverageClosePrice { get; set; }

    public decimal AverageDeliveryPercentage { get; set; }

    public decimal PriceChangePercentage { get; set; }

    public decimal VolumeScore { get; set; }

    public decimal PriceScore { get; set; }

    public decimal DeliveryScore { get; set; }

    public decimal MomentumScore { get; set; }

    public decimal FinalScore { get; set; }

    public bool FilterPassed { get; set; }

    public DateTime ProcessedAt { get; set; }
}