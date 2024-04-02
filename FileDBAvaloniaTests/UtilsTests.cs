﻿using FileDBAvalonia;
using FileDBShared.Model;

namespace FileDBAvaloniaTests;

[TestClass]
public class UtilsTests
{
    [TestInitialize]
    public void Init()
    {
        Bootstrapper.Reset();
    }

    [TestMethod]
    public void CreateFileList_NoFiles_EmptyString()
    {
        Assert.AreEqual(string.Empty, Utils.CreateFileList(new List<FileModel>()));
    }

    [TestMethod]
    public void CreateFileList_SomeFiles_ValidFileList()
    {
        var files = new List<FileModel>()
        {
            new FileModel() { Id = 1, Path = "path" },
            new FileModel() { Id = 2, Path = "path" },
            new FileModel() { Id = 3, Path = "path" },
        };
        Assert.AreEqual("1;2;3", Utils.CreateFileList(files));
    }

    [TestMethod]
    public void CreateFileList_FromIds()
    {
        IEnumerable<int> files = new List<int>() { 1, 2, 3 };
        Assert.AreEqual("1;2;3", Utils.CreateFileList(files));
    }

    [TestMethod]
    public void CreateFileIds_EmptyString_ReturnsEmptyList()
    {
        Assert.AreEqual(0, Utils.CreateFileIds(string.Empty).Count);
    }

    [TestMethod]
    public void CreateFileIds_ValidList_ReturnsValidIds()
    {
        var result = Utils.CreateFileIds("1;3;2");

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(1, result[0]);
        Assert.AreEqual(3, result[1]);
        Assert.AreEqual(2, result[2]);
    }

    [TestMethod]
    public void CreateShortFilePositionString()
    {
        Utils.SetInvariantCulture();

        var result = Utils.CreateShortFilePositionString("50.123456789 49.987654321");
        Assert.AreEqual("50.123... 49.988...", result);
    }

    [TestMethod]
    public void CreateShortFilePositionString_SwedishUICulture()
    {
        Utils.SetInvariantCulture();
        Utils.SetUICulture("se");

        var result = Utils.CreateShortFilePositionString("50.123456789 49.987654321");
        Assert.AreEqual("50.123... 49.988...", result);
    }

    [TestMethod]
    public void CreatePositionLink()
    {
        var result = Utils.CreatePositionLink("10.5 11.2", "https://example.com/LAT_LON");
        Assert.AreEqual("https://example.com/10.5_11.2", result);
    }

    [TestMethod]
    public void CreateShortText()
    {
        Assert.AreEqual("short", Utils.CreateShortText("short", 10));
        Assert.AreEqual("l...", Utils.CreateShortText("long", 1));
    }
}