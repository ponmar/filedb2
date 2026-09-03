using FileDB.Converters;
using Xunit;

namespace FileDB.Tests.Converters;

public class BooleanOrConverterTests
{
    [Fact]
    public void Convert_WhenAnyValueIsTrue_ReturnsTrue()
    {
        var converter = new BooleanOrConverter();

        var result = converter.Convert([false, null, true], typeof(bool), null, null!);

        Assert.True((bool)result!);
    }

    [Fact]
    public void Convert_WhenNoValuesAreTrue_ReturnsFalse()
    {
        var converter = new BooleanOrConverter();

        var result = converter.Convert([false, null, false], typeof(bool), null, null!);

        Assert.False((bool)result!);
    }
}
