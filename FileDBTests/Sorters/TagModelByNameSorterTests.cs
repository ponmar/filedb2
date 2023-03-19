﻿using System.Collections.Generic;
using FileDB;
using FileDB.Sorters;
using FileDBShared.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Sorters;

[TestClass]
public class TagModelByNameSorterTests
{
    private TagModelByNameSorter sorter;

    [TestInitialize]
    public void Initialize()
    {
        Bootstrapper.Reset();

        sorter = new();
    }

    [TestMethod]
    public void Compare()
    {
        var items = new List<TagModel>()
        {
            new TagModel() { Id = 0, Name = "aaa" },
            new TagModel() { Id = 2, Name = "ccc" },
            new TagModel() { Id = 1, Name = "bbb" },
        };

        items.Sort(sorter);

        Assert.AreEqual(0, items[0].Id);
        Assert.AreEqual(1, items[1].Id);
        Assert.AreEqual(2, items[2].Id);
    }
}
