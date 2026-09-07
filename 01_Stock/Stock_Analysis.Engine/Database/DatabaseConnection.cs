namespace Stock_Analysis.Engine.Database;

public class DatabaseConnection
{
    public string ConnectionString { get; }

    public DatabaseConnection()
    {
        ConnectionString =
            "Server=INSPIRON1212\\AMBERDBE25;" +
            "Database=StockAnalysisDB;" +
            "Integrated Security=True;" +
            "TrustServerCertificate=True;";
    }
}