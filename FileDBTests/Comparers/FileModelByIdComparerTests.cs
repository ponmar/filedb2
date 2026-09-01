using FileDB.Comparers;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.Comparers;

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

    [Fact]
    public void Equals_FirstIsNull_ReturnsFalse()
    {
        var comparer = new FileModelByIdComparer();
        var model = new FileModel() { Id = 1, Path = "path" };
        Assert.False(comparer.Equals(null, model));
    }

    [Fact]
    public void Equals_SecondIsNull_ReturnsFalse()
    {
        var comparer = new FileModelByIdComparer();
        var model = new FileModel() { Id = 1, Path = "path" };
        Assert.False(comparer.Equals(model, null));
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        var comparer = new FileModelByIdComparer();
        Assert.True(comparer.Equals(null, null));
    }

    [Fact]
    public void GetHashCode_NullModel_ReturnsZero()
    {
        var comparer = new FileModelByIdComparer();
        Assert.Equal(0, comparer.GetHashCode(null!));
    }

    [Fact]
    public void GetHashCode_SameId_ReturnsSameHash()
    {
        var comparer = new FileModelByIdComparer();
        var model1 = new FileModel() { Id = 42, Path = "a" };
        var model2 = new FileModel() { Id = 42, Path = "b" };
        Assert.Equal(comparer.GetHashCode(model1), comparer.GetHashCode(model2));
    }

    [Fact]
    public void GetHashCode_DifferentIds_ReturnDifferentHashes()
    {
        var comparer = new FileModelByIdComparer();
        var model1 = new FileModel() { Id = 1, Path = "a" };
        var model2 = new FileModel() { Id = 2, Path = "b" };
        Assert.NotEqual(comparer.GetHashCode(model1), comparer.GetHashCode(model2));
    }
}
