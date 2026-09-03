using System;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Extensions;
using Xunit;

namespace FileDBInterface.Tests.Extensions;

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
    [InlineData(1, 15, Season.Winter)]
    [InlineData(4, 15, Season.Spring)]
    [InlineData(7, 15, Season.Summer)]
    [InlineData(10, 15, Season.Autumn)]
    public void GetApproximatedSeason(int month, int day, Season expectedSeason)
    {
        // Arrange
        var date = new DateTime(2024, month, day);

        // Act
        var season = date.GetApproximatedSeason();

        // Assert
        Assert.Equal(expectedSeason, season);
    }

    [Fact]
    public void IsMonthAndDayInRange_WithinRange_ReturnsTrue()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15); // June 15

        // Act
        var result = date.IsMonthAndDayInRange(6, 1, 6, 30);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsMonthAndDayInRange_AtStartBoundary_ReturnsTrue()
    {
        // Arrange
        var date = new DateTime(2024, 3, 1); // March 1

        // Act
        var result = date.IsMonthAndDayInRange(3, 1, 5, 31);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsMonthAndDayInRange_AtEndBoundary_ReturnsTrue()
    {
        // Arrange
        var date = new DateTime(2024, 5, 31); // May 31

        // Act
        var result = date.IsMonthAndDayInRange(3, 1, 5, 31);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsMonthAndDayInRange_BeforeRange_ReturnsFalse()
    {
        // Arrange
        var date = new DateTime(2024, 2, 28); // February 28

        // Act
        var result = date.IsMonthAndDayInRange(3, 1, 5, 31);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsMonthAndDayInRange_AfterRange_ReturnsFalse()
    {
        // Arrange
        var date = new DateTime(2024, 6, 1); // June 1

        // Act
        var result = date.IsMonthAndDayInRange(3, 1, 5, 31);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsMonthAndDayInRange_WithInvalidDayForMonth_ClampsToMaxDay()
    {
        // Arrange
        var date = new DateTime(2024, 2, 29); // February 29 (leap year)

        // Act - February only has 29 days in 2024, so day 31 will be clamped to 29
        var result = date.IsMonthAndDayInRange(2, 1, 2, 31);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsMonthAndDayInRange_SpanningMultipleMonths_ReturnsTrue()
    {
        // Arrange
        var date = new DateTime(2024, 4, 15); // April 15

        // Act
        var result = date.IsMonthAndDayInRange(3, 20, 5, 10);

        // Assert
        Assert.True(result);
    }
}
