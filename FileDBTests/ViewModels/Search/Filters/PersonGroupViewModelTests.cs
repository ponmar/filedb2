using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class PersonGroupViewModelTests
{
    [Fact]
    public void Constructor_SelectsPersonsFromCurrentFile()
    {
        var persons = new[]
        {
            SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice"),
            SearchFilterViewModelTestHelpers.CreatePerson(2, "Bob"),
            SearchFilterViewModelTestHelpers.CreatePerson(3, "Charlie"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.GetPersonsFromFile(99)).Returns([
            SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice"),
            SearchFilterViewModelTestHelpers.CreatePerson(3, "Charlie"),
        ]);

        var viewModel = new PersonGroupViewModel(
            SearchFilterViewModelTestHelpers.CreatePersonsRepository(persons),
            SearchFilterViewModelTestHelpers.CreateFileSelector(SearchFilterViewModelTestHelpers.CreateFile(99, "photo.jpg")),
            SearchFilterViewModelTestHelpers.CreateDatabaseAccessProvider(dbAccess));

        Assert.Equal(2, viewModel.SelectedPersons.Count);
    }

    [Fact]
    public void ApplyFilter_UsesSelectedPersons()
    {
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "group.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithPersonGroupOnly(A<IEnumerable<int>>.That.Matches(ids => ids.SequenceEqual(new[] { 1, 3 })))).Returns(files);
        var viewModel = new PersonGroupViewModel(
            SearchFilterViewModelTestHelpers.CreatePersonsRepository([
                SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice"),
                SearchFilterViewModelTestHelpers.CreatePerson(3, "Charlie"),
            ]),
            SearchFilterViewModelTestHelpers.CreateFileSelector(),
            SearchFilterViewModelTestHelpers.CreateDatabaseAccessProvider(dbAccess))
        {
            SelectedPersons = [new PersonForSearch(1, "Alice"), new PersonForSearch(3, "Charlie")],
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
