namespace Stock_Analysis.Engine.Database;

public class DatabaseConnection
{
    public string ConnectionString { get; }

    public DatabaseConnection()
    {
        ConnectionString =
            "Server=DESKTOP-59M1SPM\\AMBERDBE25;" +
            "Database=StockAnalysisDB;" +
            "User Id=sa;" +
            "Password=Shubh@1234;" +
            "TrustServerCertificate=True;";
    }
}