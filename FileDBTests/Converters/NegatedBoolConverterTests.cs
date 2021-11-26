using System.Globalization;
using FileDB.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Converters
{
    [TestClass]
    public class NegatedBoolConverterTests
    {
        [TestMethod]
        public void Convert()
        {
            var converter = new NegatedBoolConverter();
            Assert.AreEqual(true, converter.Convert(false, typeof(bool), null, CultureInfo.InvariantCulture));
            Assert.AreEqual(false, converter.Convert(true, typeof(bool), null, CultureInfo.InvariantCulture));
        }
    }
}
