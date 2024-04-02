﻿using FileDBAvalonia;
using FileDBAvalonia.Comparers;
using FileDBShared.Model;

namespace FileDBAvaloniaTests.Comparers;

[TestClass]
public class FileModelByIdComparerTests
{
    [TestInitialize]
    public void Init()
    {
        Bootstrapper.Reset();
    }

    [TestMethod]
    public void Equals_DifferentInstancesSameId_AreEqual()
    {
        var comparer = new FileModelByIdComparer();
        var model1 = new FileModel() { Id = 1, Path = "path" };
        var model2 = new FileModel() { Id = 1, Path = "path" };
        Assert.IsTrue(comparer.Equals(model1, model2));
    }

    [TestMethod]
    public void Equals_SameInstances_AreEqual()
    {
        var comparer = new FileModelByIdComparer();
        var model1 = new FileModel() { Id = 1, Path = "path" };
        Assert.IsTrue(comparer.Equals(model1, model1));
    }

    [TestMethod]
    public void Equals_DifferentIds_AreNotEqual()
    {
        var comparer = new FileModelByIdComparer();
        var model1 = new FileModel() { Id = 1, Path = "path1" };
        var model2 = new FileModel() { Id = 2, Path = "path2" };
        Assert.IsFalse(comparer.Equals(model1, model2));
    }
}