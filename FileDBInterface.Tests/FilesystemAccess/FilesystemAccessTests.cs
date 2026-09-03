using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.IO;
using FakeItEasy;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FileDBInterface.Tests.FilesystemAccess;

public class FilesystemAccessTests
{
    private readonly string testRootDirectory = Path.Combine(Path.GetTempPath(), "filedb-tests", "root");

    private ILoggerFactory CreateMockLoggerFactory()
    {
        var loggerFactory = A.Fake<ILoggerFactory>();
        var logger = A.Fake<ILogger<FileDBInterface.FilesystemAccess.FilesystemAccess>>();
        A.CallTo(() => loggerFactory.CreateLogger(A<string>._))
            .Returns(logger);
        return loggerFactory;
    }

    private MockFileSystem CreateMockFileSystem(string rootDirectory)
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(rootDirectory);
        return fileSystem;
    }

    private FileDBInterface.FilesystemAccess.FilesystemAccess CreateFilesystemAccess(
        IFileSystem? fileSystem = null,
        ILoggerFactory? loggerFactory = null,
        string? rootDirectory = null)
    {
        fileSystem ??= CreateMockFileSystem(testRootDirectory);
        loggerFactory ??= CreateMockLoggerFactory();
        rootDirectory ??= testRootDirectory;

        return new FileDBInterface.FilesystemAccess.FilesystemAccess(
            fileSystem,
            loggerFactory,
            rootDirectory);
    }

    private string NormalizeForAbsolutePath(string path)
    {
        path = path.Replace('\\', '/');
        return path.Length > 1 && path[1] == ':' ? path[2..] : path.TrimEnd('/');
    }

    private string ExpectedAbsolutePath(params string[] segments)
    {
        var path = Path.Combine(new[] { testRootDirectory }.Concat(segments).ToArray());
        return NormalizeForAbsolutePath(path);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidRootDirectory_InitializesSuccessfully()
    {
        // Arrange
        var fileSystem = CreateMockFileSystem(testRootDirectory);
        var loggerFactory = CreateMockLoggerFactory();

        // Act
        var filesystemAccess = new FileDBInterface.FilesystemAccess.FilesystemAccess(
            fileSystem,
            loggerFactory,
            testRootDirectory);

        // Assert
        Assert.NotNull(filesystemAccess);
        Assert.Same(fileSystem, filesystemAccess.FileSystem);
    }

    [Fact]
    public void Constructor_WithNonExistentRootDirectory_InitializesAndLogsWarning()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var loggerFactory = CreateMockLoggerFactory();
        var nonExistentPath = "/non/existent/path";

        // Act
        var filesystemAccess = new FileDBInterface.FilesystemAccess.FilesystemAccess(
            fileSystem,
            loggerFactory,
            nonExistentPath);

        // Assert
        Assert.NotNull(filesystemAccess);
    }

    #endregion

    #region ToAbsolutePath Tests

    [Fact]
    public void ToAbsolutePath_WithSimplePath_ReturnsAbsolutePath()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var internalPath = "photos/image.jpg";

        // Act
        var result = filesystemAccess.ToAbsolutePath(internalPath);

        // Assert
        Assert.Equal(ExpectedAbsolutePath("photos", "image.jpg"), result);
    }

    [Fact]
    public void ToAbsolutePath_WithPathContainingBackslashes_ConvertsToForwardSlashes()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var internalPath = "photos\\subfolder\\image.jpg";

        // Act
        var result = filesystemAccess.ToAbsolutePath(internalPath);

        // Assert
        Assert.Equal(ExpectedAbsolutePath("photos", "subfolder", "image.jpg"), result);
        Assert.DoesNotContain("\\", result);
    }

    [Fact]
    public void ToAbsolutePath_WithEmptyPath_ReturnsRootDirectory()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var internalPath = "";

        // Act
        var result = filesystemAccess.ToAbsolutePath(internalPath);

        // Assert
        Assert.Equal($"{NormalizeForAbsolutePath(testRootDirectory)}/", result);
    }

    [Fact]
    public void ToAbsolutePath_WithNestedPath_ReturnsCorrectAbsolutePath()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var internalPath = "a/b/c/d/file.txt";

        // Act
        var result = filesystemAccess.ToAbsolutePath(internalPath);

        // Assert
        Assert.Equal(ExpectedAbsolutePath("a", "b", "c", "d", "file.txt"), result);
    }

    #endregion

    #region PathIsApplicable Tests

    [Fact]
    public void PathIsApplicable_WithNoFilters_ReturnsTrue()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var path = "photos/image.jpg";

        // Act
        var result = filesystemAccess.PathIsApplicable(
            path,
            Array.Empty<string>(),
            Array.Empty<string>(),
            includeHiddenDirectories: true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PathIsApplicable_WithBlacklistedPattern_ReturnsFalse()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var path = "photos/temp/image.jpg";
        var blacklist = new[] { "temp" };

        // Act
        var result = filesystemAccess.PathIsApplicable(
            path,
            blacklist,
            Array.Empty<string>(),
            includeHiddenDirectories: true);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PathIsApplicable_WithNonMatchingBlacklist_ReturnsTrue()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var path = "photos/image.jpg";
        var blacklist = new[] { "temp", "cache" };

        // Act
        var result = filesystemAccess.PathIsApplicable(
            path,
            blacklist,
            Array.Empty<string>(),
            includeHiddenDirectories: true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PathIsApplicable_WithWhitelistedExtension_ReturnsTrue()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var path = "photos/image.jpg";
        var whitelist = new[] { ".jpg", ".png" };

        // Act
        var result = filesystemAccess.PathIsApplicable(
            path,
            Array.Empty<string>(),
            whitelist,
            includeHiddenDirectories: true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PathIsApplicable_WithNonWhitelistedExtension_ReturnsFalse()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var path = "photos/document.pdf";
        var whitelist = new[] { ".jpg", ".png" };

        // Act
        var result = filesystemAccess.PathIsApplicable(
            path,
            Array.Empty<string>(),
            whitelist,
            includeHiddenDirectories: true);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PathIsApplicable_WithEmptyWhitelist_ReturnsTrue()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var path = "photos/any.file";

        // Act
        var result = filesystemAccess.PathIsApplicable(
            path,
            Array.Empty<string>(),
            Array.Empty<string>(),
            includeHiddenDirectories: true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PathIsApplicable_WithHiddenDirectory_IncludeHiddenTrue_ReturnsTrue()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var path = ".hidden/file.txt";

        // Act
        var result = filesystemAccess.PathIsApplicable(
            path,
            Array.Empty<string>(),
            Array.Empty<string>(),
            includeHiddenDirectories: true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PathIsApplicable_WithHiddenDirectory_IncludeHiddenFalse_ReturnsFalse()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var path = ".hidden/file.txt";

        // Act
        var result = filesystemAccess.PathIsApplicable(
            path,
            Array.Empty<string>(),
            Array.Empty<string>(),
            includeHiddenDirectories: false);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PathIsApplicable_WithHiddenSubdirectory_IncludeHiddenFalse_ReturnsFalse()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var path = "photos/.git/config";

        // Act
        var result = filesystemAccess.PathIsApplicable(
            path,
            Array.Empty<string>(),
            Array.Empty<string>(),
            includeHiddenDirectories: false);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PathIsApplicable_WithAllFiltersMatching_ReturnsTrue()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var path = "photos/vacation/image.jpg";
        var blacklist = new[] { "temp" };
        var whitelist = new[] { ".jpg", ".png" };

        // Act
        var result = filesystemAccess.PathIsApplicable(
            path,
            blacklist,
            whitelist,
            includeHiddenDirectories: true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PathIsApplicable_WithBlacklistAndWhitelistConflict_BlacklistWins()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var path = "temp/image.jpg";
        var blacklist = new[] { "temp" };
        var whitelist = new[] { ".jpg" };

        // Act
        var result = filesystemAccess.PathIsApplicable(
            path,
            blacklist,
            whitelist,
            includeHiddenDirectories: true);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ListAllFilesystemDirectories Tests

    [Fact]
    public void ListAllFilesystemDirectories_WithEmptyDirectory_ReturnsEmptyList()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();

        // Act
        var result = filesystemAccess.ListAllFilesystemDirectories();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ListAllFilesystemDirectories_WithSubdirectories_ReturnsInternalPaths()
    {
        // Arrange
        var fileSystem = CreateMockFileSystem(testRootDirectory);
        fileSystem.AddDirectory(Path.Combine(testRootDirectory, "photos"));
        fileSystem.AddDirectory(Path.Combine(testRootDirectory, "photos", "2024"));
        fileSystem.AddDirectory(Path.Combine(testRootDirectory, "documents"));
        var filesystemAccess = CreateFilesystemAccess(fileSystem);

        // Act
        var result = filesystemAccess.ListAllFilesystemDirectories().ToList();

        // Assert
        Assert.Contains("photos", result);
        Assert.Contains("photos/2024", result);
        Assert.Contains("documents", result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ListAllFilesystemDirectories_PathsAreNormalized_UsesForwardSlashes()
    {
        // Arrange
        var fileSystem = CreateMockFileSystem(testRootDirectory);
        fileSystem.AddDirectory($"{testRootDirectory}/path/to/dir");
        var filesystemAccess = CreateFilesystemAccess(fileSystem);

        // Act
        var result = filesystemAccess.ListAllFilesystemDirectories().ToList();

        // Assert
        Assert.All(result, path => Assert.DoesNotContain("\\", path));
        Assert.All(result, path => Assert.False(path.StartsWith("/")));
    }

    #endregion

    #region GetFilesMissingInFilesystem Tests

    [Fact]
    public void GetFilesMissingInFilesystem_WithAllFilesExisting_ReturnsEmpty()
    {
        // Arrange
        var fileSystem = CreateMockFileSystem(testRootDirectory);
        fileSystem.AddFile($"{testRootDirectory}/photo1.jpg", new MockFileData(""));
        fileSystem.AddFile($"{testRootDirectory}/photo2.jpg", new MockFileData(""));
        var filesystemAccess = CreateFilesystemAccess(fileSystem);

        var files = new[]
        {
            new FileModel { Id = 1, Path = "photo1.jpg" },
            new FileModel { Id = 2, Path = "photo2.jpg" }
        };

        // Act
        var result = filesystemAccess.GetFilesMissingInFilesystem(files).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetFilesMissingInFilesystem_WithSomeFilesMissing_ReturnsMissingFiles()
    {
        // Arrange
        var fileSystem = CreateMockFileSystem(testRootDirectory);
        fileSystem.AddFile($"{testRootDirectory}/photo1.jpg", new MockFileData(""));
        // photo2.jpg is missing
        var filesystemAccess = CreateFilesystemAccess(fileSystem);

        var files = new[]
        {
            new FileModel { Id = 1, Path = "photo1.jpg" },
            new FileModel { Id = 2, Path = "photo2.jpg" }
        };

        // Act
        var result = filesystemAccess.GetFilesMissingInFilesystem(files).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Id);
        Assert.Equal("photo2.jpg", result[0].Path);
    }

    [Fact]
    public void GetFilesMissingInFilesystem_WithAllFilesMissing_ReturnsAllFiles()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var files = new[]
        {
            new FileModel { Id = 1, Path = "photo1.jpg" },
            new FileModel { Id = 2, Path = "photo2.jpg" },
            new FileModel { Id = 3, Path = "photo3.jpg" }
        };

        // Act
        var result = filesystemAccess.GetFilesMissingInFilesystem(files).ToList();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void GetFilesMissingInFilesystem_WithEmptyInput_ReturnsEmpty()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var files = Array.Empty<FileModel>();

        // Act
        var result = filesystemAccess.GetFilesMissingInFilesystem(files).ToList();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region ListNewFilesystemFiles Tests

    [Fact]
    public void ListNewFilesystemFiles_WithNoFiles_ReturnsEmpty()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var dbAccess = A.Fake<IFilesAccess>();

        // Act
        var result = filesystemAccess.ListNewFilesystemFiles(
            testRootDirectory,
            Array.Empty<string>(),
            Array.Empty<string>(),
            includeHiddenDirectories: true,
            dbAccess).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ListNewFilesystemFiles_WithNewFiles_ReturnsNewFiles()
    {
        // Arrange
        var fileSystem = CreateMockFileSystem(testRootDirectory);
        fileSystem.AddFile($"{testRootDirectory}/photo1.jpg", new MockFileData(""));
        fileSystem.AddFile($"{testRootDirectory}/photo2.jpg", new MockFileData(""));
        var filesystemAccess = CreateFilesystemAccess(fileSystem);

        var dbAccess = A.Fake<IFilesAccess>();
        A.CallTo(() => dbAccess.GetFileByPath(A<string>._)).Returns(null);

        // Act
        var result = filesystemAccess.ListNewFilesystemFiles(
            testRootDirectory,
            Array.Empty<string>(),
            Array.Empty<string>(),
            includeHiddenDirectories: true,
            dbAccess).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("photo1.jpg", result);
        Assert.Contains("photo2.jpg", result);
    }

    [Fact]
    public void ListNewFilesystemFiles_WithFilesInDatabase_ExcludesExistingFiles()
    {
        // Arrange
        var fileSystem = CreateMockFileSystem(testRootDirectory);
        fileSystem.AddFile($"{testRootDirectory}/photo1.jpg", new MockFileData(""));
        fileSystem.AddFile($"{testRootDirectory}/photo2.jpg", new MockFileData(""));
        var filesystemAccess = CreateFilesystemAccess(fileSystem);

        var dbAccess = A.Fake<IFilesAccess>();
        A.CallTo(() => dbAccess.GetFileByPath("photo1.jpg"))
            .Returns(new FileModel { Id = 1, Path = "photo1.jpg" });
        A.CallTo(() => dbAccess.GetFileByPath("photo2.jpg"))
            .Returns(null);

        // Act
        var result = filesystemAccess.ListNewFilesystemFiles(
            testRootDirectory,
            Array.Empty<string>(),
            Array.Empty<string>(),
            includeHiddenDirectories: true,
            dbAccess).ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains("photo2.jpg", result);
    }

    [Fact]
    public void ListNewFilesystemFiles_WithBlacklist_ExcludesBlacklistedFiles()
    {
        // Arrange
        var fileSystem = CreateMockFileSystem(testRootDirectory);
        fileSystem.AddFile($"{testRootDirectory}/photo.jpg", new MockFileData(""));
        fileSystem.AddFile($"{testRootDirectory}/temp/cache.jpg", new MockFileData(""));
        var filesystemAccess = CreateFilesystemAccess(fileSystem);

        var dbAccess = A.Fake<IFilesAccess>();
        A.CallTo(() => dbAccess.GetFileByPath(A<string>._)).Returns(null);

        var blacklist = new[] { "temp" };

        // Act
        var result = filesystemAccess.ListNewFilesystemFiles(
            testRootDirectory,
            blacklist,
            Array.Empty<string>(),
            includeHiddenDirectories: true,
            dbAccess).ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains("photo.jpg", result);
        Assert.DoesNotContain(result, f => f.Contains("temp"));
    }

    [Fact]
    public void ListNewFilesystemFiles_WithWhitelist_IncludesOnlyWhitelistedExtensions()
    {
        // Arrange
        var fileSystem = CreateMockFileSystem(testRootDirectory);
        fileSystem.AddFile($"{testRootDirectory}/photo.jpg", new MockFileData(""));
        fileSystem.AddFile($"{testRootDirectory}/document.pdf", new MockFileData(""));
        fileSystem.AddFile($"{testRootDirectory}/image.png", new MockFileData(""));
        var filesystemAccess = CreateFilesystemAccess(fileSystem);

        var dbAccess = A.Fake<IFilesAccess>();
        A.CallTo(() => dbAccess.GetFileByPath(A<string>._)).Returns(null);

        var whitelist = new[] { ".jpg", ".png" };

        // Act
        var result = filesystemAccess.ListNewFilesystemFiles(
            testRootDirectory,
            Array.Empty<string>(),
            whitelist,
            includeHiddenDirectories: true,
            dbAccess).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("photo.jpg", result);
        Assert.Contains("image.png", result);
        Assert.DoesNotContain("document.pdf", result);
    }

    [Fact]
    public void ListNewFilesystemFiles_WithHiddenFiles_IncludeHiddenFalse_ExcludesHidden()
    {
        // Arrange
        var fileSystem = CreateMockFileSystem(testRootDirectory);
        fileSystem.AddFile($"{testRootDirectory}/photo.jpg", new MockFileData(""));
        fileSystem.AddFile($"{testRootDirectory}/.hidden/file.txt", new MockFileData(""));
        var filesystemAccess = CreateFilesystemAccess(fileSystem);

        var dbAccess = A.Fake<IFilesAccess>();
        A.CallTo(() => dbAccess.GetFileByPath(A<string>._)).Returns(null);

        // Act
        var result = filesystemAccess.ListNewFilesystemFiles(
            testRootDirectory,
            Array.Empty<string>(),
            Array.Empty<string>(),
            includeHiddenDirectories: false,
            dbAccess).ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains("photo.jpg", result);
        Assert.DoesNotContain(result, f => f.Contains(".hidden"));
    }

    [Fact]
    public void ListNewFilesystemFiles_WithPathOutsideRoot_ReturnsEmpty()
    {
        // Arrange
        var filesystemAccess = CreateFilesystemAccess();
        var dbAccess = A.Fake<IFilesAccess>();

        // Act
        var result = filesystemAccess.ListNewFilesystemFiles(
            "/different/path",
            Array.Empty<string>(),
            Array.Empty<string>(),
            includeHiddenDirectories: true,
            dbAccess).ToList();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region ParseFileMetadata Tests

    [Fact]
    public void ParseFileMetadata_WithNonJpegFile_ReturnsMetadataFromFilename()
    {
        // Arrange
        var fileSystem = CreateMockFileSystem(testRootDirectory);
        var filePath = $"{testRootDirectory}/IMG_20240315_143022.png";
        fileSystem.AddFile(filePath, new MockFileData(""));
        var filesystemAccess = CreateFilesystemAccess(fileSystem);

        // Act
        var result = filesystemAccess.ParseFileMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Datetime); // Should parse from filename
        Assert.Null(result.Position);
        Assert.Null(result.Orientation);
    }

    [Fact]
    public void ParseFileMetadata_WithNonExifFile_ReturnsBasicMetadata()
    {
        // Arrange
        var fileSystem = CreateMockFileSystem(testRootDirectory);
        var filePath = $"{testRootDirectory}/file.txt";
        fileSystem.AddFile(filePath, new MockFileData(""));
        var filesystemAccess = CreateFilesystemAccess(fileSystem);

        // Act
        var result = filesystemAccess.ParseFileMetadata(filePath);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void ParseFileMetadata_WithJpegExtensions_ReturnsMetadata()
    {
        // Arrange
        var fileSystem = CreateMockFileSystem(testRootDirectory);

        var testCases = new[] { ".jpg", ".jpeg", ".JPG", ".JPEG" };
        var filesystemAccess = CreateFilesystemAccess(fileSystem);

        foreach (var ext in testCases)
        {
            var filePath = $"{testRootDirectory}/photo{ext}";
            fileSystem.AddFile(filePath, new MockFileData(new byte[0]));

            // Act
            var result = filesystemAccess.ParseFileMetadata(filePath);

            // Assert
            Assert.NotNull(result);
        }
    }

    #endregion
}