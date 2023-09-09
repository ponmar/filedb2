using FileDB.Configuration;
using FileDB.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FileDBTests.Configuration;

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
