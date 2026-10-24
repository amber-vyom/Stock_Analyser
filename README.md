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
- **Interfaces:** A Windows Forms desktop app, a read-only ASP.NET Core API and a React dashboard.

## Architecture

```text
NSE reports -> download + extract -> CSV parser -> RawData (SQL Server)
           -> seven-day aggregation -> ProcessedData (SQL Server)
           -> ten filters -> weighted score -> Priority Queue -> Top N

React dashboard -> ASP.NET Core API -> Stock_Analysis.Engine -> SQL Server
Windows Forms app ----------------^
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

## API

| Endpoint | Description |
|---|---|
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
