using FileDBInterface.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBInterfaceTests.Extensions;

[TestClass]
public class StruingExtensionsTests
{
    [TestMethod]
    public void HasContent()
    {
        Assert.IsFalse((null as string).HasContent());
        Assert.IsFalse("".HasContent());
        Assert.IsTrue("A".HasContent());
    }
}
