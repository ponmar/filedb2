using FileDB.Configuration;
using FileDB.Extensions;
using FileDBShared.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FileDBTests.Extensions;

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
