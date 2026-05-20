using FileDB.Sorters;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.Sorters;

public class FilesByDirectoryAndDateSorterTests
{
    [Fact]
    public void Compare_SortsByDirectoryThenDate()
    {
        var items = new List<FileModel>()
        {
            new FileModel() { Id = 3, Path = "dirB/file1", Datetime = "2000-01-01" },
            new FileModel() { Id = 0, Path = "dirA/file1", Datetime = "1999-01-01" },
            new FileModel() { Id = 2, Path = "dirB/file0", Datetime = "1999-01-01" },
            new FileModel() { Id = 1, Path = "dirA/file2", Datetime = "2000-01-01" },
        };

        var sorter = new FileModelByDirectoryAndDateSorter();
        items.Sort(sorter);

        Assert.Equal(0, items[0].Id); // dirA, 1999
        Assert.Equal(1, items[1].Id); // dirA, 2000
        Assert.Equal(2, items[2].Id); // dirB, 1999
        Assert.Equal(3, items[3].Id); // dirB, 2000
    }

    [Fact]
    public void Compare_NullDatetimeSortsLast()
    {
        var items = new List<FileModel>()
        {
            new FileModel() { Id = 2, Path = "dirA/fileC", Datetime = null },
            new FileModel() { Id = 0, Path = "dirA/fileA", Datetime = "1999-01-01" },
            new FileModel() { Id = 1, Path = "dirA/fileB", Datetime = "2000-01-01" },
        };

        var sorter = new FileModelByDirectoryAndDateSorter();
        items.Sort(sorter);

        Assert.Equal(0, items[0].Id);
        Assert.Equal(1, items[1].Id);
        Assert.Equal(2, items[2].Id);
    }

    [Fact]
    public void Compare_SameDirectoryAndDateTiebreaksByPath()
    {
        var items = new List<FileModel>()
        {
            new FileModel() { Id = 1, Path = "dirA/fileB", Datetime = "2000-01-01" },
            new FileModel() { Id = 0, Path = "dirA/fileA", Datetime = "2000-01-01" },
        };

        var sorter = new FileModelByDirectoryAndDateSorter();
        items.Sort(sorter);

        Assert.Equal(0, items[0].Id);
        Assert.Equal(1, items[1].Id);
    }
}
