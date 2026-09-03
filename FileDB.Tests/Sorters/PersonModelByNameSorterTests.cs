using FileDB.Sorters;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.Sorters;

public class PersonModelByNameSorterTests
{
    [Fact]
    public void Compare()
    {
        var items = new List<PersonModel>()
        {
            new() { Id = 0, ShortName = "A", FullName = "A" },
            new() { Id = 2, ShortName = "C", FullName = "C" },
            new() { Id = 1, ShortName = "A", FullName = "B" },
        };

        var sorter = new PersonModelByNameSorter();
        items.Sort(sorter);

        Assert.Equal(0, items[0].Id);
        Assert.Equal(1, items[1].Id);
        Assert.Equal(2, items[2].Id);
    }
}
