using System.Globalization;
using FileDB.Converters;
using FileDB.Model;
using Xunit;

namespace FileDBTests.Converters;

public class FilterTypeToVisibilityConverterTests
{
    [Fact]
    public void Convert_WhenValueMatchesParameter_ReturnsTrue()
    {
        var converter = new FilterTypeToVisibilityConverter();

        var result = converter.Convert(FilterType.Date, typeof(bool), FilterType.Date, CultureInfo.InvariantCulture);

        Assert.True((bool)result!);
    }

    [Fact]
    public void Convert_WhenValueDoesNotMatchParameter_ReturnsFalse()
    {
        var converter = new FilterTypeToVisibilityConverter();

        var result = converter.Convert(FilterType.Date, typeof(bool), FilterType.Text, CultureInfo.InvariantCulture);

        Assert.False((bool)result!);
    }

    [Fact]
    public void Convert_WhenValueIsNull_ReturnsFalse()
    {
        var converter = new FilterTypeToVisibilityConverter();

        var result = converter.Convert(null, typeof(bool), FilterType.Text, CultureInfo.InvariantCulture);

        Assert.False((bool)result!);
    }
}
