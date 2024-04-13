using FileDBAvalonia.Configuration;
using FileDBInterface.Extensions;
using Xunit;

namespace FileDBAvaloniaTests.Configuration;

public class WindowModeExtensionsTests
{
    [Fact]
    public void ToFriendlyString()
    {
        foreach (var value in Enum.GetValues<WindowMode>())
        {
            Assert.True(value.ToFriendlyString().HasContent());
        }
    }
}
