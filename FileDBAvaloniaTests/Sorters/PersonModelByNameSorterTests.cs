using FileDBAvalonia.Sorters;
using FileDBInterface.Model;
using Xunit;

namespace FileDBAvaloniaTests.Sorters;

public class PersonModelByNameSorterTests
{
    [Fact]
    public void Compare()
    {
        var items = new List<PersonModel>()
        {
            new PersonModel() { Id = 0, Firstname = "A", Lastname = "A" },
            new PersonModel() { Id = 2, Firstname = "C", Lastname = "C" },
            new PersonModel() { Id = 1, Firstname = "A", Lastname = "B" },
        };

        var sorter = new PersonModelByNameSorter();
        items.Sort(sorter);

        Assert.Equal(0, items[0].Id);
        Assert.Equal(1, items[1].Id);
        Assert.Equal(2, items[2].Id);
    }
}
