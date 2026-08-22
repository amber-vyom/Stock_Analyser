namespace Stock_Analysis.Engine.Models;

public class NseMarketRecord
{
    public DateTime TradeDate { get; set; }

    public string Symbol { get; set; } = "";

    public string Series { get; set; } = "";

    public string Isin { get; set; } = "";

    public string InstrumentType { get; set; } = "";

    public decimal OpenPrice { get; set; }

    public decimal HighPrice { get; set; }

    public decimal LowPrice { get; set; }

    public decimal ClosePrice { get; set; }

    public decimal LastPrice { get; set; }

    public decimal PreviousClose { get; set; }

    public decimal SettlementPrice { get; set; }

    public long TradingVolume { get; set; }

    public decimal TradedValue { get; set; }

    public long NumberOfTrades { get; set; }
}