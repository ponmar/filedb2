using FileDBAvalonia.Comparers;
using FileDBShared.Model;
using Xunit;

namespace FileDBAvaloniaTests.Comparers;

public class FileModelByIdComparerTests
{
    [Fact]
    public void Equals_DifferentInstancesSameId_AreEqual()
    {
        var comparer = new FileModelByIdComparer();
        var model1 = new FileModel() { Id = 1, Path = "path" };
        var model2 = new FileModel() { Id = 1, Path = "path" };
        Assert.True(comparer.Equals(model1, model2));
    }

    [Fact]
    public void Equals_SameInstances_AreEqual()
    {
        var comparer = new FileModelByIdComparer();
        var model1 = new FileModel() { Id = 1, Path = "path" };
        Assert.True(comparer.Equals(model1, model1));
    }

    [Fact]
    public void Equals_DifferentIds_AreNotEqual()
    {
        var comparer = new FileModelByIdComparer();
        var model1 = new FileModel() { Id = 1, Path = "path1" };
        var model2 = new FileModel() { Id = 2, Path = "path2" };
        Assert.False(comparer.Equals(model1, model2));
    }
}
