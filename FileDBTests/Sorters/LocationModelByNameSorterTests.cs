using FileDB.Sorters;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.Sorters;

public class LocationModelByNameSorterTests
{
    [Fact]
    public void Compare()
    {
        var items = new List<LocationModel>()
        {
            new LocationModel() { Id = 0, Name = "aaa" },
            new LocationModel() { Id = 2, Name = "ccc" },
            new LocationModel() { Id = 1, Name = "bbb" },
        };

        var sorter = new LocationModelByNameSorter();
        items.Sort(sorter);

        Assert.Equal(0, items[0].Id);
        Assert.Equal(1, items[1].Id);
        Assert.Equal(2, items[2].Id);
    }
}
