using FakeItEasy;
using FileDB.Export.SearchResult;
using FileDBInterface.FileFormats;
using FileDBInterface.Model;
using System.IO.Abstractions;
using Xunit;

namespace FileDBTests.Export.SearchResult;

public class JsonExporterTests
{
    private const string DestFile = "/output/Export.json";

    private readonly IFileSystem fakeFileSystem = A.Fake<IFileSystem>();
    private string? capturedJson;

    public JsonExporterTests()
    {
        A.CallTo(() => fakeFileSystem.File.WriteAllText(A<string>._, A<string>._))
            .Invokes(call => capturedJson = (string)call.Arguments[1]!);
    }

    private JsonExporter CreateExporter() => new(fakeFileSystem);

    private static SearchResultExport MakeData(
        string name = "My Collection",
        List<ExportedFile>? files = null,
        List<PersonModel>? persons = null,
        List<LocationModel>? locations = null,
        List<TagModel>? tags = null) =>
        new(name, "1.0", new DateTime(2024, 1, 15, 10, 0, 0), string.Empty,
            files ?? [],
            persons ?? [],
            locations ?? [],
            tags ?? [],
            "https://github.com/ponmar/filedb2");

    private void Export(SearchResultExport data) => CreateExporter().Export(data, DestFile);

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
        // Formatted (indented) JSON contains newlines
        Assert.Contains('\n', capturedJson);
    }

    // --- Content ---

    [Fact]
    public void Export_CollectionNameInJson()
    {
        Export(MakeData(name: "My Collection"));
        Assert.Contains("My Collection", capturedJson);
    }

    [Fact]
    public void Export_FileInJson()
    {
        var file = new ExportedFile(1, "Files/1.jpg", "photo.jpg", FileType.Picture, "A sunset", "2020-06-15", null, null, [], [], []);
        Export(MakeData(files: [file]));
        Assert.Contains("photo.jpg", capturedJson);
        Assert.Contains("A sunset", capturedJson);
        Assert.Contains("2020-06-15", capturedJson);
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
    public void Export_ApplicationUrlInJson()
    {
        Export(MakeData());
        Assert.Contains("https://github.com/ponmar/filedb2", capturedJson);
    }
}
