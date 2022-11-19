using System.Globalization;
using FileDB.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Converters;

[TestClass]
public class ValueToBoolConverterTests
{
    [TestMethod]
    public void Convert()
    {
        var converter = new ValueToBoolConverter();
        Assert.AreEqual(true, converter.Convert("not null", typeof(string), null, CultureInfo.InvariantCulture));
        Assert.AreEqual(false, converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture));
    }
}
