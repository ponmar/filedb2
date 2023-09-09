using FileDB.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Configuration;

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
