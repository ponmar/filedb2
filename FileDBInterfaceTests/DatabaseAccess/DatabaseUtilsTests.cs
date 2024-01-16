using System.Data.SQLite;
using Dapper;
using FileDBInterface.DatabaseAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBInterfaceTests.DatabaseAccess
{
    [TestClass]
    public class DatabaseUtilsTests
    {
        [TestMethod]
        public void CalculateDistance()
        {
            Assert.AreEqual(103.97482585426138, DatabaseUtils.CalculateDistance(58.72018309972223, 14.577619799999999, 58.7211136, 14.5774585));
        }

        [TestMethod]
        public void DatabaseCreationSql_NoSyntaxError()
        {
            using var connection = new SQLiteConnection("Data Source=:memory:");
            connection.Execute(DatabaseSetup.DatabaseCreationSql);
        }
    }
}
