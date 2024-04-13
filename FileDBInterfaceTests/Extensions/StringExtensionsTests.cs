using FileDBInterface.Extensions;
using Xunit;

namespace FileDBInterfaceTests.Extensions;

public class StruingExtensionsTests
{
    [Fact]
    public void HasContent()
    {
        Assert.False((null as string).HasContent());
        Assert.False("".HasContent());
        Assert.True("A".HasContent());
    }
}
