using FakeItEasy;
using FileDB.Export.SearchResult;
using FileDB.Model;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using System.IO.Abstractions;
using Xunit;

namespace FileDBTests.Export.SearchResult;

public class HtmlExporterTests
{
    private const string DestDir = "/output";
    private const string AbsPath = "/collection/photo.jpg";
    private const string LocationLink = "https://maps/LAT,LON";

    private readonly IFileSystem fakeFileSystem = A.Fake<IFileSystem>();
    private readonly IFilesystemAccessProvider fakeFilesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
    private readonly IConfigProvider fakeConfigProvider = A.Fake<IConfigProvider>();

    private string? capturedHtml;

    public HtmlExporterTests()
    {
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess.ToAbsolutePath(A<string>._)).Returns(AbsPath);
        A.CallTo(() => fakeFileSystem.Directory.Exists(DestDir)).Returns(true);
        A.CallTo(() => fakeFileSystem.File.Exists(AbsPath)).Returns(true);
        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder().Build());

        A.CallTo(() => fakeFileSystem.File.WriteAllText(A<string>._, A<string>._))
            .Invokes(call => capturedHtml = (string)call.Arguments[1]!);
    }

    private HtmlExporter CreateExporter() =>
        new(fakeFileSystem, fakeFilesystemAccessProvider, fakeConfigProvider);

    private static ExportedFile MakeFile(
        string originalPath = "photo.jpg",
        string exportedPath = "photo.jpg",
        string? description = null,
        string? datetime = null,
        string? position = null,
        List<int>? personIds = null,
        List<int>? locationIds = null,
        List<int>? tagIds = null) =>
        new(1, exportedPath, originalPath, FileType.Picture, description, datetime, position, null,
            personIds ?? [], locationIds ?? [], tagIds ?? [], []);

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

    private void Export(RichExportData data) => CreateExporter().Export(data, DestDir);

    // --- CreateExportedFileDatetime (static) ---

    [Fact]
    public void CreateExportedFileDatetime_InvalidDatetime_ReturnsNull()
    {
        Assert.Null(HtmlExporter.CreateExportedFileDatetime("not-a-date"));
    }

    [Fact]
    public void CreateExportedFileDatetime_DateOnlyFormat_ReturnsInputString()
    {
        Assert.Equal("2020-06-15", HtmlExporter.CreateExportedFileDatetime("2020-06-15"));
    }

    [Fact]
    public void CreateExportedFileDatetime_DateTimeFormat_ReturnsFormattedDateTime()
    {
        Assert.Equal("2020-06-15 10:30", HtmlExporter.CreateExportedFileDatetime("2020-06-15T10:30:00"));
    }

    // --- Export: directory creation ---

    [Fact]
    public void Export_DirectoryDoesNotExist_CreatesDirectory()
    {
        A.CallTo(() => fakeFileSystem.Directory.Exists(DestDir)).Returns(false);
        Export(MakeData());
        A.CallTo(() => fakeFileSystem.Directory.CreateDirectory(DestDir)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_DirectoryExists_DoesNotCreateDirectory()
    {
        A.CallTo(() => fakeFileSystem.Directory.Exists(DestDir)).Returns(true);
        Export(MakeData());
        A.CallTo(() => fakeFileSystem.Directory.CreateDirectory(DestDir)).MustNotHaveHappened();
    }

    // --- Export: index.html ---

    [Fact]
    public void Export_WritesIndexHtml()
    {
        Export(MakeData());
        A.CallTo(() => fakeFileSystem.File.WriteAllText(
            A<string>.That.EndsWith("index.html"), A<string>._))
            .MustHaveHappenedOnceExactly();
    }

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
    public void Export_HtmlContainsComment()
    {
        Export(MakeData());
        Assert.Contains("<!-- FileDB 1.0", capturedHtml);
    }

    [Fact]
    public void Export_HtmlContainsOverlayScaleCssVariable()
    {
        Export(MakeData());
        Assert.Contains("--overlay-scale", capturedHtml);
    }

    [Fact]
    public void Export_HtmlContainsMediaQueries()
    {
        Export(MakeData());
        Assert.Contains("controls", capturedHtml);
        Assert.Contains("btn-play", capturedHtml);
    }

    [Fact]
    public void Export_HtmlContainsViewportMetaTag()
    {
        Export(MakeData());
        Assert.Contains("viewport", capturedHtml);
    }

    [Fact]
    public void Export_HtmlContainsScaleIndicator()
    {
        Export(MakeData());
        Assert.Contains("scale-indicator", capturedHtml);
    }

    [Fact]
    public void Export_HtmlContainsKeyboardScaleHandler()
    {
        Export(MakeData());
        Assert.Contains("adjustOverlayScale", capturedHtml);
        Assert.Contains("ArrowUp", capturedHtml);
        Assert.Contains("ArrowDown", capturedHtml);
    }

    [Fact]
    public void Export_HtmlContainsSessionStorageForScale()
    {
        Export(MakeData());
        Assert.Contains("sessionStorage", capturedHtml);
        Assert.Contains("overlayScale", capturedHtml);
    }

    [Fact]
    public void Export_HtmlContainsVerticalSwipeSupport()
    {
        Export(MakeData());
        Assert.Contains("Math.abs(dy)", capturedHtml);
        Assert.Contains("0.5, Math.min(2.0", capturedHtml);
    }

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
        Assert.Contains("btn-first", capturedHtml);
        Assert.Contains("btn-prev", capturedHtml);
        Assert.Contains("btn-play", capturedHtml);
        Assert.Contains("btn-next", capturedHtml);
        Assert.Contains("btn-last", capturedHtml);
        Assert.Contains("btn-random", capturedHtml);
        Assert.Contains("btn-repeat", capturedHtml);
    }

    [Fact]
    public void Export_HtmlControlsContainKeyBindingTooltips()
    {
        Export(MakeData());
        Assert.Contains("title=\"First (Home)\"", capturedHtml);
        Assert.Contains("title=\"Previous (PageUp, ArrowLeft)\"", capturedHtml);
        Assert.Contains("title=\"Play / Pause (Space)\"", capturedHtml);
        Assert.Contains("title=\"Next (PageDown, ArrowRight)\"", capturedHtml);
        Assert.Contains("title=\"Last (End)\"", capturedHtml);
        Assert.Contains("title=\"Random (R)\"", capturedHtml);
        Assert.Contains("title=\"Repeat (T)\"", capturedHtml);
    }

    [Fact]
    public void Export_HtmlControlsUseScaledFixedButtonSizeAndAutoHide()
    {
        Export(MakeData());
        Assert.Contains("left: 50%; transform: translateX(-50%)", capturedHtml);
        Assert.Contains("font-size: calc(1em * var(--overlay-scale))", capturedHtml);
        Assert.Contains("width: 2.2em; height: 2.2em", capturedHtml);
        Assert.Contains("scheduleHideControls(isPlaying ? 250 : 800)", capturedHtml);
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
    public void Export_NonExistingFile_IsSkippedAndNotCopied()
    {
        A.CallTo(() => fakeFileSystem.File.Exists(AbsPath)).Returns(false);
        Export(MakeData(MakeFile()));
        A.CallTo(() => fakeFileSystem.File.Copy(A<string>._, A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Export_ExistingFile_IsCopiedToDestination()
    {
        Export(MakeData(MakeFile()));
        A.CallTo(() => fakeFileSystem.File.Copy(AbsPath, A<string>.That.Contains("photo.jpg"))).MustHaveHappenedOnceExactly();
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
        var file2 = new ExportedFile(2, "photo2.jpg", "photo2.jpg", FileType.Picture, null, null, null, null, [], [], [], []);
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess.ToAbsolutePath("photo2.jpg")).Returns("/collection/photo2.jpg");
        A.CallTo(() => fakeFileSystem.File.Exists("/collection/photo2.jpg")).Returns(true);
        var data = new RichExportData("Name", "1.0", DateTime.Now, "https://example.com", [MakeFile(), file2], [], [], []);
        Export(data);
        // Both filenames appear in the slides JS array
        Assert.Contains("photo.jpg", capturedHtml);
        Assert.Contains("photo2.jpg", capturedHtml);
    }
}
