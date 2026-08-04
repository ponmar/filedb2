using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class PersonAgeViewModelTests
{
    [Fact]
    public void Constructor_DefaultsRange()
    {
        var viewModel = new PersonAgeViewModel();

        Assert.Equal(1, viewModel.PersonAgeFrom);
        Assert.Equal(100, viewModel.PersonAgeTo);
    }

    [Fact]
    public void PersonAgeFrom_WhenSetAboveMax_UpdatesMax()
    {
        var viewModel = new PersonAgeViewModel { PersonAgeTo = 10 };

        viewModel.PersonAgeFrom = 20;

        Assert.Equal(20, viewModel.PersonAgeTo);
    }

    [Fact]
    public void PersonAgeTo_WhenSetBelowMin_UpdatesMin()
    {
        var viewModel = new PersonAgeViewModel { PersonAgeFrom = 20 };

        viewModel.PersonAgeTo = 10;

        Assert.Equal(10, viewModel.PersonAgeFrom);
    }

    [Fact]
    public void ApplyFilter_ReturnsFilesWithinAgeRange()
    {
        var person = SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice", "2000-01-01");
        var file = SearchFilterViewModelTestHelpers.CreateFile(10, "photo.jpg", "2020-01-01");
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.GetPersons()).Returns([person]);
        A.CallTo(() => dbAccess.SearchFilesWithPersons(A<IEnumerable<int>>._)).Returns([file]);
        var viewModel = new PersonAgeViewModel { PersonAgeFrom = 18, PersonAgeTo = 21 };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Single(result);
        Assert.Equal(file, result[0]);
    }

    [Fact]
    public void ApplyFilter_DeduplicatesMatchingFiles()
    {
        var file = SearchFilterViewModelTestHelpers.CreateFile(10, "photo.jpg", "2020-01-01");
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.GetPersons()).Returns([
            SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice", "2000-01-01"),
            SearchFilterViewModelTestHelpers.CreatePerson(2, "Bob", "2000-01-01"),
        ]);
        A.CallTo(() => dbAccess.SearchFilesWithPersons(A<IEnumerable<int>>._)).Returns([file]);
        var viewModel = new PersonAgeViewModel { PersonAgeFrom = 18, PersonAgeTo = 30 };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Single(result);
    }
}
