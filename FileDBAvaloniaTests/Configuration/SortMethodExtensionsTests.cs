using FileDBAvalonia.Configuration;
using FileDBInterface.Extensions;
using Xunit;

namespace FileDBAvaloniaTests.Configuration;

public class SortMethodExtensionsTests
{
    [Fact]
    public void ToFriendlyString()
    {
        foreach (var value in Enum.GetValues<SortMethod>())
        {
            Assert.True(value.ToFriendlyString().HasContent());
        }
    }
}
