namespace Stock_Analysis.Engine.NSE;

public class NseDataProvider
{
    private readonly NseDownloader _downloader;

    public NseDataProvider(
        NseDownloader downloader)
    {
        _downloader = downloader;
    }

    public async Task<string>
        DownloadDailyMarketDataAsync(
            DateTime date)
    {
        string dateText =
            date.ToString("yyyyMMdd");

        string url =
            $"https://nsearchives.nseindia.com/content/cm/" +
            $"BhavCopy_NSE_CM_0_0_0_{dateText}_F_0000.csv.zip";

        string folder =
            Path.Combine(
                "Data",
                "Raw");

        string fileName =
            $"BhavCopy_NSE_CM_0_0_0_" +
            $"{dateText}_F_0000.csv.zip";

        string filePath =
            Path.Combine(
                folder,
                fileName);

        try
        {
            return await _downloader
                .DownloadAsync(
                    url,
                    filePath);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception(
                $"NSE download failed for date: " +
                $"{date:dd-MM-yyyy}\n\n" +
                $"URL:\n{url}\n\n" +
                $"Reason:\n{ex.Message}",
                ex);
        }
    }
}