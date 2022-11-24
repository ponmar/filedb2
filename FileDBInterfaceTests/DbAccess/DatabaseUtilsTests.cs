using System;
using System.Data.SQLite;
using Dapper;
using FileDBInterface.DbAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBInterfaceTests.DbAccess
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
        public void GetAgeInYears()
        {
            Assert.AreEqual(0, DatabaseUtils.GetAgeInYears(new DateTime(2021, 5, 23), new DateTime(2020, 5, 24)));
            Assert.AreEqual(1, DatabaseUtils.GetAgeInYears(new DateTime(2021, 5, 23), new DateTime(2020, 5, 23)));
            Assert.AreEqual(0, DatabaseUtils.GetAgeInYears(new DateTime(1902, 4, 4), new DateTime(1902, 1, 1)));
            Assert.AreEqual(0, DatabaseUtils.GetAgeInYears(new DateTime(1902, 4, 4), new DateTime(1902, 7, 6)));
            Assert.AreEqual(0, DatabaseUtils.GetAgeInYears(new DateTime(2022, 10, 15, 11, 44, 0), new DateTime(2022, 10, 15, 11, 44, 0)));
        }

        [TestMethod]
        public void GetYearsAgo()
        {
            Assert.AreEqual(0, DatabaseUtils.GetYearsAgo(new DateTime(2021, 5, 23), new DateTime(2020, 5, 24)));
            Assert.AreEqual(1, DatabaseUtils.GetYearsAgo(new DateTime(2021, 5, 23), new DateTime(2020, 5, 23)));
        }

        [TestMethod]
        public void GetDaysToNextBirthday()
        {
            var tomorrow = DateTime.Today + TimeSpan.FromDays(1);
            Assert.AreEqual(1, DatabaseUtils.GetDaysToNextBirthday(tomorrow));
        }

        [TestMethod]
        public void DatabaseCreationSql_NoSyntaxError()
        {
            using var connection = new SQLiteConnection("Data Source=:memory:");
            connection.Execute(DatabaseUtils.DatabaseCreationSql);
        }
    }
}
