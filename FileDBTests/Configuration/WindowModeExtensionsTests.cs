using FileDB.Configuration;
using FileDB.Extensions;
using FileDBInterface.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FileDBTests.Configuration;

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
