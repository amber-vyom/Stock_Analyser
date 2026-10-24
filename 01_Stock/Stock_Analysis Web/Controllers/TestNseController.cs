using Microsoft.AspNetCore.Mvc;
using Stock_Analysis.Engine.NSE;

namespace Stock_Analysis.Web.Controllers;

[ApiController]
[Route("api/nse")]
public class TestNseController : ControllerBase
{
    private readonly NseDownloader _downloader;

    public TestNseController(NseDownloader downloader)
    {
        _downloader = downloader;
    }

    [HttpGet("download-7-days")]
    public async Task<IActionResult> Download7Days()
    {
        // 1. Find the latest 7 trading days
        var tradingDayFinder = new FindTradingDays();

        var tradingDays =
            tradingDayFinder.GetDays(
                DateTime.Today,
                7);

        // 2. Permanent folders
        string rawFolder =
        NseDataPaths.RawFolder;

        string extractedFolder =
        NseDataPaths.ExtractedFolder;

        Directory.CreateDirectory(rawFolder);
        Directory.CreateDirectory(extractedFolder);

        var downloadedFiles = new List<string>();
        var extractedFiles = new List<string>();

        // 3. Download and extract each trading day
        foreach (var day in tradingDays)
        {
            string date =
                day.ToString("yyyyMMdd");

            string url =
                $"https://nsearchives.nseindia.com/content/cm/" +
                $"BhavCopy_NSE_CM_0_0_0_{date}_F_0000.csv.zip";

            string zipFile =
                Path.Combine(
                    rawFolder,
                    $"NSE_{date}.csv.zip");

            // Download NSE file
            string downloadedFile =
                await _downloader.DownloadAsync(
                    url,
                    zipFile);

            downloadedFiles.Add(downloadedFile);

            // Extract NSE file
            var extractor = new ExtractZip();

            string extractedFile =
                extractor.Extract(
                    downloadedFile,
                    extractedFolder);

            extractedFiles.Add(extractedFile);
        }

        // 4. Return result
        return Ok(new
        {
            Message =
                "7 NSE trading days downloaded and extracted successfully.",

            TradingDays =
                tradingDays.Select(
                    d => d.ToString("yyyy-MM-dd")),

            DownloadedFiles =
                downloadedFiles,

            ExtractedFiles =
                extractedFiles
        });
    }
}