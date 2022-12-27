using System.Collections.Generic;
using FileDB.Sorters;
using FileDBShared.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Sorters;

[TestClass]
public class PersonModelByNameSorterTests
{
    private PersonModelByNameSorter sorter;

    [TestInitialize]
    public void Initialize()
    {
        sorter = new();
    }

    [TestMethod]
    public void Compare()
    {
        var items = new List<PersonModel>()
        {
            new PersonModel() { Id = 0, Firstname = "A", Lastname = "A" },
            new PersonModel() { Id = 2, Firstname = "C", Lastname = "C" },
            new PersonModel() { Id = 1, Firstname = "A", Lastname = "B" },
        };

        items.Sort(sorter);

        Assert.AreEqual(0, items[0].Id);
        Assert.AreEqual(1, items[1].Id);
        Assert.AreEqual(2, items[2].Id);
    }
}
