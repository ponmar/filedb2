using FileDBAvalonia.Configuration;
using FileDBInterface.Extensions;

namespace FileDBAvaloniaTests.Configuration;

[TestClass]
public class WindowModeExtensionsTests
{
    [TestMethod]
    public void ToFriendlyString()
    {
        foreach (var value in Enum.GetValues<WindowMode>())
        {
            Assert.IsTrue(value.ToFriendlyString().HasContent());
        }
    }
}
