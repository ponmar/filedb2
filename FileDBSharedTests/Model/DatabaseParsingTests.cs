using FileDBShared.Model;

namespace FileDBSharedTests.Model;

[TestClass]
public class DatabaseParsingTests
{
    [TestMethod]
    public void ParsePersonsDateOfBirth_Day()
    {
        var result = DatabaseParsing.ParsePersonDateOfBirth("1957-03-23");
        Assert.AreEqual(new DateTime(year: 1957, month: 03, day: 23), result);
    }

    [TestMethod]
    public void ParsePersonsDateOfBirth_Month()
    {
        var result = DatabaseParsing.ParsePersonDateOfBirth("1957-03");
        Assert.AreEqual(new DateTime(year: 1957, month: 03, day: 1), result);
    }

    [TestMethod]
    public void ParsePersonsDateOfBirth_Year()
    {
        var result = DatabaseParsing.ParsePersonDateOfBirth("1957");
        Assert.AreEqual(new DateTime(year: 1957, month: 1, day: 1), result);
    }

    [TestMethod]
    public void ParsePersonsDeceasedDate_Day()
    {
        var result = DatabaseParsing.ParsePersonDeceasedDate("1957-03-23");
        Assert.AreEqual(new DateTime(year: 1957, month: 03, day: 23), result);
    }

    [TestMethod]
    public void ParsePersonsDeceasedDate_Month()
    {
        var result = DatabaseParsing.ParsePersonDeceasedDate("1957-03");
        Assert.AreEqual(new DateTime(year: 1957, month: 03, day: 1), result);
    }

    [TestMethod]
    public void ParsePersonsDeceasedDate_Year()
    {
        var result = DatabaseParsing.ParsePersonDeceasedDate("1957");
        Assert.AreEqual(new DateTime(year: 1957, month: 1, day: 1), result);
    }

