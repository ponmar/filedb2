using FileDB.Configuration;
using FileDBInterface.Extensions;
using Xunit;

namespace FileDBTests.Configuration;

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
