using System.Globalization;
using FileDB.Converters;
using FileDB.Extensions;
using FileDB.Lang;
using FileDB.Model;
using Xunit;

namespace FileDB.Tests.Converters;

public class FilterTypeToStringConverterTests
{
    [Theory]
    [InlineData(FilterType.Text, nameof(Strings.FilterTypeText))]
    [InlineData(FilterType.Date, nameof(Strings.FilterTypeDateTime))]
    [InlineData(FilterType.NoDateTime, nameof(Strings.FilterTypeNoDateTime))]
    [InlineData(FilterType.Combine, nameof(Strings.FilterTypeCombine))]
    public void Convert_ReturnsFriendlyString(FilterType filterType, string _)
    {
        var converter = new FilterTypeToStringConverter();

        var result = converter.Convert(filterType, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(filterType.ToFriendlyString(), result);
    }

    [Fact]
    public void Convert_Null_ReturnsNull()
    {
        var converter = new FilterTypeToStringConverter();

        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Null(result);
    }
}
