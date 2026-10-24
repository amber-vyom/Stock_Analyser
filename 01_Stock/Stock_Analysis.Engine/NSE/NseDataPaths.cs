namespace Stock_Analysis.Engine.NSE;

public static class NseDataPaths
{
    private const string EnvironmentVariable = "STOCK_DATA_DIR";

    private const string DefaultRoot =
        @"A:\01_Vyom\03_Projects\01_Stock\Stock_Data\NSE";

    public static string Root
    {
        get
        {
            string? configured =
                Environment.GetEnvironmentVariable(EnvironmentVariable);

            return string.IsNullOrWhiteSpace(configured)
                ? DefaultRoot
                : configured;
        }
    }

    public static string RawFolder => Path.Combine(Root, "Raw");

    public static string ExtractedFolder => Path.Combine(Root, "Extracted");
}
