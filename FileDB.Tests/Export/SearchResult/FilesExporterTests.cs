using FakeItEasy;
using FileDB.Export.SearchResult;
using FileDB.Model;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using System.IO.Abstractions;
using Xunit;

namespace FileDB.Tests.Export.SearchResult;

public class FilesExporterTests
{
    private const string DestDir = "/output";
    private const string AbsPath = "/collection/photo.jpg";

    private readonly IFileSystem fakeFileSystem = A.Fake<IFileSystem>();
    private readonly IFilesystemAccessProvider fakeFilesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();

    public FilesExporterTests()
    {
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess.ToAbsolutePath(A<string>._)).Returns(AbsPath);
        A.CallTo(() => fakeFileSystem.Directory.Exists(A<string>._)).Returns(true);
    }

    private FilesExporter CreateExporter() => new(fakeFileSystem, fakeFilesystemAccessProvider);

    private static ExportedFile MakeFile(
        string originalPath = "photo.jpg",
        string exportedPath = "Files/1.jpg",
        FileType fileType = FileType.Picture) =>
        new(1, exportedPath, originalPath, fileType, null, null, null, null, [], [], [], []);

    private static List<ExportedFile> MakeFilesList(params ExportedFile[] files) => [.. files];

    private void Export(List<ExportedFile> data) => CreateExporter().Export(data, DestDir);

    // --- Empty export ---

    [Fact]
    public void Export_NoFiles_NothingCopied()
    {
        Export(MakeFilesList());
        A.CallTo(() => fakeFileSystem.File.Copy(A<string>._, A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Export_NoFiles_NoDirectoryCreated()
    {
        Export(MakeFilesList());
        A.CallTo(() => fakeFileSystem.Directory.CreateDirectory(A<string>._)).MustNotHaveHappened();
    }

    // --- File copying ---

    [Fact]
    public void Export_SingleFile_CopiesFromAbsoluteSourcePath()
    {
        Export(MakeFilesList(MakeFile()));
        A.CallTo(() => fakeFileSystem.File.Copy(AbsPath, A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_SingleFile_CopiesIntoDestinationDirectory()
    {
        Export(MakeFilesList(MakeFile(exportedPath: "Files/1.jpg")));
        var expectedDest = Path.Combine(DestDir, "Files/1.jpg");
        A.CallTo(() => fakeFileSystem.File.Copy(A<string>._, expectedDest)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_TwoFiles_CopiesEachFile()
    {
        var file2 = new ExportedFile(2, "Files/2.jpg", "photo2.jpg", FileType.Picture, null, null, null, null, [], [], [], []);
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess.ToAbsolutePath("photo2.jpg")).Returns("/collection/photo2.jpg");
        Export(MakeFilesList(MakeFile(), file2));
        A.CallTo(() => fakeFileSystem.File.Copy(A<string>._, A<string>._)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public void Export_ResolvesAbsolutePathForEachFile()
    {
        Export(MakeFilesList(MakeFile(originalPath: "sub/photo.jpg")));
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess.ToAbsolutePath("sub/photo.jpg"))
            .MustHaveHappenedOnceExactly();
    }

    // --- Directory creation ---

    [Fact]
    public void Export_DestinationDirectoryExists_DoesNotCreateDirectory()
    {
        A.CallTo(() => fakeFileSystem.Directory.Exists(A<string>._)).Returns(true);
        Export(MakeFilesList(MakeFile()));
        A.CallTo(() => fakeFileSystem.Directory.CreateDirectory(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Export_DestinationDirectoryMissing_CreatesDirectory()
    {
        A.CallTo(() => fakeFileSystem.Directory.Exists(A<string>._)).Returns(false);
        Export(MakeFilesList(MakeFile(exportedPath: "Files/1.jpg")));
        var expectedDir = Path.GetDirectoryName(Path.Combine(DestDir, "Files/1.jpg"))!;
        A.CallTo(() => fakeFileSystem.Directory.CreateDirectory(expectedDir)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_TwoFilesInSameDirectory_ChecksDirectoryExistenceForEach()
    {
        var file2 = new ExportedFile(2, "Files/2.jpg", "photo2.jpg", FileType.Picture, null, null, null, null, [], [], [], []);
        Export(MakeFilesList(MakeFile(exportedPath: "Files/1.jpg"), file2));
        A.CallTo(() => fakeFileSystem.Directory.Exists(A<string>._)).MustHaveHappenedTwiceExactly();
    }

    // --- Non-picture file types are also exported ---

    [Fact]
    public void Export_DocumentFile_IsCopied()
    {
        Export(MakeFilesList(MakeFile(fileType: FileType.Document)));
        A.CallTo(() => fakeFileSystem.File.Copy(AbsPath, A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_MovieFile_IsCopied()
    {
        Export(MakeFilesList(MakeFile(fileType: FileType.Movie)));
        A.CallTo(() => fakeFileSystem.File.Copy(AbsPath, A<string>._)).MustHaveHappenedOnceExactly();
    }
}
