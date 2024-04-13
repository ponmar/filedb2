using System;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Extensions;
using Xunit;

namespace FileDBInterfaceTests.Extensions;

public class DateTimeExtensionsTests
{
    [Theory]
    [InlineData(3, 19, Season.Spring)]
    [InlineData(6, 19, Season.Spring)]
    [InlineData(6, 20, Season.Summer)]
    [InlineData(9, 21, Season.Summer)]
    [InlineData(9, 22, Season.Autumn)]
    [InlineData(12, 20, Season.Autumn)]
    [InlineData(12, 21, Season.Winter)]
    [InlineData(3, 18, Season.Winter)]
    public void GetApproximatedSeason(int month, int day, Season expectedSeason)
    {
        // Arrange
        var date = new DateTime(2024, month, day);

        // Act
        var season = date.GetApproximatedSeason();

        // Assert
        Assert.Equal(expectedSeason, season);
    }
}
