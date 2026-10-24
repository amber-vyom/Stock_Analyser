
using System.Net;
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
        NseDataPaths.RawFolder;

    private readonly string extractedFolder =
        NseDataPaths.ExtractedFolder;

    public MainForm()
    {
        downloader = new NseDownloader(new HttpClient());
        CreateWindow();
    }

    private void CreateWindow()
    {
        Text = "Stock Analysis System";
        Width = 1250;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;

        Label titleLabel = new Label
        {
            Text = "STOCK ANALYSIS SYSTEM",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            Left = 30,
            Top = 25,
            Width = 500,
            Height = 40
        };

        Controls.Add(titleLabel);

        Label nseLabel = new Label
        {
            Text = "NSE DATA",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Left = 30,
            Top = 90,
            Width = 200
        };

        Controls.Add(nseLabel);

        downloadButton = new Button
        {
            Text = "Download Latest 7 Days",
            Left = 30,
            Top = 130,
            Width = 220,
            Height = 45
        };
        downloadButton.Click += DownloadButton_Click;
        Controls.Add(downloadButton);

        parserTestButton = new Button
        {
            Text = "Test NSE Parser",
            Left = 260,
            Top = 130,
            Width = 150,
            Height = 45
        };
        parserTestButton.Click += ParserTestButton_Click;
        Controls.Add(parserTestButton);

        sevenDayTestButton = new Button
        {
            Text = "Test 7 Days",
            Left = 420,
            Top = 130,
            Width = 130,
            Height = 45
        };
        sevenDayTestButton.Click += SevenDayTestButton_Click;
        Controls.Add(sevenDayTestButton);

        databaseTestButton = new Button
        {
            Text = "Test Database",
            Left = 560,
            Top = 130,
            Width = 140,
            Height = 45
        };
        databaseTestButton.Click += DatabaseTestButton_Click;
        Controls.Add(databaseTestButton);

        moveRawDataButton = new Button
        {
            Text = "Move Raw Data to DB",
            Left = 710,
            Top = 130,
            Width = 170,
            Height = 45
        };
        moveRawDataButton.Click += MoveRawDataButton_Click;
        Controls.Add(moveRawDataButton);

        saveProcessedDataButton = new Button
        {
            Text = "Save Processed Data",
            Left = 890,
            Top = 130,
            Width = 170,
            Height = 45
        };
        saveProcessedDataButton.Click += SaveProcessedDataButton_Click;
        Controls.Add(saveProcessedDataButton);

        generateTopStocksButton = new Button
        {
            Text = "Generate Top 10",
            Left = 1070,
            Top = 130,
            Width = 140,
            Height = 45
        };
        generateTopStocksButton.Click += GenerateTopStocksButton_Click;
        Controls.Add(generateTopStocksButton);

        statusLabel = new Label
        {
            Text = "Status: Ready",
            Left = 30,
            Top = 190,
            Width = 1150,
            Height = 30
        };
        Controls.Add(statusLabel);

        fileList = new ListView
        {
            Left = 30,
            Top = 230,
            Width = 1180,
            Height = 380,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true
        };

        fileList.Columns.Add("Date", 150);
        fileList.Columns.Add("File", 600);
        fileList.Columns.Add("Status", 300);

        Controls.Add(fileList);
    }

    private async void DownloadButton_Click(object? sender, EventArgs e)
    {
        SetButtonsEnabled(false);

        try
        {
            fileList.Items.Clear();
            statusLabel.Text = "Status: Finding available NSE reports...";

            Directory.CreateDirectory(rawFolder);
            Directory.CreateDirectory(extractedFolder);

            int downloadedCount = 0;
            int checkedDays = 0;
            DateTime currentDate = DateTime.Today;

            var extractor = new ExtractZip();

            while (downloadedCount < 7 && checkedDays < 30)
            {
                if (currentDate.DayOfWeek != DayOfWeek.Saturday &&
                    currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    checkedDays++;

                    string date = currentDate.ToString("yyyyMMdd");

                    string url =
                        $"https://nsearchives.nseindia.com/content/cm/" +
                        $"BhavCopy_NSE_CM_0_0_0_{date}_F_0000.csv.zip";

                    string zipFile = Path.Combine(
                        rawFolder,
                        $"BhavCopy_NSE_CM_0_0_0_{date}_F_0000.csv.zip");

                    statusLabel.Text =
                        $"Status: Checking {currentDate:dd-MM-yyyy}...";

                    try
                    {
                        await downloader.DownloadAsync(url, zipFile);

                        extractor.Extract(zipFile, extractedFolder);

                        ListViewItem item = new ListViewItem(
                            currentDate.ToString("dd-MM-yyyy"));

                        item.SubItems.Add(Path.GetFileName(zipFile));
                        item.SubItems.Add("Downloaded");

                        fileList.Items.Add(item);
                        downloadedCount++;
                    }
                    catch (HttpRequestException ex)
                        when (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        if (File.Exists(zipFile))
                        {
                            File.Delete(zipFile);
                        }

                        ListViewItem item = new ListViewItem(
                            currentDate.ToString("dd-MM-yyyy"));

                        item.SubItems.Add(Path.GetFileName(zipFile));
                        item.SubItems.Add("Report unavailable");

                        fileList.Items.Add(item);
                    }
                }

                currentDate = currentDate.AddDays(-1);
            }

            if (downloadedCount == 0)
            {
                statusLabel.Text = "Status: No NSE reports downloaded.";

                MessageBox.Show(
                    "No NSE reports were found in the last 30 calendar days.",
                    "Download Result",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            statusLabel.Text =
                $"Status: Downloaded {downloadedCount} NSE reports.";

            MessageBox.Show(
                $"Successfully downloaded {downloadedCount} NSE reports.\n" +
                $"Trading dates checked: {checkedDays}\n\n" +
                "Check the file list for unavailable reports.",
                "Download Result",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Status: Download failed.";

            MessageBox.Show(
                ex.Message,
                "Download Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    private void ParserTestButton_Click(object? sender, EventArgs e)
    {
        try
        {
            statusLabel.Text = "Status: Testing NSE parser...";

            string[] csvFiles = Directory.GetFiles(
                extractedFolder,
                "*.csv");

            if (csvFiles.Length == 0)
            {
                MessageBox.Show(
                    "No NSE CSV files were found.\n\nDownload the 7 trading days first.",
                    "Parser Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                statusLabel.Text = "Status: No CSV files found.";
                return;
            }

            string csvFile = csvFiles[0];
            var parser = new NseParser();

            List<NseMarketRecord> records = parser.ReadData(csvFile);

            int equityRecords = records.Count(x => x.Series == "EQ");
            int nonEquityRecords = records.Count(x => x.Series != "EQ");

            string message =
                "NSE PARSER TEST\n\n" +
                $"File:\n{Path.GetFileName(csvFile)}\n\n" +
                $"Total parsed records: {records.Count}\n\n" +
                $"EQ records: {equityRecords}\n\n" +
                $"Non-EQ records: {nonEquityRecords}\n\n" +
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

            statusLabel.Text = "Status: Parser test completed.";
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Status: Parser test failed.";

            MessageBox.Show(
                ex.Message,
                "Parser Test Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void SevenDayTestButton_Click(object? sender, EventArgs e)
    {
        try
        {
            statusLabel.Text = "Status: Processing 7 days...";

            var sevenDayTest =
                new Stock_Analysis.Engine.Tests.SevenDayTest();

            string result = sevenDayTest.Run();

            MessageBox.Show(
                result,
                "7 Day Test",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            statusLabel.Text = "Status: 7 day test completed.";
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Status: 7 day processing failed.";

            MessageBox.Show(
                ex.Message,
                "7 Day Test Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void DatabaseTestButton_Click(object? sender, EventArgs e)
    {
        try
        {
            databaseTestButton.Enabled = false;
            statusLabel.Text = "Status: Connecting to database...";

            var databaseTest = new DatabaseTest();
            bool connected = await databaseTest.TestConnectionAsync();

            if (connected)
            {
                statusLabel.Text = "Status: Database connected.";

                MessageBox.Show(
                    "DATABASE CONNECTION SUCCESSFUL!\n\n" +
                    "Connected to:\nStockAnalysisDB\n\n" +
                    "SQL Server connection is working.",
                    "Database Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                statusLabel.Text = "Status: Database connection failed.";

                MessageBox.Show(
                    "Database connection failed.",
                    "Database Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Status: Database connection failed.";

            MessageBox.Show(
                "Could not connect to SQL Server.\n\n" + ex.Message,
                "Database Connection Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            databaseTestButton.Enabled = true;
        }
    }

    private async void MoveRawDataButton_Click(object? sender, EventArgs e)
    {
        try
        {
            moveRawDataButton.Enabled = false;
            statusLabel.Text = "Status: Reading NSE CSV files...";

            string[] csvFiles = Directory.GetFiles(
                extractedFolder,
                "*.csv");

            if (csvFiles.Length == 0)
            {
                MessageBox.Show(
                    "No NSE CSV files were found.\n\nDownload the latest 7 trading days first.",
                    "Raw Data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                statusLabel.Text = "Status: No CSV files found.";
                return;
            }

            var parser = new NseParser();
            var allEqRecords = new List<NseMarketRecord>();

            foreach (string csvFile in csvFiles)
            {
                statusLabel.Text =
                    $"Status: Reading {Path.GetFileName(csvFile)}...";

                List<NseMarketRecord> records = parser.ReadData(csvFile);

                allEqRecords.AddRange(
                    records.Where(x => x.Series == "EQ"));
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
                $"Status: Saving {allEqRecords.Count:N0} EQ records...";

            var repository = new RawDataRepository();

            int inserted = await repository.InsertRecordsAsync(allEqRecords);
            int skipped = allEqRecords.Count - inserted;

            statusLabel.Text =
                $"Status: Database updated. {inserted:N0} inserted, {skipped:N0} already existed.";

            MessageBox.Show(
                "RAW DATA DATABASE UPDATE\n\n" +
                $"CSV files processed: {csvFiles.Length}\n\n" +
                $"EQ records found: {allEqRecords.Count:N0}\n\n" +
                $"New records inserted: {inserted:N0}\n\n" +
                $"Existing records skipped: {skipped:N0}\n\n" +
                "RawData table updated successfully.",
                "Move Raw Data to Database",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Status: Database update failed.";

            MessageBox.Show(
                "Could not move raw data to database.\n\n" + ex.Message,
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            moveRawDataButton.Enabled = true;
        }
    }

    private async void SaveProcessedDataButton_Click(object? sender, EventArgs e)
    {
        try
        {
            saveProcessedDataButton.Enabled = false;
            statusLabel.Text = "Status: Processing 7 days...";

            var sevenDayTest =
                new Stock_Analysis.Engine.Tests.SevenDayTest();

            int inserted = await sevenDayTest.SaveToDatabaseAsync();

            statusLabel.Text =
                $"Status: ProcessedData updated. {inserted:N0} stocks inserted or refreshed.";

            MessageBox.Show(
                "PROCESSED DATA UPDATE\n\n" +
                $"Stocks inserted or refreshed: {inserted:N0}\n\n" +
                "ProcessedData table updated successfully.",
                "Processed Data",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Status: ProcessedData update failed.";

            MessageBox.Show(
                "Could not save processed data.\n\n" + ex.Message,
                "Processed Data Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            saveProcessedDataButton.Enabled = true;
        }
    }

    private async void GenerateTopStocksButton_Click(object? sender, EventArgs e)
    {
        try
        {
            generateTopStocksButton.Enabled = false;
            statusLabel.Text = "Status: Analysing stocks with 10 filters...";

            var predictionService = new StockPredictionService();

            List<StockFilterResult> topStocks =
                await predictionService.GetTopStocksAsync(10);

            if (topStocks.Count == 0)
            {
                MessageBox.Show(
                    "No processed stock data found.\n\nClick 'Save Processed Data' first.",
                    "No Data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                statusLabel.Text = "Status: No processed data found.";
                return;
            }

            fileList.Items.Clear();
            fileList.Columns.Clear();

            fileList.Columns.Add("Rank", 100);
            fileList.Columns.Add("Symbol", 300);
            fileList.Columns.Add("Score", 200);
            fileList.Columns.Add("Filters Passed", 250);

            int rank = 1;

            foreach (StockFilterResult stock in topStocks)
            {
                ListViewItem item = new ListViewItem(rank.ToString());

                item.SubItems.Add(stock.Symbol);
                item.SubItems.Add($"{stock.Score}/100");
                item.SubItems.Add($"{stock.PassedFilters}/10");

                fileList.Items.Add(item);
                rank++;
            }

            statusLabel.Text =
                $"Status: Top {topStocks.Count} stocks generated successfully.";
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Status: Stock analysis failed.";

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

    private void SetButtonsEnabled(bool enabled)
    {
        downloadButton.Enabled = enabled;
        parserTestButton.Enabled = enabled;
        sevenDayTestButton.Enabled = enabled;
        databaseTestButton.Enabled = enabled;
        moveRawDataButton.Enabled = enabled;
        saveProcessedDataButton.Enabled = enabled;
        generateTopStocksButton.Enabled = enabled;
    }
}
