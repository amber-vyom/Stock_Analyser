namespace Stock_Analysis.Engine.Models;

public class StockFilterResult
{
    public string Symbol { get; set; } = "";

    // Filter Results

    public bool Filter01Passed { get; set; }

    public bool Filter02Passed { get; set; }

    public bool Filter03Passed { get; set; }

    public bool Filter04Passed { get; set; }

    public bool Filter05Passed { get; set; }

    public bool Filter06Passed { get; set; }

    public bool Filter07Passed { get; set; }

    public bool Filter08Passed { get; set; }

    public bool Filter09Passed { get; set; }

    public bool Filter10Passed { get; set; }


    // Summary

    public int PassedFilters { get; set; }

    public decimal Score { get; set; }
}