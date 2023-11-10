using System.Globalization;
using System.Windows;
using FileDB.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Converters;

[TestClass]
public class StringNullOrEmptyToVisibilityConverterTests
{
    [TestMethod]
    public void Convert()
    {
        var converter = new StringNullOrEmptyToVisibilityConverter();
        Assert.AreEqual(Visibility.Collapsed, converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture));
        Assert.AreEqual(Visibility.Collapsed, converter.Convert(string.Empty, typeof(string), null!, CultureInfo.InvariantCulture));
        Assert.AreEqual(Visibility.Visible, converter.Convert("not empty", typeof(string), null!, CultureInfo.InvariantCulture));
    }
}
