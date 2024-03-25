using FileDBAvalonia;
using FileDBAvalonia.Sorters;
using FileDBShared.Model;

namespace FileDBAvaloniaTests.Sorters;

[TestClass]
public class LocationModelByNameSorterTests
{
    private LocationModelByNameSorter sorter = new();

    [TestInitialize]
    public void Initialize()
    {
        Bootstrapper.Reset();

        sorter = new();
    }

    [TestMethod]
    public void Compare()
    {
        var items = new List<LocationModel>()
        {
            new LocationModel() { Id = 0, Name = "aaa" },
            new LocationModel() { Id = 2, Name = "ccc" },
            new LocationModel() { Id = 1, Name = "bbb" },
        };

        items.Sort(sorter);

        Assert.AreEqual(0, items[0].Id);
        Assert.AreEqual(1, items[1].Id);
        Assert.AreEqual(2, items[2].Id);
    }
}
