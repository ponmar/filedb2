using FakeItEasy;
using FileDB.Export.SearchResult;
using FileDB.Model;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using System.IO.Abstractions;
using Xunit;

namespace FileDBTests.Export.SearchResult;

public class SelfContainedHtmlExporterTests
{
    private const string DestFile = "/output/export.html";
    private const string AbsPath = "/collection/photo.jpg";
    private const string LocationLink = "https://maps/LAT,LON";

    private readonly IFileSystem fakeFileSystem = A.Fake<IFileSystem>();
    private readonly IFilesystemAccessProvider fakeFilesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
    private readonly IConfigProvider fakeConfigProvider = A.Fake<IConfigProvider>();

    private string? capturedHtml;

    public SelfContainedHtmlExporterTests()
    {
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess.ToAbsolutePath(A<string>._)).Returns(AbsPath);
        A.CallTo(() => fakeFileSystem.File.Exists(AbsPath)).Returns(true);
        A.CallTo(() => fakeFileSystem.File.ReadAllBytes(AbsPath)).Returns([0xFF, 0xD8, 0xFF]); // minimal JPEG bytes
        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder().Build());

        A.CallTo(() => fakeFileSystem.File.WriteAllText(A<string>._, A<string>._))
            .Invokes(call => capturedHtml = (string)call.Arguments[1]!);
    }

    private SelfContainedHtmlExporter CreateExporter() =>
        new(fakeFileSystem, fakeFilesystemAccessProvider, fakeConfigProvider);

    private static ExportedFile MakeFile(
        string originalPath = "photo.jpg",
        FileType fileType = FileType.Picture,
        string? description = null,
        string? datetime = null,
        string? position = null,
        List<int>? personIds = null,
        List<int>? locationIds = null,
        List<int>? tagIds = null) =>
        new(1, originalPath, originalPath, fileType, description, datetime, position, null,
            personIds ?? [], locationIds ?? [], tagIds ?? []);

    private static RichExportData MakeData(
        ExportedFile? file = null,
        List<PersonModel>? persons = null,
        List<LocationModel>? locations = null,
        List<TagModel>? tags = null) =>
        new("My Collection", "1.0", new DateTime(2024, 1, 15, 10, 0, 0),
            "https://github.com/ponmar/filedb2",
            file is null ? [] : [file],
            persons ?? [],
            locations ?? [],
            tags ?? []);

    private void Export(RichExportData data) => CreateExporter().Export(data, DestFile);

    // --- Export: writes to specified file path ---

    [Fact]
    public void Export_WritesHtmlToDestinationFilePath()
    {
        Export(MakeData());
        A.CallTo(() => fakeFileSystem.File.WriteAllText(DestFile, A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_NeverCreatesDirectory()
    {
        Export(MakeData());
        A.CallTo(() => fakeFileSystem.Directory.CreateDirectory(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Export_NeverCopiesFiles()
    {
        Export(MakeData(MakeFile()));
        A.CallTo(() => fakeFileSystem.File.Copy(A<string>._, A<string>._)).MustNotHaveHappened();
    }

    // --- Export: HTML structure ---

    [Fact]
    public void Export_HtmlContainsCollectionName()
    {
        Export(MakeData());
        Assert.Contains("My Collection", capturedHtml);
    }

    [Fact]
    public void Export_HtmlContainsApplicationUrl()
    {
        Export(MakeData());
        Assert.Contains("https://github.com/ponmar/filedb2", capturedHtml);
    }

    [Fact]
    public void Export_HtmlFooterIsComment()
    {
        Export(MakeData());
        Assert.Contains("<!-- FileDB 1.0", capturedHtml);
        Assert.DoesNotContain("id=\"footer\"", capturedHtml);
    }

    // --- Export: slideshow structure ---

    [Fact]
    public void Export_HtmlContainsSlidesArray()
    {
        Export(MakeData());
        Assert.Contains("const slides", capturedHtml);
    }

    [Fact]
    public void Export_HtmlContainsShowSlideFunction()
    {
        Export(MakeData());
        Assert.Contains("showSlide", capturedHtml);
    }

    [Fact]
    public void Export_HtmlContainsPrevNextButtons()
    {
        Export(MakeData());
        Assert.DoesNotContain("btn-prev", capturedHtml);
        Assert.DoesNotContain("btn-next", capturedHtml);
    }

    [Fact]
    public void Export_HtmlContainsKeyboardNavigation()
    {
        Export(MakeData());
        Assert.Contains("ArrowRight", capturedHtml);
        Assert.Contains("ArrowLeft", capturedHtml);
        Assert.Contains("PageDown", capturedHtml);
        Assert.Contains("PageUp", capturedHtml);
        Assert.Contains("Home", capturedHtml);
        Assert.Contains("End", capturedHtml);
    }

    [Fact]
    public void Export_HtmlContainsTouchSupport()
    {
        Export(MakeData());
        Assert.Contains("touchstart", capturedHtml);
        Assert.Contains("touchend", capturedHtml);
    }

    // --- Export: file filtering ---

    [Fact]
    public void Export_NonExistingFile_IsSkippedAndNotRead()
    {
        A.CallTo(() => fakeFileSystem.File.Exists(AbsPath)).Returns(false);
        Export(MakeData(MakeFile()));
        A.CallTo(() => fakeFileSystem.File.ReadAllBytes(A<string>._)).MustNotHaveHappened();
    }

    // --- Export: picture embedding ---

    [Fact]
    public void Export_PictureFile_ReadsImageBytes()
    {
        Export(MakeData(MakeFile(fileType: FileType.Picture)));
        A.CallTo(() => fakeFileSystem.File.ReadAllBytes(AbsPath)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_PictureFile_HtmlContainsBase64DataUri()
    {
        Export(MakeData(MakeFile(fileType: FileType.Picture)));
        Assert.Contains("data:image/jpeg;base64,", capturedHtml);
    }

    [Fact]
    public void Export_PngFile_HtmlContainsPngMimeType()
    {
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess.ToAbsolutePath("photo.png")).Returns("/collection/photo.png");
        A.CallTo(() => fakeFileSystem.File.Exists("/collection/photo.png")).Returns(true);
        A.CallTo(() => fakeFileSystem.File.ReadAllBytes("/collection/photo.png")).Returns([0x89, 0x50, 0x4E, 0x47]);
        Export(MakeData(MakeFile(originalPath: "photo.png", fileType: FileType.Picture)));
        Assert.Contains("data:image/png;base64,", capturedHtml);
    }

    // --- Export: non-picture file ---

    [Fact]
    public void Export_NonPictureFile_DoesNotReadImageBytes()
    {
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess.ToAbsolutePath("notes.txt")).Returns("/collection/notes.txt");
        A.CallTo(() => fakeFileSystem.File.Exists("/collection/notes.txt")).Returns(true);
        Export(MakeData(MakeFile(originalPath: "notes.txt", fileType: FileType.Document)));
        A.CallTo(() => fakeFileSystem.File.ReadAllBytes(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Export_NonPictureFile_HtmlContainsFilename()
    {
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess.ToAbsolutePath("notes.txt")).Returns("/collection/notes.txt");
        A.CallTo(() => fakeFileSystem.File.Exists("/collection/notes.txt")).Returns(true);
        Export(MakeData(MakeFile(originalPath: "notes.txt", fileType: FileType.Document)));
        Assert.Contains("notes.txt", capturedHtml);
    }

    // --- Export: metadata in slides JS ---

    [Fact]
    public void Export_FileWithDescription_DescriptionInSlidesJs()
    {
        Export(MakeData(MakeFile(description: "Sunset photo")));
        Assert.Contains("Sunset photo", capturedHtml);
    }

    [Fact]
    public void Export_FileWithDatetime_DateInSlidesJs()
    {
        Export(MakeData(MakeFile(datetime: "2020-06-15")));
        Assert.Contains("2020-06-15", capturedHtml);
    }

    [Fact]
    public void Export_FileWithDatetimeAndDescription_BothPresentWithSeparator()
    {
        Export(MakeData(MakeFile(datetime: "2020-06-15", description: "Sunset")));
        Assert.Contains("2020-06-15", capturedHtml);
        Assert.Contains(": Sunset", capturedHtml);
    }

    [Fact]
    public void Export_FileWithDescriptionContainingNewline_NewlineReplacedWithBrTag()
    {
        Export(MakeData(MakeFile(description: "Line one\nLine two")));
        Assert.Contains("<br>", capturedHtml);
        Assert.DoesNotContain("\nLine two", capturedHtml);
    }

    // --- Export: persons ---

    [Fact]
    public void Export_FileWithPerson_PersonHasSpanWithTitle()
    {
        var person = new PersonModel { Id = 10, ShortName = "Alice", FullName = "Alice Smith" };
        Export(MakeData(MakeFile(personIds: [10]), persons: [person]));
        Assert.Contains("<span title=", capturedHtml);
        Assert.Contains("Alice Smith", capturedHtml);
    }

    [Fact]
    public void Export_FileWithPersonWithDob_TooltipContainsDob()
    {
        var person = new PersonModel { Id = 10, ShortName = "Alice", FullName = "Alice Smith", DateOfBirth = "1990-01-01" };
        Export(MakeData(MakeFile(personIds: [10]), persons: [person]));
        Assert.Contains("1990-01-01", capturedHtml);
    }

    [Fact]
    public void Export_FileWithPersonWithDescription_TooltipContainsDescription()
    {
        var person = new PersonModel { Id = 10, ShortName = "Alice", FullName = "Alice Smith", Description = "Photographer" };
        Export(MakeData(MakeFile(personIds: [10]), persons: [person]));
        Assert.Contains("Photographer", capturedHtml);
    }

    [Fact]
    public void Export_FileWithNoPersonIds_NoPersonEmoji()
    {
        Export(MakeData(MakeFile()));
        Assert.DoesNotContain("&#128578;", capturedHtml);
    }

    // --- Export: locations ---

    [Fact]
    public void Export_FileWithLocationNoPosition_LocationNameInHtml()
    {
        var location = new LocationModel { Id = 20, Name = "Paris" };
        Export(MakeData(MakeFile(locationIds: [20]), locations: [location]));
        Assert.Contains("Paris", capturedHtml);
    }

    [Fact]
    public void Export_FileWithLocationWithPosition_LocationIsLink()
    {
        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder { LocationLink = LocationLink }.Build());
        var location = new LocationModel { Id = 20, Name = "Paris", Position = "48.8 2.3" };
        Export(MakeData(MakeFile(locationIds: [20]), locations: [location]));
        Assert.Contains("<a href=", capturedHtml);
        Assert.Contains("Paris", capturedHtml);
    }

    [Fact]
    public void Export_FileWithNoLocationIds_NoLocationEmoji()
    {
        Export(MakeData(MakeFile()));
        Assert.DoesNotContain("&#127968;", capturedHtml);
    }

    // --- Export: tags ---

    [Fact]
    public void Export_FileWithTag_TagNameInHtml()
    {
        var tag = new TagModel { Id = 30, Name = "Nature" };
        Export(MakeData(MakeFile(tagIds: [30]), tags: [tag]));
        Assert.Contains("Nature", capturedHtml);
    }

    [Fact]
    public void Export_FileWithNoTagIds_NoTagEmoji()
    {
        Export(MakeData(MakeFile()));
        Assert.DoesNotContain("&#128278;", capturedHtml);
    }

    // --- Export: file position ---

    [Fact]
    public void Export_FileWithPositionAndLinkConfig_PositionLink()
    {
        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder { LocationLink = LocationLink }.Build());
        Export(MakeData(MakeFile(position: "59.3 18.1")));
        Assert.Contains("&#x1F6F0;", capturedHtml);
        Assert.Contains("<a href=", capturedHtml);
    }

    [Fact]
    public void Export_FileWithPositionButNoLinkConfig_NoPositionLink()
    {
        Export(MakeData(MakeFile(position: "59.3 18.1")));
        Assert.DoesNotContain("&#x1F6F0;", capturedHtml);
    }

    // --- Export: counter rendered by JS, not baked into slides data ---

    [Fact]
    public void Export_TwoFiles_SlidesJsArrayContainsBothEntries()
    {
        var file2 = new ExportedFile(2, "photo2.jpg", "photo2.jpg", FileType.Picture, null, null, null, null, [], [], []);
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess.ToAbsolutePath("photo2.jpg")).Returns("/collection/photo2.jpg");
        A.CallTo(() => fakeFileSystem.File.Exists("/collection/photo2.jpg")).Returns(true);
        A.CallTo(() => fakeFileSystem.File.ReadAllBytes("/collection/photo2.jpg")).Returns([0xFF, 0xD8, 0xFF]);
        var data = new RichExportData("Name", "1.0", DateTime.Now, "https://example.com", [MakeFile(), file2], [], [], []);
        Export(data);
        // Both filenames appear in the slides JS array
        Assert.Contains("photo.jpg", capturedHtml);
        Assert.Contains("photo2.jpg", capturedHtml);
    }
}

