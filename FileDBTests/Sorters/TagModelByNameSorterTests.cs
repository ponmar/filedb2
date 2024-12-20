﻿using FileDB.Sorters;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.Sorters;

[Collection("Sequential")]
public class TagModelByNameSorterTests
{
    [Fact]
    public void Compare()
    {
        var items = new List<TagModel>()
        {
            new TagModel() { Id = 0, Name = "aaa" },
            new TagModel() { Id = 2, Name = "ccc" },
            new TagModel() { Id = 1, Name = "bbb" },
        };

        var sorter = new TagModelByNameSorter();
        items.Sort(sorter);

        Assert.Equal(0, items[0].Id);
        Assert.Equal(1, items[1].Id);
        Assert.Equal(2, items[2].Id);
    }
}
