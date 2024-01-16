using Dapper;
using FileDBInterface.DatabaseAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SQLite;

namespace FileDBInterfaceTests.DatabaseAccess;

[TestClass]
public class DatabaseSetupTests
{
    [TestMethod]
    public void DatabaseCreationSql_NoSyntaxError()
    {
        using var connection = new SQLiteConnection("Data Source=:memory:");
        connection.Execute(DatabaseSetup.DatabaseCreationSql);
    }
}
