using System.IO.Compression;
using System.Net;
using Stock_Analysis.Engine.Database;
using Stock_Analysis.Engine.DSA;
using Stock_Analysis.Engine.Models;
using Stock_Analysis.Engine.NSE;

namespace Stock_Analysis.Engine.Services;

public record DayDownloadResult(DateTime Date, string Status);

public record PipelineSummary(
    List<DayDownloadResult> Downloads,
    int CsvFilesUsed,
    int EqRecords,
    int RawInserted,
    int RawSkipped,
    int ProcessedSaved);

public class NsePipelineService
{
    private const int WindowSize = 7;
    private const int MaxCalendarDaysBack = 30;

    private readonly NseDownloader downloader;

    public NsePipelineService(NseDownloader downloader)
    {
        this.downloader = downloader;
    }

    public async Task<List<DayDownloadResult>> DownloadLatestAsync(
        DateTime? today = null)
    {
        Directory.CreateDirectory(NseDataPaths.RawFolder);
        Directory.CreateDirectory(NseDataPaths.ExtractedFolder);

        var results = new List<DayDownloadResult>();
        var extractor = new ExtractZip();
        DateTime date = (today ?? DateTime.Today).Date;
        int downloaded = 0;

        for (int back = 0; back < MaxCalendarDaysBack && downloaded < WindowSize; back++)
        {
            DateTime current = date.AddDays(-back);

            if (current.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            string stamp = current.ToString("yyyyMMdd");
            string url =
                "https://nsearchives.nseindia.com/content/cm/" +
                $"BhavCopy_NSE_CM_0_0_0_{stamp}_F_0000.csv.zip";
            string zipFile = Path.Combine(
                NseDataPaths.RawFolder,
                $"BhavCopy_NSE_CM_0_0_0_{stamp}_F_0000.csv.zip");

            try
            {
                await downloader.DownloadAsync(url, zipFile);
                extractor.Extract(zipFile, NseDataPaths.ExtractedFolder);
                results.Add(new DayDownloadResult(current, "Downloaded"));
                downloaded++;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                DeleteQuietly(zipFile);
                results.Add(new DayDownloadResult(current, "Report unavailable (holiday or not published)"));
            }
            catch (InvalidDataException)
            {
                DeleteQuietly(zipFile);
                results.Add(new DayDownloadResult(current, "Invalid ZIP file"));
            }
        }

        return results;
    }

    public async Task<(int Files, int EqRecords, int Inserted)> LoadRawAsync()
    {
        List<string> files = LatestCsvFiles();
        List<NseMarketRecord> eq = ReadEqRecords(files);

        if (eq.Count == 0)
        {
            return (files.Count, 0, 0);
        }

        int inserted = await new RawDataRepository().InsertRecordsAsync(eq);

        return (files.Count, eq.Count, inserted);
    }

    public async Task<int> SaveProcessedAsync()
    {
        List<string> files = LatestCsvFiles();
        List<NseMarketRecord> eq = ReadEqRecords(files);

        if (eq.Count == 0)
        {
            return 0;
        }

        List<SevenDayStockData> stocks = new SevenDayAggregator().Aggregate(eq);

        return await new ProcessedDataRepository().InsertRecordsAsync(stocks);
    }

    public async Task<PipelineSummary> RunAllAsync()
    {
        List<DayDownloadResult> downloads = await DownloadLatestAsync();
        var raw = await LoadRawAsync();
        int processed = await SaveProcessedAsync();

        return new PipelineSummary(
            downloads,
            raw.Files,
            raw.EqRecords,
            raw.Inserted,
            raw.EqRecords - raw.Inserted,
            processed);
    }

    private static List<string> LatestCsvFiles()
    {
        if (!Directory.Exists(NseDataPaths.ExtractedFolder))
        {
            return new List<string>();
        }

        return NseReportFiles.GetLatestCsvFiles(
            Directory.GetFiles(NseDataPaths.ExtractedFolder, "*.csv"),
            WindowSize);
    }

    private static List<NseMarketRecord> ReadEqRecords(List<string> files)
    {
        var parser = new NseParser();
        var records = new List<NseMarketRecord>();

        foreach (string file in files)
        {
            records.AddRange(parser.ReadData(file).Where(r => r.Series == "EQ"));
        }

        return records;
    }

    private static void DeleteQuietly(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
    }
}
