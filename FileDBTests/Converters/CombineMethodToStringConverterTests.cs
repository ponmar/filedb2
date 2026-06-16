using System.Globalization;
using FileDB.Converters;
using FileDB.ViewModels.Search;
using Xunit;

namespace FileDBTests.Converters;

public class CombineMethodToStringConverterTests
{
    [Theory]
    [InlineData(CombineMethod.And, "AND")]
    [InlineData(CombineMethod.Or, "OR")]
    [InlineData(CombineMethod.Xor, "XOR")]
    public void Convert_ReturnsFriendlyString(CombineMethod combineMethod, string expected)
    {
        var converter = new CombineMethodToStringConverter();

        var result = converter.Convert(combineMethod, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_Null_ReturnsNull()
    {
        var converter = new CombineMethodToStringConverter();

        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Null(result);
    }
}
