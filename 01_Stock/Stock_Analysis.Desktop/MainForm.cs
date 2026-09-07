using System.Windows.Forms;
using Stock_Analysis.Engine.Analysis;
using Stock_Analysis.Engine.Database;
using Stock_Analysis.Engine.Models;
using Stock_Analysis.Engine.NSE;

namespace Stock_Analysis.Desktop;

public class MainForm : Form
{
    private Button downloadButton = null!;
    private Button parserTestButton = null!;
    private Button sevenDayTestButton = null!;
    private Button databaseTestButton = null!;
    private Button moveRawDataButton = null!;
    private Button saveProcessedDataButton = null!;
    private Button generateTopStocksButton = null!;

    private Label statusLabel = null!;
    private ListView fileList = null!;

    private readonly NseDownloader downloader;

    private readonly string rawFolder =
        @"A:\01_Vyom\03_Projects\01_Stock\Stock_Data\NSE\Raw";

    private readonly string extractedFolder =
        @"A:\01_Vyom\03_Projects\01_Stock\Stock_Data\NSE\Extracted";

    public MainForm()
    {
        downloader =
            new NseDownloader(
                new HttpClient());

        CreateWindow();
    }

    private void CreateWindow()
    {
        Text = "Stock Analysis System";

        Width = 1250;
        Height = 700;

        StartPosition =
            FormStartPosition.CenterScreen;

        // ==========================================
        // TITLE
        // ==========================================

        Label titleLabel = new Label();

        titleLabel.Text =
            "STOCK ANALYSIS SYSTEM";

        titleLabel.Font =
            new Font(
                "Segoe UI",
                20,
                FontStyle.Bold);

        titleLabel.Left = 30;
        titleLabel.Top = 25;
        titleLabel.Width = 500;
        titleLabel.Height = 40;

        Controls.Add(titleLabel);

        // ==========================================
        // NSE LABEL
        // ==========================================

        Label nseLabel = new Label();

        nseLabel.Text =
            "NSE DATA";

        nseLabel.Font =
            new Font(
                "Segoe UI",
                14,
                FontStyle.Bold);

        nseLabel.Left = 30;
        nseLabel.Top = 90;
        nseLabel.Width = 200;

        Controls.Add(nseLabel);

        // ==========================================
        // DOWNLOAD BUTTON
        // ==========================================

        downloadButton = new Button();

        downloadButton.Text =
            "Download Latest 7 Days";

        downloadButton.Left = 30;
        downloadButton.Top = 130;
        downloadButton.Width = 220;
        downloadButton.Height = 45;

        downloadButton.Click +=
            DownloadButton_Click;

        Controls.Add(downloadButton);

        // ==========================================
        // PARSER TEST BUTTON
        // ==========================================

        parserTestButton = new Button();

        parserTestButton.Text =
            "Test NSE Parser";

        parserTestButton.Left = 260;
        parserTestButton.Top = 130;
        parserTestButton.Width = 150;
        parserTestButton.Height = 45;

        parserTestButton.Click +=
            ParserTestButton_Click;

        Controls.Add(parserTestButton);

        // ==========================================
        // 7 DAY TEST BUTTON
        // ==========================================

        sevenDayTestButton = new Button();

        sevenDayTestButton.Text =
            "Test 7 Days";

        sevenDayTestButton.Left = 420;
        sevenDayTestButton.Top = 130;
        sevenDayTestButton.Width = 130;
        sevenDayTestButton.Height = 45;

        sevenDayTestButton.Click +=
            SevenDayTestButton_Click;

        Controls.Add(sevenDayTestButton);

        // ==========================================
        // DATABASE TEST BUTTON
        // ==========================================

        databaseTestButton = new Button();

        databaseTestButton.Text =
            "Test Database";

        databaseTestButton.Left = 560;
        databaseTestButton.Top = 130;
        databaseTestButton.Width = 140;
        databaseTestButton.Height = 45;

        databaseTestButton.Click +=
            DatabaseTestButton_Click;

        Controls.Add(databaseTestButton);

        // ==========================================
        // MOVE RAW DATA BUTTON
        // ==========================================

        moveRawDataButton = new Button();

        moveRawDataButton.Text =
            "Move Raw Data to DB";

        moveRawDataButton.Left = 710;
        moveRawDataButton.Top = 130;
        moveRawDataButton.Width = 170;
        moveRawDataButton.Height = 45;

        moveRawDataButton.Click +=
            MoveRawDataButton_Click;

        Controls.Add(moveRawDataButton);

        // ==========================================
        // SAVE PROCESSED DATA BUTTON
        // ==========================================

        saveProcessedDataButton = new Button();

        saveProcessedDataButton.Text =
            "Save Processed Data";

        saveProcessedDataButton.Left = 890;
        saveProcessedDataButton.Top = 130;
        saveProcessedDataButton.Width = 170;
        saveProcessedDataButton.Height = 45;

        saveProcessedDataButton.Click +=
            SaveProcessedDataButton_Click;

        Controls.Add(saveProcessedDataButton);

        // ==========================================
        // GENERATE TOP 10 BUTTON
        // ==========================================

        generateTopStocksButton = new Button();

        generateTopStocksButton.Text =
            "Generate Top 10";

        generateTopStocksButton.Left = 1070;
        generateTopStocksButton.Top = 130;
        generateTopStocksButton.Width = 140;
        generateTopStocksButton.Height = 45;

        generateTopStocksButton.Click +=
            GenerateTopStocksButton_Click;

        Controls.Add(generateTopStocksButton);

        // ==========================================
        // STATUS
        // ==========================================

        statusLabel = new Label();

        statusLabel.Text =
            "Status: Ready";

        statusLabel.Left = 30;
        statusLabel.Top = 190;
        statusLabel.Width = 1150;
        statusLabel.Height = 30;

        Controls.Add(statusLabel);

        // ==========================================
        // FILE / RESULT LIST
        // ==========================================

        fileList = new ListView();

        fileList.Left = 30;
        fileList.Top = 230;
        fileList.Width = 1180;
        fileList.Height = 380;

        fileList.View =
            View.Details;

        fileList.FullRowSelect =
            true;

        fileList.GridLines =
            true;

        fileList.Columns.Add(
            "Date",
            150);

        fileList.Columns.Add(
            "File",
            600);

        fileList.Columns.Add(
            "Status",
            300);

        Controls.Add(fileList);
    }

