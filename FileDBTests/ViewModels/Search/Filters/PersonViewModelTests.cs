using FakeItEasy;
using System.Collections.Generic;
using System.Linq;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class PersonViewModelTests
{
    [Fact]
    public void Constructor_SelectsPersonFromCurrentFile()
    {
        var persons = new[]
        {
            SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice"),
            SearchFilterViewModelTestHelpers.CreatePerson(2, "Bob"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.GetPersonsFromFile(99)).Returns([SearchFilterViewModelTestHelpers.CreatePerson(2, "Bob")]);

        var viewModel = new PersonViewModel(
            SearchFilterViewModelTestHelpers.CreatePersonsRepository(persons),
            SearchFilterViewModelTestHelpers.CreateFileSelector(SearchFilterViewModelTestHelpers.CreateFile(99, "photo.jpg")),
            SearchFilterViewModelTestHelpers.CreateDatabaseAccessProvider(dbAccess));

        Assert.Equal("Bob", viewModel.SelectedPerson!.Name);
    }

    [Fact]
    public void ApplyFilter_WithNegateFalse_ReturnsMatchingFiles()
    {
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "person.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithPersons(A<IEnumerable<int>>.That.Matches(ids => ids.SequenceEqual(new[] { 2 })))).Returns(files);
        var viewModel = new PersonViewModel(
            SearchFilterViewModelTestHelpers.CreatePersonsRepository([
                SearchFilterViewModelTestHelpers.CreatePerson(2, "Bob"),
            ]),
            SearchFilterViewModelTestHelpers.CreateFileSelector(),
            SearchFilterViewModelTestHelpers.CreateDatabaseAccessProvider(dbAccess))
        {
            SelectedPerson = new PersonForSearch(2, "Bob"),
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
