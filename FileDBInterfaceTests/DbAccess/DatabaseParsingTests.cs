using System;
using FileDBInterface.DbAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBInterfaceTests.DbAccess
{
    [TestClass]
    public class DatabaseParsingTests
    {
        [TestMethod]
        public void ParsePersonsDateOfBirth()
        {
            var result = DatabaseParsing.ParsePersonsDateOfBirth("1957-03-23");
            Assert.AreEqual(new DateTime(year: 1957, month: 03, day: 23), result);
        }

        [TestMethod]
        public void ToPersonsDateOfBirth()
        {
            var result = DatabaseParsing.ToPersonsDateOfBirth(new DateTime(year: 1957, month: 03, day: 23));
            Assert.AreEqual("1957-03-23", result);
        }

        [TestMethod]
        public void ParsePersonsDeceased()
        {
            var result = DatabaseParsing.ParsePersonsDeceased("1957-03-23");
            Assert.AreEqual(new DateTime(year: 1957, month: 03, day: 23), result);
        }

        [TestMethod]
        public void ToPersonsDeceased()
        {
            var result = DatabaseParsing.ToPersonsDeceased(new DateTime(year: 1957, month: 03, day: 23));
            Assert.AreEqual("1957-03-23", result);
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
            Assert.AreEqual(58.3839033, result.Value.lat);
            Assert.AreEqual(15.7028438, result.Value.lon);
        }

        [TestMethod]
        public void ParseFilesPositionFromUrl_Format2()
        {
            var result = DatabaseParsing.ParseFilesPositionFromUrl("https://www.google.com/maps?q=loc:58.41050909972222,15.618398199722222");
            Assert.AreEqual(58.41050909972222, result.Value.lat);
            Assert.AreEqual(15.618398199722222, result.Value.lon);
        }

        [TestMethod]
        public void ToFilesPosition()
        {
            Assert.AreEqual("34.123 75.321", DatabaseParsing.ToFilesPosition(34.123, 75.321));
        }
    }
}
