using FakeItEasy;
using FileDB.Export.SearchResult;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using System.IO.Abstractions;
using Xunit;

namespace FileDBTests.Export.SearchResult;

public class M3uExporterTests
{
    private const string DestFile = "/output/Export.m3u";

    private readonly IFileSystem fakeFileSystem = A.Fake<IFileSystem>();
    private string? capturedContent;

    public M3uExporterTests()
    {
        A.CallTo(() => fakeFileSystem.File.WriteAllText(A<string>._, A<string>._))
            .Invokes(call => capturedContent = (string)call.Arguments[1]!);
    }

    private M3uExporter CreateExporter() => new(fakeFileSystem);

    private static ExportedFile MakeFile(
        string originalPath = "photo.jpg",
        string exportedPath = "Files/1.jpg",
        FileType fileType = FileType.Picture) =>
        new(1, exportedPath, originalPath, fileType, null, null, null, null, [], [], []);

    private static M3uExportData MakeData(
        string name = "My Collection",
        params ExportedFile[] files) =>
        new(name, [.. files]);

    private void Export(M3uExportData data) => CreateExporter().Export(data, DestFile);

    // --- Output file ---

    [Fact]
    public void Export_WritesToDestinationFilePath()
    {
        Export(MakeData());
        A.CallTo(() => fakeFileSystem.File.WriteAllText(DestFile, A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    // --- M3U header ---

    [Fact]
    public void Export_ContentStartsWithExtM3uHeader()
    {
        Export(MakeData());
        Assert.StartsWith("#EXTM3U", capturedContent);
    }

    [Fact]
    public void Export_ContentContainsPlaylistName()
    {
        Export(MakeData(name: "Summer 2020"));
        Assert.Contains("#PLAYLIST:Summer 2020", capturedContent);
    }

    // --- File entries ---

    [Fact]
    public void Export_NoFiles_ContentHasOnlyHeader()
    {
        Export(MakeData());
        Assert.DoesNotContain("#EXTINF:", capturedContent);
    }

    [Fact]
    public void Export_SingleFile_ContainsExtinfWithOriginalPath()
    {
        Export(MakeData(files: [MakeFile(originalPath: "photo.jpg")]));
        Assert.Contains("#EXTINF:10,photo.jpg", capturedContent);
    }

    [Fact]
    public void Export_SingleFile_ContainsExportedPath()
    {
        Export(MakeData(files: [MakeFile(exportedPath: "Files/1.jpg")]));
        Assert.Contains("Files/1.jpg", capturedContent);
    }

    [Fact]
    public void Export_TwoFiles_BothHaveExtinfEntries()
    {
        var file2 = new ExportedFile(2, "Files/2.jpg", "other.jpg", FileType.Picture, null, null, null, null, [], [], []);
        Export(MakeData(files: [MakeFile(), file2]));
        Assert.Contains("#EXTINF:10,photo.jpg", capturedContent);
        Assert.Contains("#EXTINF:10,other.jpg", capturedContent);
    }

    [Fact]
    public void Export_TwoFiles_BothExportedPathsPresent()
    {
        var file2 = new ExportedFile(2, "Files/2.jpg", "other.jpg", FileType.Picture, null, null, null, null, [], [], []);
        Export(MakeData(files: [MakeFile(exportedPath: "Files/1.jpg"), file2]));
        Assert.Contains("Files/1.jpg", capturedContent);
        Assert.Contains("Files/2.jpg", capturedContent);
    }

    // --- Line endings ---

    [Fact]
    public void Export_UsesWindowsLineEndings()
    {
        Export(MakeData(files: [MakeFile()]));
        Assert.Contains("\r\n", capturedContent);
    }

    // --- All file types are included ---

    [Fact]
    public void Export_MovieFile_IsIncluded()
    {
        Export(MakeData(files: [MakeFile(fileType: FileType.Movie)]));
        Assert.Contains("#EXTINF:", capturedContent);
    }

    [Fact]
    public void Export_DocumentFile_IsIncluded()
    {
        Export(MakeData(files: [MakeFile(fileType: FileType.Document)]));
        Assert.Contains("#EXTINF:", capturedContent);
    }
}
