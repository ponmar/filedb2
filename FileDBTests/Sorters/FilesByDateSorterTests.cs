using FileDB.Sorters;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.Sorters;

public class FilesByDateSorterTests
{
    [Fact]
    public void Compare()
    {
        var items = new List<FileModel>()
        {
            new FileModel() { Id = 3, Path = "a", Datetime = null },
            new FileModel() { Id = 1, Path = "path", Datetime = "2000-10-20" },
            new FileModel() { Id = 0, Path = "path", Datetime = "1999-10-20" },
            new FileModel() { Id = 2, Path = "path", Datetime = "2000-10-21" },
            new FileModel() { Id = 4, Path = "b", Datetime = null },
        };

        var sorter = new FileModelByDateSorter();
        items.Sort(sorter);

        Assert.Equal(0, items[0].Id);
        Assert.Equal(1, items[1].Id);
        Assert.Equal(2, items[2].Id);
        Assert.Equal(3, items[3].Id);
        Assert.Equal(4, items[4].Id);
    }
}
