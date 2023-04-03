using FileDB;
using FileDB.Comparers;
using FileDBShared.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Comparers;

[TestClass]
public class FileModelIdComparerTests
{
    [TestInitialize]
    public void Init()
    {
        Bootstrapper.Reset();
    }

    [TestMethod]
    public void Equals_DifferentInstancesSameId_AreEqual()
    {
        var comparer = new FileModelIdComparer();
        var model1 = new FileModel() { Id = 1, Path = "path" };
        var model2 = new FileModel() { Id = 1, Path = "path" };
        Assert.IsTrue(comparer.Equals(model1, model2));
    }

    [TestMethod]
    public void Equals_SameInstances_AreEqual()
    {
        var comparer = new FileModelIdComparer();
        var model1 = new FileModel() { Id = 1, Path = "path" };
        Assert.IsTrue(comparer.Equals(model1, model1));
    }

    [TestMethod]
    public void Equals_DifferentIds_AreNotEqual()
    {
        var comparer = new FileModelIdComparer();
        var model1 = new FileModel() { Id = 1, Path = "path1" };
        var model2 = new FileModel() { Id = 2, Path = "path2" };
        Assert.IsFalse(comparer.Equals(model1, model2));
    }
}
