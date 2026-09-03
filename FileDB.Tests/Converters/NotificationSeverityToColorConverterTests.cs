using System.Globalization;
using Avalonia.Media;
using FileDB.Converters;
using FileDB.Notifications;
using Xunit;

namespace FileDB.Tests.Converters;

public class NotificationSeverityToColorConverterTests
{
    [Theory]
    [InlineData(NotificationSeverity.Info, 0, 127, 0)]
    [InlineData(NotificationSeverity.Warning, 255, 127, 0)]
    [InlineData(NotificationSeverity.Error, 255, 0, 0)]
    public void Convert_ReturnsExpectedBrushColor(NotificationSeverity severity, byte red, byte green, byte blue)
    {
        var converter = new NotificationSeverityToColorConverter();

        var result = converter.Convert(severity, typeof(IBrush), null, CultureInfo.InvariantCulture);

        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Color.FromRgb(red, green, blue), brush.Color);
    }

    [Fact]
    public void Convert_UnknownSeverity_ThrowsNotSupportedException()
    {
        var converter = new NotificationSeverityToColorConverter();

        Assert.Throws<NotSupportedException>(() => converter.Convert(null, typeof(IBrush), null, CultureInfo.InvariantCulture));
    }
}
