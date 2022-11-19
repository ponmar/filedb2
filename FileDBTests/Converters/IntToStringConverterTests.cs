using System.Globalization;
using FileDB.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Converters;

[TestClass]
public class IntToStringConverterTests
{
    [TestMethod]
    public void Convert()
    {
        var converter = new IntToStringConverter();
        Assert.AreEqual("1337", converter.Convert(1337, typeof(int), null, CultureInfo.InvariantCulture));
    }
}
