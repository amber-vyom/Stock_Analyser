using Stock_Analysis.Engine.Analysis.Filters;
using Stock_Analysis.Engine.Models;

namespace Stock_Analysis.Engine.Analysis;

public class StockFilterEngine
{
    private readonly Filter01Completeness filter01;
    private readonly Filter02PositiveReturn filter02;
    private readonly Filter03StrongReturn filter03;
    private readonly Filter04CloseAboveOpen filter04;
    private readonly Filter05PriceStability filter05;
    private readonly Filter06VolumeActivity filter06;
    private readonly Filter07VolumeConsistency filter07;
    private readonly Filter08StrongMomentum filter08;
    private readonly Filter09HighTradedValue filter09;
    private readonly Filter10OverallQuality filter10;

    public StockFilterEngine()
    {
        filter01 = new Filter01Completeness();
        filter02 = new Filter02PositiveReturn();
        filter03 = new Filter03StrongReturn();
        filter04 = new Filter04CloseAboveOpen();
        filter05 = new Filter05PriceStability();
        filter06 = new Filter06VolumeActivity();
        filter07 = new Filter07VolumeConsistency();
        filter08 = new Filter08StrongMomentum();
        filter09 = new Filter09HighTradedValue();
        filter10 = new Filter10OverallQuality();
    }

    public StockFilterResult Analyse(
        SevenDayStockData stock)
    {
        var result = new StockFilterResult
        {
            Symbol = stock.Symbol
        };

        result.Filter01Passed =
            filter01.Passes(stock);

        result.Filter02Passed =
            filter02.Passes(stock);

        result.Filter03Passed =
            filter03.Passes(stock);

        result.Filter04Passed =
            filter04.Passes(stock);

        result.Filter05Passed =
            filter05.Passes(stock);

        result.Filter06Passed =
            filter06.Passes(stock);

        result.Filter07Passed =
            filter07.Passes(stock);

        result.Filter08Passed =
            filter08.Passes(stock);

        result.Filter09Passed =
            filter09.Passes(stock);

        result.Filter10Passed =
            filter10.Passes(stock);

        result.PassedFilters =
            CountPassedFilters(result);

        result.Score =
            result.PassedFilters * 10;

        return result;
    }

    private int CountPassedFilters(
        StockFilterResult result)
    {
        int count = 0;

        if (result.Filter01Passed) count++;
        if (result.Filter02Passed) count++;
        if (result.Filter03Passed) count++;
        if (result.Filter04Passed) count++;
        if (result.Filter05Passed) count++;
        if (result.Filter06Passed) count++;
        if (result.Filter07Passed) count++;
        if (result.Filter08Passed) count++;
        if (result.Filter09Passed) count++;
        if (result.Filter10Passed) count++;

        return count;
    }
}