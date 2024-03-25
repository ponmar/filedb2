using FileDBAvalonia;
using FileDBAvalonia.Sorters;
using FileDBShared.Model;

namespace FileDBAvaloniaTests.Sorters;

[TestClass]
public class FilesByDateSorterTests
{
    private FileModelByDateSorter sorter = new();

    [TestInitialize]
    public void Initialize()
    {
        Bootstrapper.Reset();

        sorter = new();
    }

    [TestMethod]
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

        items.Sort(sorter);

        Assert.AreEqual(0, items[0].Id);
        Assert.AreEqual(1, items[1].Id);
        Assert.AreEqual(2, items[2].Id);
        Assert.AreEqual(3, items[3].Id);
        Assert.AreEqual(4, items[4].Id);
    }
}
