using System.Globalization;
using FileDB.Configuration;
using FileDB.Converters;
using FileDB.Extensions;
using FileDB.Lang;
using Xunit;

namespace FileDBTests.Converters;

public class WindowModeToStringConverterTests
{
    [Theory]
    [InlineData(WindowMode.Normal, nameof(Strings.WindowModeNormal))]
    [InlineData(WindowMode.Maximized, nameof(Strings.WindowModeMaximized))]
    [InlineData(WindowMode.Fullscreen, nameof(Strings.WindowModeFullscreen))]
    public void Convert_ReturnsFriendlyString(WindowMode windowMode, string _)
    {
        var converter = new WindowModeToStringConverter();

        var result = converter.Convert(windowMode, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(windowMode.ToFriendlyString(), result);
    }

    [Fact]
    public void Convert_Null_ReturnsNull()
    {
        var converter = new WindowModeToStringConverter();

        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Null(result);
    }
}
