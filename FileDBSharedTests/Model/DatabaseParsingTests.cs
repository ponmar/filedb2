using FileDBShared.Model;
using Xunit;

namespace FileDBSharedTests.Model;

public class DatabaseParsingTests
{
    [Fact]
    public void ParsePersonsDateOfBirth_Day()
    {
        var result = DatabaseParsing.ParsePersonDateOfBirth("1957-03-23");
        Assert.Equal(new DateTime(year: 1957, month: 03, day: 23), result);
    }

    [Fact]
    public void ParsePersonsDateOfBirth_Month()
    {
        var result = DatabaseParsing.ParsePersonDateOfBirth("1957-03");
        Assert.Equal(new DateTime(year: 1957, month: 03, day: 1), result);
    }

    [Fact]
    public void ParsePersonsDateOfBirth_Year()
    {
        var result = DatabaseParsing.ParsePersonDateOfBirth("1957");
        Assert.Equal(new DateTime(year: 1957, month: 1, day: 1), result);
    }

    [Fact]
    public void ParsePersonsDeceasedDate_Day()
    {
        var result = DatabaseParsing.ParsePersonDeceasedDate("1957-03-23");
        Assert.Equal(new DateTime(year: 1957, month: 03, day: 23), result);
    }

    [Fact]
    public void ParsePersonsDeceasedDate_Month()
    {
        var result = DatabaseParsing.ParsePersonDeceasedDate("1957-03");
        Assert.Equal(new DateTime(year: 1957, month: 03, day: 1), result);
    }

    [Fact]
    public void ParsePersonsDeceasedDate_Year()
    {
        var result = DatabaseParsing.ParsePersonDeceasedDate("1957");
        Assert.Equal(new DateTime(year: 1957, month: 1, day: 1), result);
    }

    [Fact]
    public void ParseFilesDatetime_Null_ParseError()
    {
        var result = DatabaseParsing.ParseFilesDatetime(null);
        Assert.Null(result);
    }

    [Fact]
    public void ParseFilesDatetime_DateAndTime_Success()
    {
        var result = DatabaseParsing.ParseFilesDatetime("2005-08-23T21:31:51");
        Assert.Equal(new DateTime(year: 2005, month: 08, day: 23, hour: 21, minute: 31, second: 51), result);
    }

    [Fact]
    public void ParseFilesDatetime_Date_Success()
    {
        var result = DatabaseParsing.ParseFilesDatetime("2005-08-23");
        Assert.Equal(new DateTime(year: 2005, month: 08, day: 23), result);
    }

    [Fact]
    public void ParseFilesDatetime_YearAndMonth_Success()
    {
        var result = DatabaseParsing.ParseFilesDatetime("2005-08");
        Assert.Equal(new DateTime(year: 2005, month: 08, day: 1), result);
    }

    [Fact]
    public void ParseFilesDatetime_Year_Success()
    {
        var result = DatabaseParsing.ParseFilesDatetime("2005");
        Assert.Equal(new DateTime(year: 2005, month: 1, day: 1), result);
    }

    [Fact]
    public void DateTakenToFilesDatetime_DateAndTime_Success()
    {
        var result = DatabaseParsing.DateTakenToFilesDatetime(new DateTime(year: 1957, month: 03, day: 23, hour: 23, minute: 5, second: 54, millisecond: 123));
        Assert.Equal("1957-03-23T23:05:54", result);
    }

    [Fact]
    public void PathToFilesDatetime_ValidDate_Success()
    {
        Assert.Equal("1957-03-23", DatabaseParsing.PathToFilesDatetime("subdir/1957-03-23/file.jpg"));
    }

    [Fact]
    public void PathToFilesDatetime_DateAndTimeInFilename_Success()
    {
        Assert.Equal("1957-03-23T12:23:49", DatabaseParsing.PathToFilesDatetime("subdir/1957-03-23/19570323_122349.mp4"));
    }

    [Fact]
    public void PathToFilesDatetime_DateTimeAndPostfixInFilename_Success()
    {
        // This filename format is used by Samsung GT-I9195
        Assert.Equal("1957-03-23T12:23:49", DatabaseParsing.PathToFilesDatetime("subdir/1957-03-23/19570323_122349-1.mp4"));
    }

    [Fact]
    public void PathToFilesDatetime_DateTimePrefixAndPostfixInFilename_Success()
    {
        Assert.Equal("2015-12-18", DatabaseParsing.PathToFilesDatetime("subdir/IMG-20151218-WA0004.jpg"));
    }

    [Fact]
    public void PathToFilesDatetime_InvalidDate_Error()
    {
        Assert.Null(DatabaseParsing.PathToFilesDatetime("subdir/19570323/file.jpg"));
    }

    [Fact]
    public void ParseFilesPosition()
    {
        Assert.Equal((34.123, 75.321), DatabaseParsing.ParseFilesPosition("34.123 75.321"));
    }

    [Fact]
    public void ParseFilesPosition_InvalidLatitude()
    {
        Assert.Null(DatabaseParsing.ParseFilesPosition("-91 75.321"));
        Assert.Null(DatabaseParsing.ParseFilesPosition("91 75.321"));
    }

    [Fact]
    public void ParseFilesPosition_InvalidLongitude()
    {
        Assert.Null(DatabaseParsing.ParseFilesPosition("34.123 -181"));
        Assert.Null(DatabaseParsing.ParseFilesPosition("34.123 181"));
    }

    [Fact]
    public void ParseFilesPositionFromUrl_InvalidFormat()
    {
        Assert.Null(DatabaseParsing.ParseFilesPositionFromUrl("https://www.google.se"));
    }

    [Fact]
    public void ParseFilesPositionFromUrl_Format1()
    {
        var result = DatabaseParsing.ParseFilesPositionFromUrl("https://www.google.se/maps/place/Enhagsv%C3%A4gen+47,+589+43+Link%C3%B6ping/@58.3839033,15.7028438,17z/data=!3m1!4b1!4m5!3m4!1s0x46596c268b2f8165:0xf9d7a0f58269b2a1!8m2!3d58.3839033!4d15.7050325");
        Assert.NotNull(result);
        Assert.Equal(58.3839033, result.Value.lat);
        Assert.Equal(15.7028438, result.Value.lon);
    }

    [Fact]
    public void ParseFilesPositionFromUrl_Format2()
    {
        var result = DatabaseParsing.ParseFilesPositionFromUrl("https://www.google.com/maps?q=loc:58.41050909972222,15.618398199722222");
        Assert.NotNull(result);
        Assert.Equal(58.41050909972222, result.Value.lat);
        Assert.Equal(15.618398199722222, result.Value.lon);
    }

    [Fact]
    public void ToFilesPosition()
    {
        Assert.Equal("34.123 75.321", DatabaseParsing.ToFilesPosition(34.123, 75.321));
    }

    [Theory]
    [InlineData(-72, 0)]
    [InlineData(0, 1)]
    [InlineData(90, 8)]
    [InlineData(180, 3)]
    [InlineData(270, 6)]
    [InlineData(561, 0)]
    public void DegreesToOrientation(int degrees, int orientation)
    {
        Assert.Equal(orientation, DatabaseParsing.DegreesToOrientation(degrees));
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 180)]
    [InlineData(4, 0)]
    [InlineData(5, 0)]
    [InlineData(6, 270)]
    [InlineData(7, 0)]
    [InlineData(8, 90)]
    [InlineData(9, 0)]
    public void OrientationToDegrees(int? orientation, int degrees)
    {
        Assert.Equal(degrees, DatabaseParsing.OrientationToDegrees(orientation));
    }
}
