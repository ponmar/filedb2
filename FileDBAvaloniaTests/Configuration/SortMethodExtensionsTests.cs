using FileDBAvalonia.Configuration;
using FileDBInterface.Extensions;

namespace FileDBAvaloniaTests.Configuration;

[TestClass]
public class SortMethodExtensionsTests
{
    [TestMethod]
    public void ToFriendlyString()
    {
        foreach (var value in Enum.GetValues<SortMethod>())
        {
            Assert.IsTrue(value.ToFriendlyString().HasContent());
        }
    }
}
