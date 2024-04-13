using FileDBAvalonia;
using FileDBAvalonia.Sorters;
using FileDBShared.Model;
using Xunit;

namespace FileDBAvaloniaTests.Sorters;

[Collection("Sequential")]
public class PersonModelByNameSorterTests
{
    private PersonModelByNameSorter sorter;

    public PersonModelByNameSorterTests()
    {
        Bootstrapper.Reset();
        sorter = new();
    }

    [Fact]
    public void Compare()
    {
        var items = new List<PersonModel>()
        {
            new PersonModel() { Id = 0, Firstname = "A", Lastname = "A" },
            new PersonModel() { Id = 2, Firstname = "C", Lastname = "C" },
            new PersonModel() { Id = 1, Firstname = "A", Lastname = "B" },
        };

        items.Sort(sorter);

        Assert.Equal(0, items[0].Id);
        Assert.Equal(1, items[1].Id);
        Assert.Equal(2, items[2].Id);
    }
}
