﻿using FileDB.Extensions;
using FileDBInterface.Extensions;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.Extensions;

public class SexExtensionsTests
{
    [Fact]
    public void ToFriendlyString()
    {
        foreach (var value in Enum.GetValues<Sex>())
        {
            Assert.True(value.ToFriendlyString().HasContent());
        }
    }
}
