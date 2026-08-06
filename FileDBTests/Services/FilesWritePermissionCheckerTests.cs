using FakeItEasy;
using FileDB.Model;
using FileDB.Services;
using FileDBInterface.FilesystemAccess;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace FileDBTests.Services;

public class FilesWritePermissionCheckerTests
{
    [Fact]
    public void HasWritePermission_WhenRootDirectoryIsWritable_ReturnsTrue()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), "filedb-services-tests", Guid.NewGuid().ToString("N"));
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(rootDirectory);
        var configProvider = A.Fake<IConfigProvider>();
        var filesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
        var filesystemAccess = A.Fake<IFilesystemAccess>();
        A.CallTo(() => configProvider.FilePaths).Returns(new ApplicationFilePaths(rootDirectory, "config", "database"));
        A.CallTo(() => filesystemAccessProvider.FilesystemAccess).Returns(filesystemAccess);
        A.CallTo(() => filesystemAccess.FileSystem).Returns(fileSystem);

        var checker = new FilesWritePermissionChecker(configProvider, filesystemAccessProvider);

        Assert.True(checker.HasWritePermission);
        Assert.Empty(fileSystem.Directory.GetFiles(rootDirectory));
    }

    [Fact]
    public void HasWritePermission_WhenRootDirectoryIsMissing_ReturnsFalse()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), "filedb-services-tests", Guid.NewGuid().ToString("N"));
        var fileSystem = new MockFileSystem();
        var configProvider = A.Fake<IConfigProvider>();
        var filesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
        var filesystemAccess = A.Fake<IFilesystemAccess>();
        A.CallTo(() => configProvider.FilePaths).Returns(new ApplicationFilePaths(rootDirectory, "config", "database"));
        A.CallTo(() => filesystemAccessProvider.FilesystemAccess).Returns(filesystemAccess);
        A.CallTo(() => filesystemAccess.FileSystem).Returns(fileSystem);

        var checker = new FilesWritePermissionChecker(configProvider, filesystemAccessProvider);

        Assert.False(checker.HasWritePermission);
    }
}
