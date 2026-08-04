using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class TextViewModelTests
{
    [Fact]
    public void ApplyFilter_UnionsMatchesFromAllEnabledSources()
    {
        var person = SearchFilterViewModelTestHelpers.CreatePerson(1, "Match Person", description: "match");
        var location = SearchFilterViewModelTestHelpers.CreateLocation(1, "Match Location", description: "match");
        var tag = SearchFilterViewModelTestHelpers.CreateTag(1, "Match Tag");

        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(10, "base.jpg"),
        };
        var personFiles = new[] { SearchFilterViewModelTestHelpers.CreateFile(11, "person.jpg") };
        var locationFiles = new[] { SearchFilterViewModelTestHelpers.CreateFile(12, "location.jpg") };
        var tagFiles = new[] { SearchFilterViewModelTestHelpers.CreateFile(13, "tag.jpg") };

        var personsRepo = SearchFilterViewModelTestHelpers.CreatePersonsRepository([person]);
        var locationsRepo = SearchFilterViewModelTestHelpers.CreateLocationsRepository([location]);
        var tagsRepo = SearchFilterViewModelTestHelpers.CreateTagsRepository([tag]);
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFiles("match", false)).Returns(files);
        A.CallTo(() => dbAccess.SearchFilesWithPersons(A<IEnumerable<int>>._)).Returns(personFiles);
        A.CallTo(() => dbAccess.SearchFilesWithLocations(A<IEnumerable<int>>._)).Returns(locationFiles);
        A.CallTo(() => dbAccess.SearchFilesWithTags(A<IEnumerable<int>>._)).Returns(tagFiles);

        var viewModel = new TextViewModel(personsRepo, locationsRepo, tagsRepo)
        {
            TextFilterSearchPattern = "match",
            TextFilterPersons = true,
            TextFilterLocations = true,
            TextFilterTags = true,
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(4, result.Count);
        Assert.Contains(files[0], result);
        Assert.Contains(personFiles[0], result);
        Assert.Contains(locationFiles[0], result);
        Assert.Contains(tagFiles[0], result);
    }
}
