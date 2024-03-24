using FileDBAvalonia.ViewModels.Dialogs;

namespace FileDBAvaloniaTests.ViewModels.Dialogs;

[TestClass]
public class DirectoryTreeCreatorTests
{
    [TestMethod]
    public void Build()
    {
        // Arrange
        var dirPaths = new List<string>
        {
            "0",
            "1/2/3",
        };

        // Act
        var dirs = DirectoryTreeCreator.Build(dirPaths);

        // Assert
        Assert.AreEqual(2, dirs.Count);

        Assert.AreEqual("0", dirs[0].Name);
        Assert.AreEqual("1", dirs[1].Name);

        Assert.AreEqual(0, dirs[0].Directories.Count);
        Assert.AreEqual(1, dirs[1].Directories.Count);
        Assert.AreEqual("2", dirs[1].Directories[0].Name);

        Assert.AreEqual(1, dirs[1].Directories[0].Directories.Count);
        Assert.AreEqual("3", dirs[1].Directories[0].Directories[0].Name);
        Assert.AreEqual(0, dirs[1].Directories[0].Directories[0].Directories.Count);
    }
}