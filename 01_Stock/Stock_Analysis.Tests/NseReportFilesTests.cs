using Stock_Analysis.Engine.NSE;

namespace Stock_Analysis.Tests;

public class NseReportFilesTests
{
    private static string File(string date) =>
        $@"C:\data\BhavCopy_NSE_CM_0_0_0_{date}_F_0000.csv";

    [Fact]
    public void ParseDate_ReadsDateFromFileName()
    {
        Assert.Equal(new DateTime(2026, 8, 14), NseReportFiles.ParseDate(File("20260814")));
    }

    [Fact]
    public void ParseDate_ReturnsNullForUnrelatedOrInvalidNames()
    {
        Assert.Null(NseReportFiles.ParseDate(@"C:\data\notes.csv"));
        Assert.Null(NseReportFiles.ParseDate(File("20261399")));
    }

    [Fact]
    public void GetLatest_SkipsHolidayGapsAndReturnsLatestSevenInDateOrder()
    {
        string[] dates =
        {
            "20260810", "20260811", "20260812", "20260814", "20260817",
            "20260818", "20260819", "20260820", "20260821"
        };

        var result = NseReportFiles.GetLatestCsvFiles(dates.Select(File).Reverse(), 7);

        Assert.Equal(7, result.Count);
        Assert.Equal(File("20260812"), result.First());
        Assert.Equal(File("20260821"), result.Last());
    }

    [Fact]
    public void GetLatest_IgnoresFilesWithoutDatesAndDuplicateDates()
    {
        var result = NseReportFiles.GetLatestCsvFiles(
            new[] { File("20260814"), File("20260814"), @"C:\data\other.csv" }, 7);

        Assert.Single(result);
    }
}
