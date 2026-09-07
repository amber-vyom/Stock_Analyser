using Stock_Analysis.Engine.Database;
using Stock_Analysis.Engine.DSA;
using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis;

public class StockPredictionService
{
    private readonly ProcessedDataRepository repository;
    private readonly StockRankingService rankingService;

    public StockPredictionService()
    {
        repository =
            new ProcessedDataRepository();

        rankingService =
            new StockRankingService();
    }

    public async Task<List<StockFilterResult>>
        GetTopStocksAsync(
            int topCount = 10)
    {
        // ==========================================
        // READ ALL PROCESSED STOCKS FROM DATABASE
        // ==========================================

        List<SevenDayStockData> stocks =
            await repository.GetAllRecordsAsync();

        if (stocks.Count == 0)
        {
            return new List<StockFilterResult>();
        }

        // ==========================================
        // RUN 10 FILTERS + PRIORITY QUEUE
        // ==========================================

        List<StockFilterResult> topStocks =
            rankingService.RankStocks(
                stocks,
                topCount);

        return topStocks;
    }
}