using FakeItEasy;
using FileDB.Export.Database;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using System.IO.Abstractions;
using Xunit;

namespace FileDBTests.Export.Database;

public class DatabaseJsonExporterTests
{
    private const string DestFile = "/output/Export.json";

    private readonly IFileSystem fakeFileSystem = A.Fake<IFileSystem>();
    private string? capturedJson;

    public DatabaseJsonExporterTests()
    {
        A.CallTo(() => fakeFileSystem.File.WriteAllText(A<string>._, A<string>._))
            .Invokes(call => capturedJson = (string)call.Arguments[1]!);
    }

    private DatabaseJsonExporter CreateExporter() => new(fakeFileSystem);

    private static DatabaseExport MakeData(
        List<PersonModel>? persons = null,
        List<LocationModel>? locations = null,
        List<TagModel>? tags = null,
        List<FileModel>? files = null) =>
        new("1.0", new DateTime(2024, 1, 15, 10, 0, 0),
            persons ?? [],
            locations ?? [],
            tags ?? [],
            files ?? []);

    private void Export(DatabaseExport data) => CreateExporter().Export(data, DestFile);

    // --- Output file ---

    [Fact]
    public void Export_WritesToDestinationFilePath()
    {
        Export(MakeData());
        A.CallTo(() => fakeFileSystem.File.WriteAllText(DestFile, A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_WritesNonEmptyContent()
    {
        Export(MakeData());
        Assert.NotNull(capturedJson);
        Assert.NotEmpty(capturedJson);
    }

    // --- JSON validity ---

    [Fact]
    public void Export_OutputIsValidJson()
    {
        Export(MakeData());
        var ex = Record.Exception(() => Newtonsoft.Json.JsonConvert.DeserializeObject(capturedJson!));
        Assert.Null(ex);
    }

    [Fact]
    public void Export_OutputIsFormattedJson()
    {
        Export(MakeData());
        Assert.Contains('\n', capturedJson!);
    }

    // --- Content ---

    [Fact]
    public void Export_VersionInJson()
    {
        Export(MakeData());
        Assert.Contains("1.0", capturedJson!);
    }

    [Fact]
    public void Export_PersonInJson()
    {
        var person = new PersonModel { Id = 10, ShortName = "Alice", FullName = "Alice Smith" };
        Export(MakeData(persons: [person]));
        Assert.Contains("Alice Smith", capturedJson);
    }

    [Fact]
    public void Export_LocationInJson()
    {
        var location = new LocationModel { Id = 20, Name = "Paris" };
        Export(MakeData(locations: [location]));
        Assert.Contains("Paris", capturedJson);
    }

    [Fact]
    public void Export_TagInJson()
    {
        var tag = new TagModel { Id = 30, Name = "Nature" };
        Export(MakeData(tags: [tag]));
        Assert.Contains("Nature", capturedJson);
    }

    [Fact]
    public void Export_FileInJson()
    {
        var file = new FileModel { Id = 1, Path = "photos/sunset.jpg", Description = "Beautiful sunset" };
        Export(MakeData(files: [file]));
        Assert.Contains("photos/sunset.jpg", capturedJson);
        Assert.Contains("Beautiful sunset", capturedJson);
    }
}