    [TestMethod]
    public void ParseFilesDatetime_Null_ParseError()
    {
        var result = DatabaseParsing.ParseFilesDatetime(null);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ParseFilesDatetime_DateAndTime_Success()
    {
        var result = DatabaseParsing.ParseFilesDatetime("2005-08-23T21:31:51");
        Assert.AreEqual(new DateTime(year: 2005, month: 08, day: 23, hour: 21, minute: 31, second: 51), result);
    }

    [TestMethod]
    public void ParseFilesDatetime_Date_Success()
    {
        var result = DatabaseParsing.ParseFilesDatetime("2005-08-23");
        Assert.AreEqual(new DateTime(year: 2005, month: 08, day: 23), result);
    }

    [TestMethod]
    public void ParseFilesDatetime_YearAndMonth_Success()
    {
        var result = DatabaseParsing.ParseFilesDatetime("2005-08");
        Assert.AreEqual(new DateTime(year: 2005, month: 08, day: 1), result);
    }

    [TestMethod]
    public void ParseFilesDatetime_Year_Success()
    {
        var result = DatabaseParsing.ParseFilesDatetime("2005");
        Assert.AreEqual(new DateTime(year: 2005, month: 1, day: 1), result);
    }

    [TestMethod]
    public void DateTakenToFilesDatetime_DateAndTime_Success()
    {
        var result = DatabaseParsing.DateTakenToFilesDatetime(new DateTime(year: 1957, month: 03, day: 23, hour: 23, minute: 5, second: 54, millisecond: 123));
        Assert.AreEqual("1957-03-23T23:05:54", result);
    }

    [TestMethod]
    public void PathToFilesDatetime_ValidDate_Success()
    {
        Assert.AreEqual("1957-03-23", DatabaseParsing.PathToFilesDatetime("subdir/1957-03-23/file.jpg"));
    }

    [TestMethod]
    public void PathToFilesDatetime_DateAndTimeInFilename_Success()
    {
        Assert.AreEqual("1957-03-23T12:23:49", DatabaseParsing.PathToFilesDatetime("subdir/1957-03-23/19570323_122349.mp4"));
    }

    [TestMethod]
    public void PathToFilesDatetime_DateTimeAndPostfixInFilename_Success()
    {
        // This filename format is used by Samsung GT-I9195
        Assert.AreEqual("1957-03-23T12:23:49", DatabaseParsing.PathToFilesDatetime("subdir/1957-03-23/19570323_122349-1.mp4"));
    }

    [TestMethod]
    public void PathToFilesDatetime_DateTimePrefixAndPostfixInFilename_Success()
    {
        Assert.AreEqual("2015-12-18", DatabaseParsing.PathToFilesDatetime("subdir/IMG-20151218-WA0004.jpg"));
    }

    [TestMethod]
    public void PathToFilesDatetime_InvalidDate_Error()
    {
        Assert.IsNull(DatabaseParsing.PathToFilesDatetime("subdir/19570323/file.jpg"));
    }

    [TestMethod]
    public void ParseFilesPosition()
    {
        Assert.AreEqual((34.123, 75.321), DatabaseParsing.ParseFilesPosition("34.123 75.321"));
    }

    [TestMethod]
    public void ParseFilesPosition_InvalidLatitude()
    {
        Assert.IsNull(DatabaseParsing.ParseFilesPosition("-91 75.321"));
        Assert.IsNull(DatabaseParsing.ParseFilesPosition("91 75.321"));
    }

    [TestMethod]
    public void ParseFilesPosition_InvalidLongitude()
    {
        Assert.IsNull(DatabaseParsing.ParseFilesPosition("34.123 -181"));
        Assert.IsNull(DatabaseParsing.ParseFilesPosition("34.123 181"));
    }

    [TestMethod]
    public void ParseFilesPositionFromUrl_InvalidFormat()
    {
        Assert.IsNull(DatabaseParsing.ParseFilesPositionFromUrl("https://www.google.se"));
    }

    [TestMethod]
    public void ParseFilesPositionFromUrl_Format1()
    {
        var result = DatabaseParsing.ParseFilesPositionFromUrl("https://www.google.se/maps/place/Enhagsv%C3%A4gen+47,+589+43+Link%C3%B6ping/@58.3839033,15.7028438,17z/data=!3m1!4b1!4m5!3m4!1s0x46596c268b2f8165:0xf9d7a0f58269b2a1!8m2!3d58.3839033!4d15.7050325");
        Assert.IsNotNull(result);
        Assert.AreEqual(58.3839033, result.Value.lat);
        Assert.AreEqual(15.7028438, result.Value.lon);
    }

    [TestMethod]
    public void ParseFilesPositionFromUrl_Format2()
    {
        var result = DatabaseParsing.ParseFilesPositionFromUrl("https://www.google.com/maps?q=loc:58.41050909972222,15.618398199722222");
        Assert.IsNotNull(result);
        Assert.AreEqual(58.41050909972222, result.Value.lat);
        Assert.AreEqual(15.618398199722222, result.Value.lon);
    }

    [TestMethod]
    public void ToFilesPosition()
    {
        Assert.AreEqual("34.123 75.321", DatabaseParsing.ToFilesPosition(34.123, 75.321));
    }

    [DataTestMethod]
    [DataRow(-72, 0)]
    [DataRow(0, 1)]
    [DataRow(90, 8)]
    [DataRow(180, 3)]
    [DataRow(270, 6)]
    [DataRow(561, 0)]
    public void DegreesToOrientation(int degrees, int orientation)
    {
        Assert.AreEqual(orientation, DatabaseParsing.DegreesToOrientation(degrees));
    }

    [DataTestMethod]
    [DataRow(null, 0)]
    [DataRow(0, 0)]
    [DataRow(1, 0)]
    [DataRow(2, 0)]
    [DataRow(3, 180)]
    [DataRow(4, 0)]
    [DataRow(5, 0)]
    [DataRow(6, 270)]
    [DataRow(7, 0)]
    [DataRow(8, 90)]
    [DataRow(9, 0)]
    public void OrientationToDegrees(int? orientation, int degrees)
    {
        Assert.AreEqual(degrees, DatabaseParsing.OrientationToDegrees(orientation));
    }
}
