using System.Globalization;
using FileDB.Converters;
using Xunit;

namespace FileDB.Tests.Converters;

public class NumberToMonthConverterTests
{
    [Fact]
    public void Convert_ReturnsAbbreviatedMonthName()
    {
        var converter = new NumberToMonthConverter();
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            var result = converter.Convert(3, typeof(string), null, CultureInfo.InvariantCulture);

            Assert.Equal("Mar", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void Convert_Null_ReturnsNull()
    {
        var converter = new NumberToMonthConverter();

        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Null(result);
    }
}
