using FileDBAvalonia;
using FileDBAvalonia.Sorters;
using FileDBShared.Model;
using Xunit;

namespace FileDBAvaloniaTests.Sorters;

[Collection("Sequential")]
public class FilesByPathSorterTests
{
    private FileModelByPathSorter sorter;

    public FilesByPathSorterTests()
    {
        Bootstrapper.Reset();
        sorter = new();
    }

    [Fact]
    public void Compare()
    {
        var items = new List<FileModel>()
        {
            new FileModel() { Id = 0, Path = "aaa" },
            new FileModel() { Id = 2, Path = "ccc" },
            new FileModel() { Id = 1, Path = "bbb" },
        };

        items.Sort(sorter);

        Assert.Equal(0, items[0].Id);
        Assert.Equal(1, items[1].Id);
        Assert.Equal(2, items[2].Id);
    }
}
