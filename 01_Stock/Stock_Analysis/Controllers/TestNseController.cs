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

    [HttpGet("download")]
    public async Task<IActionResult> Download()
    {
        string date = "20260713";

        string url =
            $"https://nsearchives.nseindia.com/content/cm/" +
            $"BhavCopy_NSE_CM_0_0_0_{date}_F_0000.csv.zip";

        string rawFolder =
            @"A:\01_Vyom\03_Projects\01_Stock\Stock_Data\NSE\Raw";

        string fileName =
            $"NSE_{date}.csv.zip";

        string zipFile =
            Path.Combine(rawFolder, fileName);

        // 1. Download NSE file
        string downloadedFile =
            await _downloader.DownloadAsync(
                url,
                zipFile);

        // 2. Extract ZIP
        string extractedFolder =
            @"A:\01_Vyom\03_Projects\01_Stock\Stock_Data\NSE\Extracted";

        var extractor = new ExtractZip();

        extractor.Extract(
            downloadedFile,
            extractedFolder);

        // 3. Find extracted CSV
        string[] csvFiles =
            Directory.GetFiles(
                extractedFolder,
                "*.csv");

        if (csvFiles.Length == 0)
        {
            return NotFound(
                "No CSV file was found after extraction.");
        }

        string csvFile = csvFiles[0];

        // 4. Read CSV
        var parser = new NseParser();

        var records =
            parser.ReadData(csvFile);

        // 5. Return result
        return Ok(new
        {
            Message = "NSE data downloaded and processed successfully.",
            CsvFile = csvFile,
            TotalRecords = records.Count,

            FirstStock =
                records.Count > 0
                    ? records[0].Symbol
                    : "",

            LastStock =
                records.Count > 0
                    ? records[^1].Symbol
                    : ""
        });
    }
}