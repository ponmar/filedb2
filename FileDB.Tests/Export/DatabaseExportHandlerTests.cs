using FakeItEasy;
using FileDB.Export;
using FileDBInterface.Model;
using System.IO.Abstractions;
using Xunit;

namespace FileDB.Tests.Export;

public class DatabaseExportHandlerTests
{
    private const string DestDir = "/output";

    private readonly IFileSystem fakeFileSystem = A.Fake<IFileSystem>();
    private string? capturedPath;
    private string? capturedJson;

    public DatabaseExportHandlerTests()
    {
        A.CallTo(() => fakeFileSystem.File.WriteAllText(A<string>._, A<string>._))
            .Invokes(call =>
            {
                capturedPath = (string)call.Arguments[0]!;
                capturedJson = (string)call.Arguments[1]!;
            });
    }

    private DatabaseExportHandler CreateHandler() => new(DestDir, fakeFileSystem);

    private void Export(
        List<PersonModel>? persons = null,
        List<LocationModel>? locations = null,
        List<TagModel>? tags = null,
        List<FileModel>? files = null) =>
        CreateHandler().Export(persons ?? [], locations ?? [], tags ?? [], files ?? []);

    // --- Output file ---

    [Fact]
    public void Export_WritesJsonFile()
    {
        Export();
        A.CallTo(() => fakeFileSystem.File.WriteAllText(A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Export_WritesToDatabaseExportJsonInDestinationDirectory()
    {
        Export();
        Assert.EndsWith("DatabaseExport.json", capturedPath);
        Assert.StartsWith(DestDir, capturedPath);
    }

    [Fact]
    public void Export_WritesNonEmptyJson()
    {
        Export();
        Assert.NotNull(capturedJson);
        Assert.NotEmpty(capturedJson);
    }

    // --- Content ---

    [Fact]
    public void Export_PersonsIncludedInJson()
    {
        var person = new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith" };
        Export(persons: [person]);
        Assert.Contains("Alice Smith", capturedJson);
    }

    [Fact]
    public void Export_LocationsIncludedInJson()
    {
        var location = new LocationModel { Id = 2, Name = "Paris" };
        Export(locations: [location]);
        Assert.Contains("Paris", capturedJson);
    }

    [Fact]
    public void Export_TagsIncludedInJson()
    {
        var tag = new TagModel { Id = 3, Name = "Nature" };
        Export(tags: [tag]);
        Assert.Contains("Nature", capturedJson);
    }

    [Fact]
    public void Export_FilesIncludedInJson()
    {
        var file = new FileModel { Id = 4, Path = "photos/sunset.jpg" };
        Export(files: [file]);
        Assert.Contains("photos/sunset.jpg", capturedJson);
    }
}
