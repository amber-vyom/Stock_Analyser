using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis;

public class StockFilterEngine
{
    private readonly FundamentalAnalysis fundamentalAnalysis;
    private readonly StatisticalAnalysis statisticalAnalysis;
    private readonly TechnicalAnalysis technicalAnalysis;

    public StockFilterEngine()
    {
        fundamentalAnalysis =
            new FundamentalAnalysis();

        statisticalAnalysis =
            new StatisticalAnalysis();

        technicalAnalysis =
            new TechnicalAnalysis();
    }

    public StockAnalysisResult Analyze(
        SevenDayStockData stock)
    {
        int passedFilters = 0;

        // ==========================================
        // FUNDAMENTAL ANALYSIS
        // ==========================================

        if (fundamentalAnalysis.Passes(stock))
        {
            passedFilters++;
        }

        // ==========================================
        // STATISTICAL ANALYSIS
        // ==========================================

        if (statisticalAnalysis.Passes(stock))
        {
            passedFilters++;
        }

        // ==========================================
        // TECHNICAL ANALYSIS
        // ==========================================

        if (technicalAnalysis.Passes(stock))
        {
            passedFilters++;
        }

        // ==========================================
        // SCORE
        // ==========================================

        decimal score =
            CalculateScore(stock, passedFilters);

        // ==========================================
        // PREDICTION
        // ==========================================

        string prediction =
            GetPrediction(score);

        return new StockAnalysisResult
        {
            Symbol = stock.Symbol,

            Score = score,

            FilterCount =
                passedFilters,

            Prediction =
                prediction,

            StockData =
                stock
        };
    }

    // ==========================================
    // SCORE CALCULATION
    // ==========================================

    private decimal CalculateScore(
        SevenDayStockData stock,
        int passedFilters)
    {
        decimal score = 0;

        // Filter score
        score +=
            passedFilters * 20;

        // Positive price movement
        if (stock.PriceChangePercentage > 0)
        {
            score += 10;
        }

        // Good trading activity
        if (stock.TotalVolume > 0)
        {
            score += 5;
        }

        // Good trade activity
        if (stock.TotalTrades > 0)
        {
            score += 5;
        }

        return score;
    }

    // ==========================================
    // PREDICTION
    // ==========================================

    private string GetPrediction(
        decimal score)
    {
        if (score >= 90)
        {
            return "BUY";
        }

        if (score >= 70)
        {
            return "HOLD";
        }

        return "WATCH";
    }
}