using System.Globalization;
using FileDB.Configuration;
using FileDB.Converters;
using FileDB.Extensions;
using FileDB.Lang;
using Xunit;

namespace FileDB.Tests.Converters;

public class ThemeToStringConverterTests
{
    [Theory]
    [InlineData(Theme.Default, nameof(Strings.ThemeDefault))]
    [InlineData(Theme.Dark, nameof(Strings.ThemeDark))]
    [InlineData(Theme.Light, nameof(Strings.ThemeLight))]
    public void Convert_ReturnsFriendlyString(Theme theme, string _)
    {
        var converter = new ThemeToStringConverter();

        var result = converter.Convert(theme, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(theme.ToFriendlyString(), result);
    }

    [Fact]
    public void Convert_Null_ReturnsNull()
    {
        var converter = new ThemeToStringConverter();

        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Null(result);
    }
}
