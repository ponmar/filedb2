using System.Collections.Generic;
using FileDB.Sorters;
using FileDBInterface.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Sorters;

[TestClass]
public class FilesByPathSorterTests
{
    private FilesModelByPathSorter sorter;

    [TestInitialize]
    public void Initialize()
    {
        sorter = new();
    }

    [TestMethod]
    public void Compare()
    {
        var items = new List<FilesModel>()
        {
            new FilesModel() { Id = 0, Path = "aaa" },
            new FilesModel() { Id = 2, Path = "ccc" },
            new FilesModel() { Id = 1, Path = "bbb" },
        };

        items.Sort(sorter);

        Assert.AreEqual(0, items[0].Id);
        Assert.AreEqual(1, items[1].Id);
        Assert.AreEqual(2, items[2].Id);
    }
}
