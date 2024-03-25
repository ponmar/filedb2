using FileDBAvalonia.Extensions;
using FileDBInterface.Extensions;
using FileDBShared.Model;

namespace FileDBAvaloniaTests.Extensions;

[TestClass]
public class SexExtensionsTests
{
    [TestMethod]
    public void ToFriendlyString()
    {
        foreach (var value in Enum.GetValues<Sex>())
        {
            Assert.IsTrue(value.ToFriendlyString().HasContent());
        }
    }
}