    // ==================================================
    // DOWNLOAD 7 DAYS
    // ==================================================

    private async void DownloadButton_Click(
        object? sender,
        EventArgs e)
    {
        try
        {
            downloadButton.Enabled = false;
            parserTestButton.Enabled = false;
            sevenDayTestButton.Enabled = false;
            databaseTestButton.Enabled = false;
            moveRawDataButton.Enabled = false;
            saveProcessedDataButton.Enabled = false;
            generateTopStocksButton.Enabled = false;

            fileList.Items.Clear();

            statusLabel.Text =
                "Status: Finding trading days...";

            var tradingDayFinder =
                new FindTradingDays();

            var tradingDays =
                tradingDayFinder.GetDays(
                    DateTime.Today,
                    7);

            Directory.CreateDirectory(
                rawFolder);

            Directory.CreateDirectory(
                extractedFolder);

            foreach (string oldCsv in
                     Directory.GetFiles(
                         extractedFolder,
                         "*.csv"))
            {
                File.Delete(oldCsv);
            }

            foreach (var day in tradingDays)
            {
                string date =
                    day.ToString("yyyyMMdd");

                statusLabel.Text =
                    $"Status: Downloading {day:dd-MM-yyyy}...";

                string url =
                    $"https://nsearchives.nseindia.com/content/cm/" +
                    $"BhavCopy_NSE_CM_0_0_0_{date}_F_0000.csv.zip";

                string zipFile =
                    Path.Combine(
                        rawFolder,
                        $"NSE_{date}.csv.zip");

                await downloader.DownloadAsync(
                    url,
                    zipFile);

                var extractor =
                    new ExtractZip();

                extractor.Extract(
                    zipFile,
                    extractedFolder);

                ListViewItem item =
                    new ListViewItem(
                        day.ToString("dd-MM-yyyy"));

                item.SubItems.Add(
                    Path.GetFileName(zipFile));

                item.SubItems.Add(
                    "Downloaded");

                fileList.Items.Add(item);
            }

            statusLabel.Text =
                "Status: 7 days downloaded successfully.";
        }
        catch (Exception ex)
        {
            statusLabel.Text =
                "Status: Download failed.";

            MessageBox.Show(
                ex.Message,
                "Download Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            downloadButton.Enabled = true;
            parserTestButton.Enabled = true;
            sevenDayTestButton.Enabled = true;
            databaseTestButton.Enabled = true;
            moveRawDataButton.Enabled = true;
            saveProcessedDataButton.Enabled = true;
            generateTopStocksButton.Enabled = true;
        }
    }

    // ==================================================
    // TEST NSE PARSER
    // ==================================================

    private void ParserTestButton_Click(
        object? sender,
        EventArgs e)
    {
        try
        {
            statusLabel.Text =
                "Status: Testing NSE parser...";

            string[] csvFiles =
                Directory.GetFiles(
                    extractedFolder,
                    "*.csv");

            if (csvFiles.Length == 0)
            {
                MessageBox.Show(
                    "No NSE CSV files were found.\n\n" +
                    "Download the 7 trading days first.",
                    "Parser Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                statusLabel.Text =
                    "Status: No CSV files found.";

                return;
            }

            string csvFile =
                csvFiles[0];

            var parser =
                new NseParser();

            List<NseMarketRecord> records =
                parser.ReadData(csvFile);

            int equityRecords =
                records.Count(
                    x => x.Series == "EQ");

            int nonEquityRecords =
                records.Count(
                    x => x.Series != "EQ");

            string message =
                "NSE PARSER TEST\n\n" +

                $"File:\n" +
                $"{Path.GetFileName(csvFile)}\n\n" +

                $"Total parsed records: " +
                $"{records.Count}\n\n" +

                $"EQ records: " +
                $"{equityRecords}\n\n" +

                $"Non-EQ records: " +
                $"{nonEquityRecords}\n\n" +

                "First 10 EQ stocks:\n" +
                "------------------------------\n";

            foreach (var record in records
                         .Where(x => x.Series == "EQ")
                         .Take(10))
            {
                message +=
                    $"{record.Symbol} | " +
                    $"Open: {record.OpenPrice:F2} | " +
                    $"Close: {record.ClosePrice:F2} | " +
                    $"Volume: {record.TradingVolume}\n";
            }

            MessageBox.Show(
                message,
                "Parser Test",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            statusLabel.Text =
                "Status: Parser test completed.";
        }
        catch (Exception ex)
        {
            statusLabel.Text =
                "Status: Parser test failed.";

            MessageBox.Show(
                ex.Message,
                "Parser Test Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    // ==================================================
    // TEST 7 DAYS
    // ==================================================

    private void SevenDayTestButton_Click(
        object? sender,
        EventArgs e)
    {
        try
        {
            statusLabel.Text =
                "Status: Processing 7 days...";

            var sevenDayTest =
                new Stock_Analysis.Engine.Tests.SevenDayTest();

            string result =
                sevenDayTest.Run();

            MessageBox.Show(
                result,
                "7 Day Test",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            statusLabel.Text =
                "Status: 7 day test completed.";
        }
        catch (Exception ex)
        {
            statusLabel.Text =
                "Status: 7 day processing failed.";

            MessageBox.Show(
                ex.Message,
                "7 Day Test Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    // ==================================================
    // TEST DATABASE CONNECTION
    // ==================================================

    private async void DatabaseTestButton_Click(
        object? sender,
        EventArgs e)
    {
        try
        {
            databaseTestButton.Enabled = false;

            statusLabel.Text =
                "Status: Connecting to database...";

            var databaseTest =
                new DatabaseTest();

            bool connected =
                await databaseTest.TestConnectionAsync();

            if (connected)
            {
                statusLabel.Text =
                    "Status: Database connected.";

                MessageBox.Show(
                    "DATABASE CONNECTION SUCCESSFUL!\n\n" +
                    "Connected to:\n" +
                    "StockAnalysisDB\n\n" +
                    "SQL Server connection is working.",
                    "Database Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                statusLabel.Text =
                    "Status: Database connection failed.";

                MessageBox.Show(
                    "Database connection failed.",
                    "Database Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            statusLabel.Text =
                "Status: Database connection failed.";

            MessageBox.Show(
                "Could not connect to SQL Server.\n\n" +
                ex.Message,
                "Database Connection Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            databaseTestButton.Enabled = true;
        }
    }

    // ==================================================
    // MOVE RAW DATA TO DATABASE
    // ==================================================

    private async void MoveRawDataButton_Click(
        object? sender,
        EventArgs e)
    {
        try
        {
            moveRawDataButton.Enabled = false;

            statusLabel.Text =
                "Status: Reading NSE CSV files...";

            string[] csvFiles =
                Directory.GetFiles(
                    extractedFolder,
                    "*.csv");

            if (csvFiles.Length == 0)
            {
                MessageBox.Show(
                    "No NSE CSV files were found.\n\n" +
                    "Download the latest 7 trading days first.",
                    "Raw Data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                statusLabel.Text =
                    "Status: No CSV files found.";

                return;
            }

            var parser =
                new NseParser();

            var allEqRecords =
                new List<NseMarketRecord>();

            foreach (string csvFile in csvFiles)
            {
                statusLabel.Text =
                    $"Status: Reading {Path.GetFileName(csvFile)}...";

                List<NseMarketRecord> records =
                    parser.ReadData(csvFile);

                List<NseMarketRecord> eqRecords =
                    records
                        .Where(x => x.Series == "EQ")
                        .ToList();

                allEqRecords.AddRange(
                    eqRecords);
            }

            if (allEqRecords.Count == 0)
            {
                MessageBox.Show(
                    "No EQ records were found.",
                    "Raw Data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            statusLabel.Text =
                $"Status: Saving " +
                $"{allEqRecords.Count:N0} EQ records...";

            var repository =
                new RawDataRepository();

            int inserted =
                await repository.InsertRecordsAsync(
                    allEqRecords);

            int skipped =
                allEqRecords.Count - inserted;

            statusLabel.Text =
                $"Status: Database updated. " +
                $"{inserted:N0} inserted, " +
                $"{skipped:N0} already existed.";

            MessageBox.Show(
                "RAW DATA DATABASE UPDATE\n\n" +

                $"CSV files processed: " +
                $"{csvFiles.Length}\n\n" +

                $"EQ records found: " +
                $"{allEqRecords.Count:N0}\n\n" +

                $"New records inserted: " +
                $"{inserted:N0}\n\n" +

                $"Existing records skipped: " +
                $"{skipped:N0}\n\n" +

                "RawData table updated successfully.",

                "Move Raw Data to Database",

                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            statusLabel.Text =
                "Status: Database update failed.";

            MessageBox.Show(
                "Could not move raw data to database.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            moveRawDataButton.Enabled = true;
        }
    }

    // ==================================================
    // SAVE PROCESSED DATA
    // ==================================================

    private async void SaveProcessedDataButton_Click(
        object? sender,
        EventArgs e)
    {
        try
        {
            saveProcessedDataButton.Enabled = false;

            statusLabel.Text =
                "Status: Processing 7 days...";

            var sevenDayTest =
                new Stock_Analysis.Engine.Tests.SevenDayTest();

            int inserted =
                await sevenDayTest.SaveToDatabaseAsync();

            statusLabel.Text =
                $"Status: ProcessedData updated. " +
                $"{inserted:N0} stocks inserted.";

            MessageBox.Show(
                "PROCESSED DATA UPDATE\n\n" +

                $"Stocks inserted: " +
                $"{inserted:N0}\n\n" +

                "ProcessedData table updated successfully.",

                "Processed Data",

                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            statusLabel.Text =
                "Status: ProcessedData update failed.";

            MessageBox.Show(
                "Could not save processed data.\n\n" +
                ex.Message,

                "Processed Data Error",

                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            saveProcessedDataButton.Enabled = true;
        }
    }

    // ==================================================
    // GENERATE TOP 10 STOCKS
    // ==================================================

    private async void GenerateTopStocksButton_Click(
        object? sender,
        EventArgs e)
    {
        try
        {
            generateTopStocksButton.Enabled = false;

            statusLabel.Text =
                "Status: Analysing stocks with 10 filters...";

            var predictionService =
                new StockPredictionService();

            List<StockFilterResult> topStocks =
                await predictionService.GetTopStocksAsync(10);

            if (topStocks.Count == 0)
            {
                MessageBox.Show(
                    "No processed stock data found.\n\n" +
                    "Click 'Save Processed Data' first.",
                    "No Data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                statusLabel.Text =
                    "Status: No processed data found.";

                return;
            }

            fileList.Items.Clear();

            fileList.Columns.Clear();

            fileList.Columns.Add(
                "Rank",
                100);

            fileList.Columns.Add(
                "Symbol",
                300);

            fileList.Columns.Add(
                "Score",
                200);

            fileList.Columns.Add(
                "Filters Passed",
                250);

            int rank = 1;

            foreach (
                StockFilterResult stock
                in topStocks)
            {
                ListViewItem item =
                    new ListViewItem(
                        rank.ToString());

                item.SubItems.Add(
                    stock.Symbol);

                item.SubItems.Add(
                    $"{stock.Score}/100");

                item.SubItems.Add(
                    $"{stock.PassedFilters}/10");

                fileList.Items.Add(item);

                rank++;
            }

            statusLabel.Text =
                $"Status: Top {topStocks.Count} stocks generated successfully.";
        }
        catch (Exception ex)
        {
            statusLabel.Text =
                "Status: Stock analysis failed.";

            MessageBox.Show(
                ex.Message,
                "Stock Analysis Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            generateTopStocksButton.Enabled = true;
        }
    }
}