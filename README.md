# Stock Analysis System

An NSE-based stock analysis system built with C# and .NET to automate market data collection, process historical stock data, screen equities using multiple filters, and rank stocks using data structures and algorithms.

## Overview

The Stock Analysis System is designed to transform raw NSE market data into structured, analysable stock information. It uses an ETL pipeline, SQL Server storage, statistical analysis, rule-based screening, and Priority Queue-based ranking.

## Key Features

- **Automated Data Collection:** Downloads available NSE equity BhavCopy reports.
- **ETL Pipeline:** Extracts, parses, transforms, and processes market data.
- **Seven-Day Analysis:** Aggregates stock data across recent trading days.
- **SQL Server Integration:** Stores raw and processed stock data.
- **Multi-Filter Screening:** Evaluates stocks using 10 screening filters.
- **DSA-Based Ranking:** Uses a Priority Queue to retrieve highly ranked stocks.
- **Desktop Application:** Provides a Windows interface for running the analysis pipeline.

## Technology Stack

| Component | Technology |
|---|---|
| Language | C# |
| Framework | .NET |
| Desktop UI | Windows Forms |
| Database | Microsoft SQL Server |
| Data Source | NSE market reports |
| Core Concepts | ETL, DSA, statistical analysis |
| Future Scope | Web API, web dashboard, machine learning |

## System Architecture

```text
NSE Market Reports
        |
        v
Data Download and Extraction
        |
        v
CSV Parsing and Validation
        |
        v
Raw Data Storage
        |
        v
Seven-Day Aggregation
        |
        v
Processed Data Storage
        |
        v
Ten Stock Filters
        |
        v
Priority Queue Ranking
        |
        v
Top 10 Stocks
```

## Project Structure

```text
Stock_Analysis
├── Stock_Analysis.Engine
│   ├── Analysis
│   ├── Database
│   ├── DSA
│   ├── ETL
│   ├── Models
│   ├── NSE
│   ├── Services
│   └── Tests
├── Stock_Analysis.Desktop
└── Stock_Analysis.Web
```

## Current Status

The project includes NSE data downloading, CSV parsing, seven-day aggregation, SQL Server integration, ten screening filters, and Priority Queue-based stock ranking.

The scoring system and data pipeline are being refined as development continues.

## Planned Enhancements

- Weighted stock scoring and improved screening.
- Persistent storage of final ranked stocks.
- ASP.NET Core Web API integration.
- Web-based dashboard.
- Machine learning models for predictive analysis.
- A separate sentiment analysis module.

## Disclaimer

This project is intended for educational and research purposes. Stock screening and ranking results are not guaranteed predictions of future market performance and should not be treated as investment advice.
