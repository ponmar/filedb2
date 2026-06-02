using Dapper;
using FileDBInterface.DatabaseAccess.SQLite;
using System.Data.SQLite;
using Xunit;

namespace FileDBInterfaceTests.DatabaseAccess.SQLite;

public class SqLiteDatabaseCreatorTests
{
    [Fact]
    public void DatabaseCreationSql_NoSyntaxError()
    {
        using var connection = new SQLiteConnection("Data Source=:memory:");
        connection.Execute(SqLiteDatabaseCreator.DatabaseCreationSql);
    }
}
