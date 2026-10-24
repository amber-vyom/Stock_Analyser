namespace Stock_Analysis.Engine.Analysis;

public class ScoringOptions
{
    public decimal CompletenessWeight { get; set; } = 10m;
    public decimal PositiveReturnWeight { get; set; } = 8m;
    public decimal StrongReturnWeight { get; set; } = 12m;
    public decimal CloseVersusOpenWeight { get; set; } = 10m;
    public decimal PriceStabilityWeight { get; set; } = 10m;
    public decimal VolumeWeight { get; set; } = 10m;
    public decimal VolumeConsistencyWeight { get; set; } = 10m;
    public decimal MomentumWeight { get; set; } = 10m;
    public decimal TradedValueWeight { get; set; } = 10m;
    public decimal OverallQualityWeight { get; set; } = 10m;

    public int MinimumTradingDaysToRank { get; set; } = 7;
    public int ExpectedTradingDays { get; set; } = 7;
    public decimal PositiveReturnFullPercent { get; set; } = 2m;
    public decimal StrongReturnFullPercent { get; set; } = 10m;
    public decimal CloseVersusOpenFullPercent { get; set; } = 5m;
    public decimal StabilityZeroRangePercent { get; set; } = 30m;
    public decimal VolumeFloor { get; set; } = 100_000m;
    public decimal VolumeFull { get; set; } = 10_000_000m;
    public decimal TradedValueFloor { get; set; } = 1_000_000m;
    public decimal TradedValueFull { get; set; } = 1_000_000_000m;
}
