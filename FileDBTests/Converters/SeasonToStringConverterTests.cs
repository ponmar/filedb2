using System.Globalization;
using FileDB.Converters;
using FileDB.Extensions;
using FileDB.Lang;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.Converters;

public class SeasonToStringConverterTests
{
    [Theory]
    [InlineData(Season.Spring, nameof(Strings.SeasonSpring))]
    [InlineData(Season.Summer, nameof(Strings.SeasonSummer))]
    [InlineData(Season.Autumn, nameof(Strings.SeasonAutumn))]
    [InlineData(Season.Winter, nameof(Strings.SeasonWinter))]
    public void Convert_ReturnsFriendlyString(Season season, string _)
    {
        var converter = new SeasonToStringConverter();

        var result = converter.Convert(season, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(season.ToFriendlyString(), result);
    }

    [Fact]
    public void Convert_Null_ReturnsNull()
    {
        var converter = new SeasonToStringConverter();

        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Null(result);
    }
}
