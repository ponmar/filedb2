using System;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBInterfaceTests.Extensions;

[TestClass]
public class DateTimeExtensionsTests
{
    [DataRow(3, 19, Season.Spring)]
    [DataRow(6, 19, Season.Spring)]
    [DataRow(6, 20, Season.Summer)]
    [DataRow(9, 21, Season.Summer)]
    [DataRow(9, 22, Season.Autumn)]
    [DataRow(12, 20, Season.Autumn)]
    [DataRow(12, 21, Season.Winter)]
    [DataRow(3, 18, Season.Winter)]
    [TestMethod]
    public void GetApproximatedSeason(int month, int day, Season expectedSeason)
    {
        // Arrange
        var date = new DateTime(2024, month, day);

        // Act
        var season = date.GetApproximatedSeason();

        // Assert
        Assert.AreEqual(expectedSeason, season);
    }
}
