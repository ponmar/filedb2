using FileDB;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests;

public class UtilsTests
{
    [Fact]
    public void CreateFileList_NoFiles_EmptyString()
    {
        Assert.Equal(string.Empty, Utils.CreateFileList(new List<FileModel>()));
    }

    [Fact]
    public void CreateFileList_SomeFiles_ValidFileList()
    {
        var files = new List<FileModel>()
        {
            new FileModel() { Id = 1, Path = "path" },
            new FileModel() { Id = 2, Path = "path" },
            new FileModel() { Id = 3, Path = "path" },
        };
        Assert.Equal("1;2;3", Utils.CreateFileList(files));
    }

    [Fact]
    public void CreateFileList_FromIds()
    {
        IEnumerable<int> files = new List<int>() { 1, 2, 3 };
        Assert.Equal("1;2;3", Utils.CreateFileList(files));
    }

    [Fact]
    public void CreateFileIds_EmptyString_ReturnsEmptyList()
    {
        Assert.Empty(Utils.CreateFileIds(string.Empty));
    }

    [Fact]
    public void CreateFileIds_ValidList_ReturnsValidIds()
    {
        var result = Utils.CreateFileIds("1;3;2");

        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0]);
        Assert.Equal(3, result[1]);
        Assert.Equal(2, result[2]);
    }

    [Fact]
    public void CreateShortFilePositionString()
    {
        Utils.SetInvariantCulture();

        var result = Utils.CreateShortFilePositionString("50.123456789 49.987654321");
        Assert.Equal("50.123... 49.988...", result);
    }

    [Fact]
    public void CreateShortFilePositionString_SwedishUICulture()
    {
        Utils.SetInvariantCulture();
        Utils.SetUICulture("se");

        var result = Utils.CreateShortFilePositionString("50.123456789 49.987654321");
        Assert.Equal("50.123... 49.988...", result);
    }

    [Fact]
    public void CreatePositionLink()
    {
        var result = Utils.CreatePositionLink("10.5 11.2", "https://example.com/LAT_LON");
        Assert.Equal("https://example.com/10.5_11.2", result);
    }

    [Fact]
    public void CreateShortText()
    {
        Assert.Equal("short", Utils.CreateShortText("short", 10));
        Assert.Equal("l...", Utils.CreateShortText("long", 1));
    }
}
