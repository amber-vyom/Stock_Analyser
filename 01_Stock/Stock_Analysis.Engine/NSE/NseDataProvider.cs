namespace Stock_Analysis.Engine.NSE;

public class NseDataProvider
{
    private readonly NseDownloader _downloader;

    public NseDataProvider(NseDownloader downloader)
    {
        _downloader = downloader;
    }

    public async Task<string> DownloadDailyMarketDataAsync(
        DateTime date)
    {
        string dateText = date.ToString("yyyyMMdd");

        string url =
            $"https://nsearchives.nseindia.com/content/cm/" +
            $"BhavCopy_NSE_CM_0_0_0_{dateText}_F_0000.csv.zip";

        string folder =
            Path.Combine("Data", "Raw");

        string fileName =
            $"BhavCopy_NSE_CM_{dateText}.csv.zip";

        string filePath =
            Path.Combine(folder, fileName);

        return await _downloader.DownloadAsync(
            url,
            filePath);
    }
}