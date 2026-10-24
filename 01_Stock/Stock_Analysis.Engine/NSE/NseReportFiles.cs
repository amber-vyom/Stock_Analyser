using System.Text.RegularExpressions;

namespace Stock_Analysis.Engine.NSE;

public static class NseReportFiles
{
    private static readonly Regex DatePattern =
        new(@"_(\d{8})_F_0000", RegexOptions.Compiled);

    public static DateTime? ParseDate(string fileName)
    {
        Match match = DatePattern.Match(Path.GetFileName(fileName));

        if (!match.Success)
        {
            return null;
        }

        return DateTime.TryParseExact(
            match.Groups[1].Value,
            "yyyyMMdd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out DateTime date)
            ? date
            : null;
    }

    public static List<string> GetLatestCsvFiles(
        IEnumerable<string> csvFiles,
        int count)
    {
        return csvFiles
            .Select(file => (File: file, Date: ParseDate(file)))
            .Where(x => x.Date.HasValue)
            .GroupBy(x => x.Date!.Value)
            .Select(g => g.First())
            .OrderByDescending(x => x.Date)
            .Take(count)
            .OrderBy(x => x.Date)
            .Select(x => x.File)
            .ToList();
    }
}
