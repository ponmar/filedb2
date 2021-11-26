using System;
using FileDBInterface;
using MetadataExtractor;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBInterfaceTests
{
    [TestClass]
    public class DatabaseParsingTests
    {
        [TestMethod]
        public void ParsePersonsDateOfBirth()
        {
            Assert.AreEqual(new DateTime(year: 1957, month: 03, day: 23), DatabaseParsing.ParsePersonsDateOfBirth("1957-03-23"));
        }

        [TestMethod]
        public void ToPersonsDateOfBirth()
        {
            Assert.AreEqual("1957-03-23", DatabaseParsing.ToPersonsDateOfBirth(new DateTime(year: 1957, month: 03, day: 23)));
        }

        [TestMethod]
        public void ParsePersonsDeceased()
        {
            Assert.AreEqual(new DateTime(year: 1957, month: 03, day: 23), DatabaseParsing.ParsePersonsDeceased("1957-03-23"));
        }

        [TestMethod]
        public void ToPersonsDeceased()
        {
            Assert.AreEqual("1957-03-23", DatabaseParsing.ToPersonsDeceased(new DateTime(year: 1957, month: 03, day: 23)));
        }

        [TestMethod]
        public void ParseFilesDatetime()
        {
            Assert.AreEqual(new DateTime(year: 2005, month: 08, day: 23, hour: 21, minute: 31, second: 51), DatabaseParsing.ParseFilesDatetime("2005-08-23T21:31:51"));
            Assert.IsNull(DatabaseParsing.ParseFilesDatetime(null));

            Assert.IsTrue(DatabaseParsing.ParseFilesDatetime("2005-08-23T21:31:51", out var result));
            Assert.AreEqual(new DateTime(year: 2005, month: 08, day: 23, hour: 21, minute: 31, second: 51), result);
            Assert.IsFalse(DatabaseParsing.ParseFilesDatetime(null, out var _));
        }

        [TestMethod]
        public void DateTakenToFilesDatetime()
        {
            Assert.AreEqual("1957-03-23T23:05:54", DatabaseParsing.DateTakenToFilesDatetime(new DateTime(year: 1957, month: 03, day: 23, hour: 23, minute: 5, second: 54, millisecond: 123)));
        }

        [TestMethod]
        public void PathToFilesDatetime()
        {
            Assert.AreEqual("1957-03-23", DatabaseParsing.PathToFilesDatetime("subdir/1957-03-23/file.jpg"));
            Assert.IsNull(DatabaseParsing.PathToFilesDatetime("subdir/19570323/file.jpg"));
        }

        [TestMethod]
        public void ParseFilesPosition()
        {
            Assert.AreEqual((34.123, 75.321), DatabaseParsing.ParseFilesPosition("34.123 75.321"));
        }

        [TestMethod]
        public void ToFilesPosition()
        {
            Assert.AreEqual("34.123 75.321", DatabaseParsing.ToFilesPosition(34.123, 75.321));
        }
    }
}
