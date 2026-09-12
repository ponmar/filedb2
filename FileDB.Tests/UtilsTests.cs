using FileDBInterface.Model;
using System.Globalization;
using Xunit;

namespace FileDB.Tests;

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
        Assert.Equal("1;2;3", Utils.CreateFileList([1, 2, 3]));
    }

    [Fact]
    public void CreateFileIds_EmptyString_ReturnsEmptyList()
    {
        var result = Utils.TryParseFileIds(string.Empty, out var fileIds);
        Assert.False(result);
        Assert.Null(fileIds);
    }

    [Fact]
    public void CreateFileIds_ValidList_ReturnsValidIds()
    {
        var result = Utils.TryParseFileIds("1;3;2", out var fileIds);

        Assert.True(result);
        Assert.Equal(fileIds, [1, 3, 2]);
    }

    [Fact]
    public void CreateShortFilePositionString()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        try
        {
            Utils.SetInvariantCulture();

            var result = Utils.CreateShortFilePositionString("50.123456789 49.987654321");
            Assert.Equal("50.123... 49.988...", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.DefaultThreadCurrentCulture = originalDefaultCulture;
        }
    }

    [Fact]
    public void CreateShortFilePositionString_SwedishUICulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        var originalDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        try
        {
            Utils.SetInvariantCulture();
            Utils.SetUICulture("se");

            var result = Utils.CreateShortFilePositionString("50.123456789 49.987654321");
            Assert.Equal("50.123... 49.988...", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
            CultureInfo.DefaultThreadCurrentCulture = originalDefaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = originalDefaultUiCulture;
        }
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
