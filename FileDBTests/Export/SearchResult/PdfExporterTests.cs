using FakeItEasy;
using FileDB.Export.SearchResult;
using FileDB.Model;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using QuestPDF.Infrastructure;
using System.IO.Abstractions;
using Xunit;

namespace FileDBTests.Export.SearchResult;

public class PdfExporterTests
{
    // Minimal valid 1×1 white PNG
    private static readonly byte[] MinimalPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAC0lEQVQI12NgAAIABQ" +
        "AABjkB6QAAAABJRU5ErkJggg==");

    private const string DestFile = "/output/export.pdf";
    private const string AbsPath = "/collection/photo.jpg";
    private const string LocationLink = "https://maps/LAT,LON";

    private readonly IFileSystem fakeFileSystem = A.Fake<IFileSystem>();
    private readonly IFilesystemAccessProvider fakeFilesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
    private readonly IConfigProvider fakeConfigProvider = A.Fake<IConfigProvider>();

    private byte[]? capturedBytes;

    public PdfExporterTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder().Build());
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess.ToAbsolutePath(A<string>._)).Returns(AbsPath);
        A.CallTo(() => fakeFileSystem.File.ReadAllBytes(AbsPath)).Returns(MinimalPng);
        A.CallTo(() => fakeFileSystem.File.WriteAllBytes(A<string>._, A<byte[]>._))
            .Invokes(call => capturedBytes = (byte[])call.Arguments[1]!);
    }

    private PdfExporter CreateExporter(QuestPDF.Helpers.PageSize? pageSize = null) =>
        new(fakeFileSystem, fakeFilesystemAccessProvider, pageSize ?? QuestPDF.Helpers.PageSizes.A4, fakeConfigProvider);

    private static ExportedFile MakePictureFile(
        string? datetime = null,
        string? description = null,
        string? position = null,
        int? orientation = null,
        List<int>? personIds = null,
        List<int>? locationIds = null,
        List<int>? tagIds = null) =>
        new(1, "photo.jpg", "photo.jpg", FileType.Picture, description, datetime, position, orientation,
            personIds ?? [], locationIds ?? [], tagIds ?? []);

    private static ExportedFile MakeNonPictureFile(FileType fileType = FileType.Document) =>
        new(2, "notes.txt", "notes.txt", fileType, null, null, null, null, [], [], []);

    private static SearchResultExport MakeData(
        List<ExportedFile>? files = null,
        List<PersonModel>? persons = null,
        List<LocationModel>? locations = null,
        List<TagModel>? tags = null) =>
        new("My Collection", "1.0", new DateTime(2024, 1, 15, 10, 0, 0), string.Empty,
            files ?? [],
            persons ?? [],
            locations ?? [],
            tags ?? [],
            "https://github.com/ponmar/filedb2");

    private void Export(SearchResultExport data) => CreateExporter().Export(data, DestFile);

    // --- Output file ---

    [Fact]
    public void Export_EmptyData_WritesBytesToDestinationPath()
    {
        Export(MakeData());
        A.CallTo(() => fakeFileSystem.File.WriteAllBytes(DestFile, A<byte[]>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_EmptyData_WritesNonEmptyPdfBytes()
    {
        Export(MakeData());
        Assert.NotNull(capturedBytes);
        Assert.NotEmpty(capturedBytes);
    }

    [Fact]
    public void Export_EmptyData_WrittenBytesArePdf()
    {
        Export(MakeData());
        // PDF files start with "%PDF"
        Assert.Equal((byte)'%', capturedBytes![0]);
        Assert.Equal((byte)'P', capturedBytes[1]);
        Assert.Equal((byte)'D', capturedBytes[2]);
        Assert.Equal((byte)'F', capturedBytes[3]);
    }

    // --- File type filtering ---

    [Fact]
    public void Export_NonPictureFilesOnly_StillWritesPdf()
    {
        Export(MakeData(files: [MakeNonPictureFile(FileType.Document), MakeNonPictureFile(FileType.Movie)]));
        A.CallTo(() => fakeFileSystem.File.WriteAllBytes(DestFile, A<byte[]>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_PictureFile_ReadsBytesViaIFileSystem()
    {
        Export(MakeData(files: [MakePictureFile()]));
        A.CallTo(() => fakeFileSystem.File.ReadAllBytes(AbsPath)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_PictureFile_WritesPdf()
    {
        Export(MakeData(files: [MakePictureFile()]));
        A.CallTo(() => fakeFileSystem.File.WriteAllBytes(DestFile, A<byte[]>._))
            .MustHaveHappenedOnceExactly();
        Assert.NotNull(capturedBytes);
        Assert.NotEmpty(capturedBytes);
    }

    [Fact]
    public void Export_NonPictureFile_DoesNotReadBytes()
    {
        Export(MakeData(files: [MakeNonPictureFile()]));
        A.CallTo(() => fakeFileSystem.File.ReadAllBytes(A<string>._)).MustNotHaveHappened();
    }

    // --- Picture metadata does not throw ---

    [Fact]
    public void Export_PictureWithDatetime_DoesNotThrow()
    {
        Export(MakeData(files: [MakePictureFile(datetime: "2020-06-15")]));
        Assert.NotNull(capturedBytes);
    }

    [Fact]
    public void Export_PictureWithDescription_DoesNotThrow()
    {
        Export(MakeData(files: [MakePictureFile(description: "Holiday")]));
        Assert.NotNull(capturedBytes);
    }

    [Fact]
    public void Export_PictureWithPerson_DoesNotThrow()
    {
        var person = new PersonModel { Id = 10, ShortName = "Alice", FullName = "Alice Smith" };
        Export(MakeData(files: [MakePictureFile(personIds: [10])], persons: [person]));
        Assert.NotNull(capturedBytes);
    }

    [Fact]
    public void Export_PictureWithLocationNoPosition_DoesNotThrow()
    {
        var location = new LocationModel { Id = 20, Name = "Paris" };
        Export(MakeData(files: [MakePictureFile(locationIds: [20])], locations: [location]));
        Assert.NotNull(capturedBytes);
    }

    [Fact]
    public void Export_PictureWithLocationAndPositionLink_DoesNotThrow()
    {
        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder { LocationLink = LocationLink }.Build());
        var location = new LocationModel { Id = 20, Name = "Paris", Position = "48.8 2.3" };
        Export(MakeData(files: [MakePictureFile(locationIds: [20])], locations: [location]));
        Assert.NotNull(capturedBytes);
    }

    [Fact]
    public void Export_PictureWithTag_DoesNotThrow()
    {
        var tag = new TagModel { Id = 30, Name = "Nature" };
        Export(MakeData(files: [MakePictureFile(tagIds: [30])], tags: [tag]));
        Assert.NotNull(capturedBytes);
    }

    [Fact]
    public void Export_PictureWithPositionAndLinkConfig_DoesNotThrow()
    {
        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder { LocationLink = LocationLink }.Build());
        Export(MakeData(files: [MakePictureFile(position: "59.3 18.1")]));
        Assert.NotNull(capturedBytes);
    }

    [Fact]
    public void Export_PictureWithPositionButNoLinkConfig_DoesNotThrow()
    {
        Export(MakeData(files: [MakePictureFile(position: "59.3 18.1")]));
        Assert.NotNull(capturedBytes);
    }

    [Fact]
    public void Export_PictureWithOrientation90_DoesNotThrow()
    {
        Export(MakeData(files: [MakePictureFile(orientation: 6)])); // EXIF 6 = 90° CW
        Assert.NotNull(capturedBytes);
    }

    [Fact]
    public void Export_MultiplePictureFiles_WritesPdfOnce()
    {
        Export(MakeData(files: [MakePictureFile(), MakePictureFile()]));
        A.CallTo(() => fakeFileSystem.File.WriteAllBytes(DestFile, A<byte[]>._))
            .MustHaveHappenedOnceExactly();
    }
}

