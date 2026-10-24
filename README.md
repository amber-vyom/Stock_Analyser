# Stock Analysis System

A rule-based NSE stock screening and ranking system built with C# and .NET. It downloads NSE BhavCopy reports, stores them in SQL Server, aggregates a rolling seven-trading-day window, applies ten screening filters, computes a weighted score out of 100, and ranks stocks with a Priority Queue.

This is a screening tool, not a prediction system. It does not use real-time data, and the score has not been backtested, so it should not be read as a forecast of future returns.

## Features

- **Data collection:** Downloads NSE UDiFF CM BhavCopy ZIP reports. Unavailable days (holidays, 404) are skipped.
- **Parsing:** Reads the CSV reports; only the EQ series is used for analysis.
- **Seven-day aggregation:** Groups records by symbol (dictionary) in date order and computes return, volume, traded value, price range and volume consistency. The window uses the latest seven downloaded reports.
- **SQL Server storage:** Raw data and processed data tables. Saving processed data updates existing symbols in a single transaction.
- **Ten filters:** Completeness, positive return, strong return, close above open, price stability, volume activity, volume consistency, momentum, traded value and overall quality.
- **Weighted score:** A configurable score out of 100 (`ScoringOptions`) that is separate from the pass/fail filters.
- **Ranking:** A Priority Queue orders by score, then passed-filter count, then symbol. Stocks with fewer than seven trading days are excluded.
- **Interfaces:** A React dashboard backed by an ASP.NET Core API. The "Update data from NSE" button runs the whole pipeline (download, raw data, processed data). The older Windows Forms app is still in the repo but is no longer the main interface.

## Architecture

```text
NSE reports -> download + extract -> CSV parser -> RawData (SQL Server)
           -> seven-day aggregation -> ProcessedData (SQL Server)
           -> ten filters -> weighted score -> Priority Queue -> Top N

React dashboard -> ASP.NET Core API -> Stock_Analysis.Engine -> SQL Server
```

## Project structure

```text
01_Stock
|-- Stock_Analysis.Engine     business logic (Analysis, Database, DSA, ETL, Models, NSE, Services)
|-- Stock_Analysis.Desktop    Windows Forms app
|-- Stock_Analysis Web        ASP.NET Core API
|-- Stock_Analysis.Frontend   React + TypeScript dashboard (Vite)
|-- Stock_Analysis.Tests      xUnit tests
`-- Stock_Data                downloaded NSE reports
```

## Data structures and algorithms used

| Concept | Where it is used | Complexity |
|---|---|---|
| List traversal | Reading and parsing daily records (`NseParser`, `SevenDayAggregator`) | O(n) |
| Dictionary (hash table) | Grouping records by symbol in `SevenDayAggregator` | O(1) average per insert, O(n) total |
| Sliding window | Only the latest 7 trading-day reports are analysed (`NseReportFiles.GetLatestCsvFiles`) | O(f log f) for f files |
| Priority Queue (binary heap) | Top-N ranking in `StockRankingService.RankStocks` | O(n log n) overall, O(log n) per insert/remove |
| Merge Sort | Full ranking (`RankAllWithMergeSort`) and sorting by symbol (`DSA/MergeSorter.cs`), stable | O(n log n) time, O(n) extra space |
| Binary Search | Symbol lookup in the API (`DSA/SymbolSearch.cs`) | O(log n) on a sorted list |

The heap-based Top-N and the Merge Sort full ranking use the same comparison (score, then passed filters, then symbol), and a unit test checks that they produce identical orderings.

## API

| Endpoint | Description |
|---|---|
| `POST /api/pipeline/run` | Downloads the latest reports, loads RawData and refreshes ProcessedData (one run at a time) |
| `GET /api/health` | Database connectivity and stock count |
| `GET /api/stocks?search=` | Processed seven-day metrics |
| `GET /api/stocks/top?count=10` | Ranked stocks with score, filters and metrics |
| `GET /api/stocks/{symbol}` | Score, filters and metrics for one stock |

## Running locally

Requirements: .NET 10 SDK, Node.js, and a SQL Server instance with a `StockAnalysisDB` database (the connection string is in `DatabaseConnection.cs`).

```bash
dotnet test 01_Stock/Stock_Analysis.Tests/Stock_Analysis.Tests.csproj
dotnet run --project "01_Stock/Stock_Analysis Web/Stock_Analysis Web.csproj" --launch-profile http
cd 01_Stock/Stock_Analysis.Frontend && npm install && npm run dev
```

Open http://localhost:5173. The downloaded-report folder defaults to the path in `NseDataPaths.cs`; set the `STOCK_DATA_DIR` environment variable to override it.

## Hosting (Netlify and Streamlit)

Netlify and Streamlit Community Cloud cannot reach a SQL Server running on a personal computer, so the hosted versions show a static snapshot of the ranking instead of live data.

1. Start the API locally, then export the snapshot:

   ```bash
   powershell -File 01_Stock/tools/export-snapshot.ps1
   ```

   This writes `snapshot.json` into `Stock_Analysis.Frontend/public` and `Stock_Analysis.Streamlit`. Commit and push both files.

2. **Netlify:** import the GitHub repo. `netlify.toml` sets the base folder, build command and `VITE_STATIC=true`, so the site reads `snapshot.json` and hides the "Update data" button.

3. **Streamlit Community Cloud:** create an app from the same repo with the main file `01_Stock/Stock_Analysis.Streamlit/streamlit_app.py`. Dependencies come from `requirements.txt` in that folder.

To refresh the hosted sites, re-run the export and push again.

## Status and roadmap

Done: download, parse, aggregate, store, filter, score, rank, desktop app, API, dashboard, unit tests for scoring, ranking, aggregation and report-file selection, and a GitHub Actions workflow.

Planned:

- Unique keys, indexes and batched inserts for the database tables.
- Tests for the repositories, downloader and parser.
- Historical backtesting of the score before any predictive claims.
- A separate machine-learning module with time-based train/test splits.
- A separate sentiment-analysis module.

## Disclaimer

This project is for educational and research purposes. Screening and ranking results are not investment advice.
