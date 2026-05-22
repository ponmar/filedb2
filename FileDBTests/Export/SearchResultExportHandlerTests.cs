using FakeItEasy;
using FileDB.Export;
using FileDB.Model;
using FileDBInterface.Model;
using QuestPDF.Infrastructure;
using System.IO.Abstractions;
using Xunit;

namespace FileDBTests.Export;

public class SearchResultExportHandlerTests
{
    // Minimal valid 1×1 white PNG for PDF export tests
    private static readonly byte[] MinimalPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAC0lEQVQI12NgAAIABQ" +
        "AABjkB6QAAAABJRU5ErkJggg==");

    private const string DestDir = "/output";
    private const string AbsPath = "/collection/photo.jpg";

    private readonly IFileSystem fakeFileSystem = A.Fake<IFileSystem>();
    private readonly IFilesystemAccessProvider fakeFilesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
    private readonly IDatabaseAccessProvider fakeDbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IConfigProvider fakeConfigProvider = A.Fake<IConfigProvider>();

    private readonly List<(string Path, string Content)> writtenTextFiles = [];
    private readonly List<(string Path, byte[] Bytes)> writtenBinaryFiles = [];
    private readonly List<(string Source, string Dest)> copiedFiles = [];

    public SearchResultExportHandlerTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder().Build());
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess.ToAbsolutePath(A<string>._)).Returns(AbsPath);
        A.CallTo(() => fakeFileSystem.Directory.Exists(A<string>._)).Returns(true);
        A.CallTo(() => fakeFileSystem.File.ReadAllBytes(AbsPath)).Returns(MinimalPng);

        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetLocationsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetTagsFromFile(A<int>._)).Returns([]);

        A.CallTo(() => fakeFileSystem.File.WriteAllText(A<string>._, A<string>._))
            .Invokes(call => writtenTextFiles.Add(((string)call.Arguments[0]!, (string)call.Arguments[1]!)));
        A.CallTo(() => fakeFileSystem.File.WriteAllBytes(A<string>._, A<byte[]>._))
            .Invokes(call => writtenBinaryFiles.Add(((string)call.Arguments[0]!, (byte[])call.Arguments[1]!)));
        A.CallTo(() => fakeFileSystem.File.Copy(A<string>._, A<string>._))
            .Invokes(call => copiedFiles.Add(((string)call.Arguments[0]!, (string)call.Arguments[1]!)));
    }

    private SearchResultExportHandler CreateHandler() =>
        new(fakeDbAccessProvider, fakeFilesystemAccessProvider, fakeFileSystem, fakeConfigProvider);

    private static FileModel MakeFileModel(int id = 1, string path = "photo.jpg") =>
        new() { Id = id, Path = path };

    private void Export(
        List<FileModel>? files = null,
        List<SearchResultExportType>? exportTypes = null,
        CancellationToken cancellationToken = default) =>
        CreateHandler().Export(DestDir, "Test Export", files ?? [], exportTypes ?? [], cancellationToken);

    // --- Export type dispatching ---

    [Fact]
    public void Export_NoExportTypes_NothingWritten()
    {
        Export(exportTypes: []);
        Assert.Empty(writtenTextFiles);
        Assert.Empty(writtenBinaryFiles);
        Assert.Empty(copiedFiles);
    }

    [Fact]
    public void Export_JsonType_WritesExportJsonFile()
    {
        Export(exportTypes: [SearchResultExportType.Json]);
        Assert.Single(writtenTextFiles);
        Assert.EndsWith("Export.json", writtenTextFiles[0].Path);
    }

    [Fact]
    public void Export_M3uType_WritesExportM3uFile()
    {
        Export(exportTypes: [SearchResultExportType.M3u]);
        Assert.Single(writtenTextFiles);
        Assert.EndsWith("Export.m3u", writtenTextFiles[0].Path);
    }

    [Fact]
    public void Export_HtmlType_WritesIndexHtmlFile()
    {
        Export(exportTypes: [SearchResultExportType.Html]);
        Assert.Single(writtenTextFiles);
        Assert.EndsWith("index.html", writtenTextFiles[0].Path);
    }

    [Fact]
    public void Export_SelfContainedHtmlType_WritesExportHtmlFile()
    {
        Export(exportTypes: [SearchResultExportType.SelfContainedHtml]);
        Assert.Single(writtenTextFiles);
        Assert.EndsWith("Export.html", writtenTextFiles[0].Path);
    }

    [Fact]
    public void Export_PdfType_WritesExportPdfFile()
    {
        Export(exportTypes: [SearchResultExportType.Pdf]);
        Assert.Single(writtenBinaryFiles);
        Assert.EndsWith("Export.pdf", writtenBinaryFiles[0].Path);
    }

    [Fact]
    public void Export_FilesType_WithFile_CopiesFile()
    {
        Export(files: [MakeFileModel()], exportTypes: [SearchResultExportType.Files]);
        Assert.Single(copiedFiles);
    }

    [Fact]
    public void Export_MultipleTypes_AllOutputsWritten()
    {
        Export(exportTypes: [SearchResultExportType.Json, SearchResultExportType.M3u, SearchResultExportType.SelfContainedHtml]);
        Assert.Equal(3, writtenTextFiles.Count);
    }

    // --- Exported file path assembly ---

    [Fact]
    public void Export_SingleFile_ExportedPathContains1WithExtension()
    {
        Export(files: [MakeFileModel(path: "photo.jpg")], exportTypes: [SearchResultExportType.Json]);
        var json = writtenTextFiles[0].Content;
        Assert.Contains("1.jpg", json);
    }

    [Fact]
    public void Export_TwoFiles_ExportedPathsIncrementIndex()
    {
        Export(files: [MakeFileModel(1, "photo.jpg"), MakeFileModel(2, "video.mp4")],
               exportTypes: [SearchResultExportType.Json]);
        var json = writtenTextFiles[0].Content;
        Assert.Contains("Files", json);
        Assert.Contains("1.jpg", json);
        Assert.Contains("2.mp4", json);
    }

    // --- DB data assembly ---

    [Fact]
    public void Export_PersonsQueriedFromDbForEachFile()
    {
        var files = new List<FileModel> { MakeFileModel(1), MakeFileModel(2) };
        Export(files: files, exportTypes: [SearchResultExportType.Json]);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(1)).MustHaveHappenedOnceExactly();
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(2)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_LocationsQueriedFromDbForEachFile()
    {
        var files = new List<FileModel> { MakeFileModel(1), MakeFileModel(2) };
        Export(files: files, exportTypes: [SearchResultExportType.Json]);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetLocationsFromFile(1)).MustHaveHappenedOnceExactly();
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetLocationsFromFile(2)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_TagsQueriedFromDbForEachFile()
    {
        var files = new List<FileModel> { MakeFileModel(1), MakeFileModel(2) };
        Export(files: files, exportTypes: [SearchResultExportType.Json]);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetTagsFromFile(1)).MustHaveHappenedOnceExactly();
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetTagsFromFile(2)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_PersonReturnedFromDb_IncludedInJson()
    {
        var person = new PersonModel { Id = 10, ShortName = "Alice", FullName = "Alice Smith" };
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(1)).Returns([person]);
        Export(files: [MakeFileModel(1)], exportTypes: [SearchResultExportType.Json]);
        Assert.Contains("Alice Smith", writtenTextFiles[0].Content);
    }

    [Fact]
    public void Export_SamePersonFromTwoFiles_DeduplicatedInJson()
    {
        var person = new PersonModel { Id = 10, ShortName = "Alice", FullName = "Alice Smith" };
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([person]);
        Export(files: [MakeFileModel(1), MakeFileModel(2)], exportTypes: [SearchResultExportType.Json]);
        var json = writtenTextFiles[0].Content;
        // "Alice Smith" appears exactly once in the Persons array (deduplicated)
        Assert.Equal(1, CountOccurrences(json, "Alice Smith"));
    }

    [Fact]
    public void Export_LocationReturnedFromDb_IncludedInJson()
    {
        var location = new LocationModel { Id = 20, Name = "Paris" };
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetLocationsFromFile(1)).Returns([location]);
        Export(files: [MakeFileModel(1)], exportTypes: [SearchResultExportType.Json]);
        Assert.Contains("Paris", writtenTextFiles[0].Content);
    }

    [Fact]
    public void Export_TagReturnedFromDb_IncludedInJson()
    {
        var tag = new TagModel { Id = 30, Name = "Nature" };
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetTagsFromFile(1)).Returns([tag]);
        Export(files: [MakeFileModel(1)], exportTypes: [SearchResultExportType.Json]);
        Assert.Contains("Nature", writtenTextFiles[0].Content);
    }

    // --- Cancellation ---

    [Fact]
    public void Export_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Assert.Throws<OperationCanceledException>(() =>
            Export(files: [MakeFileModel()], exportTypes: [SearchResultExportType.Json], cancellationToken: cts.Token));
    }

    private static int CountOccurrences(string text, string value) =>
        (text.Length - text.Replace(value, string.Empty).Length) / value.Length;
}
