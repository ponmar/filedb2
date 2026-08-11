using FakeItEasy;
using FileDB.Export;
using FileDB.Model;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using System.IO;
using Xunit;

namespace FileDBTests.Export;

public class SearchResultExportDataBuilderTests
{
    private readonly IDatabaseAccessProvider fakeDbAccessProvider = A.Fake<IDatabaseAccessProvider>();

    private SearchResultExportDataBuilder CreateBuilder() =>
        new(fakeDbAccessProvider);

    private static FileModel MakeFileModel(int id = 1, string path = "photo.jpg") =>
        new() { Id = id, Path = path };

    public SearchResultExportDataBuilderTests()
    {
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetLocationsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetTagsFromFile(A<int>._)).Returns([]);
    }

    // --- BuildFilesList ---

    [Fact]
    public void BuildFilesList_SingleFile_ExportedPathContainsIndexAndExtension()
    {
        var result = CreateBuilder().BuildFilesList([MakeFileModel(path: "photo.jpg")]);
        Assert.Equal(Path.Combine("Files", "1.jpg"), result[0].ExportedPath);
    }

    [Fact]
    public void BuildFilesList_TwoFiles_ExportedPathsIncrementIndex()
    {
        var result = CreateBuilder().BuildFilesList([MakeFileModel(1, "photo.jpg"), MakeFileModel(2, "video.mp4")]);
        Assert.Equal(Path.Combine("Files", "1.jpg"), result[0].ExportedPath);
        Assert.Equal(Path.Combine("Files", "2.mp4"), result[1].ExportedPath);
    }

    [Fact]
    public void BuildFilesList_DoesNotQueryDb()
    {
        CreateBuilder().BuildFilesList([MakeFileModel()]);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).MustNotHaveHappened();
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetLocationsFromFile(A<int>._)).MustNotHaveHappened();
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetTagsFromFile(A<int>._)).MustNotHaveHappened();
    }

    [Fact]
    public void BuildFilesList_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new System.Threading.CancellationTokenSource();
        cts.Cancel();
        Assert.Throws<OperationCanceledException>(() =>
            CreateBuilder().BuildFilesList([MakeFileModel()], cts.Token));
    }

    // --- BuildForM3u ---

    [Fact]
    public void BuildForM3u_DoesNotQueryDb()
    {
        CreateBuilder().BuildForM3u([MakeFileModel()], "Test");
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).MustNotHaveHappened();
    }

    [Fact]
    public void BuildForM3u_SetsName()
    {
        var result = CreateBuilder().BuildForM3u([MakeFileModel()], "MyPlaylist");
        Assert.Equal("MyPlaylist", result.Name);
    }

    // --- BuildRich ---

    [Fact]
    public void BuildRich_PersonsQueriedFromDbForEachFile()
    {
        var files = new System.Collections.Generic.List<FileModel> { MakeFileModel(1), MakeFileModel(2) };
        CreateBuilder().BuildRich(files, "Test");
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(1)).MustHaveHappenedOnceExactly();
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(2)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void BuildRich_LocationsQueriedFromDbForEachFile()
    {
        var files = new System.Collections.Generic.List<FileModel> { MakeFileModel(1), MakeFileModel(2) };
        CreateBuilder().BuildRich(files, "Test");
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetLocationsFromFile(1)).MustHaveHappenedOnceExactly();
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetLocationsFromFile(2)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void BuildRich_TagsQueriedFromDbForEachFile()
    {
        var files = new System.Collections.Generic.List<FileModel> { MakeFileModel(1), MakeFileModel(2) };
        CreateBuilder().BuildRich(files, "Test");
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetTagsFromFile(1)).MustHaveHappenedOnceExactly();
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetTagsFromFile(2)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void BuildRich_SamePersonFromTwoFiles_DeduplicatedInResult()
    {
        var person = new PersonModel { Id = 10, ShortName = "Alice", FullName = "Alice Smith" };
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([person]);
        var result = CreateBuilder().BuildRich([MakeFileModel(1), MakeFileModel(2)], "Test");
        Assert.Single(result.Persons);
    }

    [Fact]
    public void BuildRich_PersonReturnedFromDb_IncludedInResult()
    {
        var person = new PersonModel { Id = 10, ShortName = "Alice", FullName = "Alice Smith" };
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(1)).Returns([person]);
        var result = CreateBuilder().BuildRich([MakeFileModel(1)], "Test");
        Assert.Single(result.Persons);
        Assert.Equal("Alice Smith", result.Persons[0].FullName);
    }

    [Fact]
    public void BuildRich_LocationReturnedFromDb_IncludedInResult()
    {
        var location = new LocationModel { Id = 20, Name = "Paris" };
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetLocationsFromFile(1)).Returns([location]);
        var result = CreateBuilder().BuildRich([MakeFileModel(1)], "Test");
        Assert.Single(result.Locations);
        Assert.Equal("Paris", result.Locations[0].Name);
    }

    [Fact]
    public void BuildRich_TagReturnedFromDb_IncludedInResult()
    {
        var tag = new TagModel { Id = 30, Name = "Nature" };
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetTagsFromFile(1)).Returns([tag]);
        var result = CreateBuilder().BuildRich([MakeFileModel(1)], "Test");
        Assert.Single(result.Tags);
        Assert.Equal("Nature", result.Tags[0].Name);
    }

    [Fact]
    public void BuildRich_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new System.Threading.CancellationTokenSource();
        cts.Cancel();
        Assert.Throws<OperationCanceledException>(() =>
            CreateBuilder().BuildRich([MakeFileModel()], "Test", cts.Token));
    }

    // --- BuildForJson ---

    [Fact]
    public void BuildForJson_QueriesDb()
    {
        CreateBuilder().BuildForJson([MakeFileModel(1)], "Test");
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(1)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void BuildForJson_FileListContainsFileId()
    {
        var result = CreateBuilder().BuildForJson([MakeFileModel(42)], "Test");
        Assert.Contains("42", result.FileList);
    }
}
