using FileDBAvalonia.ViewModels.Dialogs;
using Xunit;

namespace FileDBAvaloniaTests.ViewModels.Dialogs;

public class DirectoryTreeCreatorTests
{
    [Fact]
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
        Assert.Equal(2, dirs.Count);

        Assert.Equal("0", dirs[0].Name);
        Assert.Equal("1", dirs[1].Name);

        Assert.Empty(dirs[0].Directories);
        Assert.Single(dirs[1].Directories);
        Assert.Equal("2", dirs[1].Directories[0].Name);

        Assert.Single(dirs[1].Directories[0].Directories);
        Assert.Equal("3", dirs[1].Directories[0].Directories[0].Name);
        Assert.Empty(dirs[1].Directories[0].Directories[0].Directories);
    }
}