using FileDBAvalonia;
using FileDBAvalonia.Sorters;
using FileDBShared.Model;
using Xunit;

namespace FileDBAvaloniaTests.Sorters;

[Collection("Sequential")]
public class LocationModelByNameSorterTests
{
    private LocationModelByNameSorter sorter;

    public LocationModelByNameSorterTests()
    {
        Bootstrapper.Reset();
        sorter = new();
    }

    [Fact]
    public void Compare()
    {
        var items = new List<LocationModel>()
        {
            new LocationModel() { Id = 0, Name = "aaa" },
            new LocationModel() { Id = 2, Name = "ccc" },
            new LocationModel() { Id = 1, Name = "bbb" },
        };

        items.Sort(sorter);

        Assert.Equal(0, items[0].Id);
        Assert.Equal(1, items[1].Id);
        Assert.Equal(2, items[2].Id);
    }
}
