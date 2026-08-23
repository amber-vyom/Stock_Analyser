using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Database;

public class ProcessedDataTest
{
    private readonly ProcessedDataRepository repository;

    public ProcessedDataTest()
    {
        repository = new ProcessedDataRepository();
    }

    public async Task<int> SaveAsync(
        List<SevenDayStockData> records)
    {
        if (records == null || records.Count == 0)
        {
            return 0;
        }

        int inserted =
            await repository.InsertRecordsAsync(records);

        return inserted;
    }
}