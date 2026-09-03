using System.Globalization;
using FileDB.Converters;
using FileDB.Notifications;
using Xunit;

namespace FileDB.Tests.Converters;

public class NotificationSeverityToStringConverterTests
{
    [Theory]
    [InlineData(NotificationSeverity.Info, "\xD83D\xDEC8")]
    [InlineData(NotificationSeverity.Warning, "\u26A0")]
    [InlineData(NotificationSeverity.Error, "\u26A0")]
    public void Convert_ReturnsExpectedSymbol(NotificationSeverity severity, string expected)
    {
        var converter = new NotificationSeverityToStringConverter();

        var result = converter.Convert(severity, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_UnknownSeverity_ThrowsNotSupportedException()
    {
        var converter = new NotificationSeverityToStringConverter();

        Assert.Throws<NotSupportedException>(() => converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture));
    }
}
