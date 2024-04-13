using Dapper;
using FileDBInterface.DatabaseAccess;
using System.Data.SQLite;
using Xunit;

namespace FileDBInterfaceTests.DatabaseAccess;

public class DatabaseSetupTests
{
    [Fact]
    public void DatabaseCreationSql_NoSyntaxError()
    {
        using var connection = new SQLiteConnection("Data Source=:memory:");
        connection.Execute(DatabaseSetup.DatabaseCreationSql);
    }
}
