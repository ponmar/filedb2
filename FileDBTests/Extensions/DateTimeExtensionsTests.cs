using System;
using FileDB.Extensions;
using Xunit;

namespace FileDBTests.Extensions;

public class DateTimeExtensionsTests
{
    [Fact]
    public void ToDateAndTime_WithRegularDateTime_ReturnsFormattedString()
    {
        // Arrange
        var dateTime = new DateTime(2024, 3, 15, 14, 30, 0);

        // Act
        var result = dateTime.ToDateAndTime();

        // Assert
        Assert.Equal("2024-03-15 14:30", result);
    }

    [Fact]
    public void ToDateAndTime_WithMidnight_ReturnsFormattedString()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 1, 0, 0, 0);

        // Act
        var result = dateTime.ToDateAndTime();

        // Assert
        Assert.Equal("2024-01-01 00:00", result);
    }

    [Fact]
    public void ToDateAndTime_WithNoon_ReturnsFormattedString()
    {
        // Arrange
        var dateTime = new DateTime(2024, 12, 31, 12, 0, 0);

        // Act
        var result = dateTime.ToDateAndTime();

        // Assert
        Assert.Equal("2024-12-31 12:00", result);
    }

    [Fact]
    public void ToDateAndTime_WithSingleDigitMonthAndDay_ReturnsPaddedString()
    {
        // Arrange
        var dateTime = new DateTime(2024, 5, 9, 8, 5, 0);

        // Act
        var result = dateTime.ToDateAndTime();

        // Assert
        Assert.Equal("2024-05-09 08:05", result);
    }

    [Fact]
    public void ToDateAndTime_WithSeconds_IgnoresSeconds()
    {
        // Arrange
        var dateTime = new DateTime(2024, 6, 15, 14, 30, 45);

        // Act
        var result = dateTime.ToDateAndTime();

        // Assert
        Assert.Equal("2024-06-15 14:30", result);
    }

    [Fact]
    public void ToDate_WithRegularDate_ReturnsFormattedString()
    {
        // Arrange
        var dateTime = new DateTime(2024, 3, 15, 14, 30, 0);

        // Act
        var result = dateTime.ToDate();

        // Assert
        Assert.Equal("2024-03-15", result);
    }

    [Fact]
    public void ToDate_WithTimeComponent_IgnoresTime()
    {
        // Arrange
        var dateTime = new DateTime(2024, 12, 31, 23, 59, 59);

        // Act
        var result = dateTime.ToDate();

        // Assert
        Assert.Equal("2024-12-31", result);
    }

    [Fact]
    public void ToDate_WithSingleDigitMonthAndDay_ReturnsPaddedString()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 9, 0, 0, 0);

        // Act
        var result = dateTime.ToDate();

        // Assert
        Assert.Equal("2024-01-09", result);
    }

    [Fact]
    public void ToDate_WithLeapYearDate_ReturnsFormattedString()
    {
        // Arrange
        var dateTime = new DateTime(2024, 2, 29);

        // Act
        var result = dateTime.ToDate();

        // Assert
        Assert.Equal("2024-02-29", result);
    }

    [Fact]
    public void ToDate_WithMinValue_ReturnsFormattedString()
    {
        // Arrange
        var dateTime = DateTime.MinValue; // 0001-01-01

        // Act
        var result = dateTime.ToDate();

        // Assert
        Assert.Equal("0001-01-01", result);
    }
}
