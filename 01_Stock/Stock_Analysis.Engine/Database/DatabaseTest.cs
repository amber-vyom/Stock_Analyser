using Microsoft.Data.SqlClient;

namespace Stock_Analysis.Engine.Database;

public class DatabaseTest
{
    private readonly DatabaseConnection databaseConnection;

    public DatabaseTest()
    {
        databaseConnection =
            new DatabaseConnection();
    }

    public async Task<bool> TestConnectionAsync()
    {
        using SqlConnection connection =
            new SqlConnection(
                databaseConnection.ConnectionString);

        await connection.OpenAsync();

        return connection.State ==
               System.Data.ConnectionState.Open;
    }
}