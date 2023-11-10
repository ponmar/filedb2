using System.Collections.Generic;
using FileDB;
using FileDB.Sorters;
using FileDBShared.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Sorters;

[TestClass]
public class FilesByPathSorterTests
{
    private FileModelByPathSorter sorter = new();

    [TestInitialize]
    public void Initialize()
    {
        Bootstrapper.Reset();

        sorter = new();
    }

    [TestMethod]
    public void Compare()
    {
        var items = new List<FileModel>()
        {
            new FileModel() { Id = 0, Path = "aaa" },
            new FileModel() { Id = 2, Path = "ccc" },
            new FileModel() { Id = 1, Path = "bbb" },
        };

        items.Sort(sorter);

        Assert.AreEqual(0, items[0].Id);
        Assert.AreEqual(1, items[1].Id);
        Assert.AreEqual(2, items[2].Id);
    }
}
