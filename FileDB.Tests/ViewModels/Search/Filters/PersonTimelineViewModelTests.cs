using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDB.Tests.ViewModels.Search.Filters;

public class PersonTimelineViewModelTests
{
    private static PersonTimelineViewModel CreateViewModel(
        PersonForSearch[]? persons = null,
        int ageFrom = 0,
        int ageTo = 100,
        int filesPerYear = 1)
    {
        var personModels = (persons ?? []).Select(p =>
            SearchFilterViewModelTestHelpers.CreatePerson(p.Id, p.Name)).ToArray();
        var repo = SearchFilterViewModelTestHelpers.CreatePersonsRepository(personModels);
        return new PersonTimelineViewModel(repo)
        {
            AgeFrom = ageFrom,
            AgeTo = ageTo,
            FilesPerYear = filesPerYear,
        };
    }

    [Fact]
    public void Constructor_DefaultsValues()
    {
        var repo = SearchFilterViewModelTestHelpers.CreatePersonsRepository([]);
        var viewModel = new PersonTimelineViewModel(repo);

        Assert.Equal(0, viewModel.AgeFrom);
        Assert.Equal(100, viewModel.AgeTo);
        Assert.Equal(1, viewModel.FilesPerYear);
    }

    [Fact]
    public void Constructor_PopulatesPersonsFromRepository()
    {
        var persons = new[]
        {
            SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice"),
            SearchFilterViewModelTestHelpers.CreatePerson(2, "Bob"),
        };
        var repo = SearchFilterViewModelTestHelpers.CreatePersonsRepository(persons);

        var viewModel = new PersonTimelineViewModel(repo);

        Assert.Equal(2, viewModel.Persons.Count);
        Assert.Contains(viewModel.Persons, p => p.Name == "Alice");
        Assert.Contains(viewModel.Persons, p => p.Name == "Bob");
    }

    [Fact]
    public void AgeFrom_WhenSetAboveAgeTo_UpdatesAgeTo()
    {
        var repo = SearchFilterViewModelTestHelpers.CreatePersonsRepository([]);
        var viewModel = new PersonTimelineViewModel(repo) { AgeTo = 10 };

        viewModel.AgeFrom = 20;

        Assert.Equal(20, viewModel.AgeTo);
    }

    [Fact]
    public void AgeTo_WhenSetBelowAgeFrom_UpdatesAgeFrom()
    {
        var repo = SearchFilterViewModelTestHelpers.CreatePersonsRepository([]);
        var viewModel = new PersonTimelineViewModel(repo) { AgeFrom = 20 };

        viewModel.AgeTo = 10;

        Assert.Equal(10, viewModel.AgeFrom);
    }

    [Fact]
    public void ApplyFilter_ReturnsOneFilePerAge_WhenFilesPerYearIsDefault()
    {
        // Alice born 2000-01-01; files at age 20 and 21
        var person = SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice", "2000-01-01");
        var file20 = SearchFilterViewModelTestHelpers.CreateFile(10, "age20.jpg", "2020-06-01");
        var file21 = SearchFilterViewModelTestHelpers.CreateFile(11, "age21.jpg", "2021-06-01");
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithPersons(A<IEnumerable<int>>._)).Returns([file20, file21]);
        var repo = SearchFilterViewModelTestHelpers.CreatePersonsRepository([person]);
        var viewModel = new PersonTimelineViewModel(repo)
        {
            SelectedPerson = new PersonForSearch(1, "Alice"),
            AgeFrom = 18,
            AgeTo = 25,
            FilesPerYear = 1,
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.Id == file20.Id);
        Assert.Contains(result, f => f.Id == file21.Id);
    }

    [Fact]
    public void ApplyFilter_LimitsToFilesPerYear()
    {
        // Two files at the same age — FilesPerYear = 1, so only one returned per age
        var person = SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice", "2000-01-01");
        var fileA = SearchFilterViewModelTestHelpers.CreateFile(10, "a.jpg", "2020-03-01");
        var fileB = SearchFilterViewModelTestHelpers.CreateFile(11, "b.jpg", "2020-09-01");
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithPersons(A<IEnumerable<int>>._)).Returns([fileA, fileB]);
        var repo = SearchFilterViewModelTestHelpers.CreatePersonsRepository([person]);
        var viewModel = new PersonTimelineViewModel(repo)
        {
            SelectedPerson = new PersonForSearch(1, "Alice"),
            AgeFrom = 18,
            AgeTo = 25,
            FilesPerYear = 1,
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Single(result);
    }

    [Fact]
    public void ApplyFilter_ReturnsMultipleFilesPerAge_WhenFilesPerYearIsHigher()
    {
        var person = SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice", "2000-01-01");
        var fileA = SearchFilterViewModelTestHelpers.CreateFile(10, "a.jpg", "2020-03-01");
        var fileB = SearchFilterViewModelTestHelpers.CreateFile(11, "b.jpg", "2020-09-01");
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithPersons(A<IEnumerable<int>>._)).Returns([fileA, fileB]);
        var repo = SearchFilterViewModelTestHelpers.CreatePersonsRepository([person]);
        var viewModel = new PersonTimelineViewModel(repo)
        {
            SelectedPerson = new PersonForSearch(1, "Alice"),
            AgeFrom = 18,
            AgeTo = 25,
            FilesPerYear = 2,
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ApplyFilter_ExcludesFilesOutsideAgeRange()
    {
        // Alice age 20 at file date — excluded when range is 30..40
        var person = SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice", "2000-01-01");
        var file = SearchFilterViewModelTestHelpers.CreateFile(10, "photo.jpg", "2020-06-01");
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithPersons(A<IEnumerable<int>>._)).Returns([file]);
        var repo = SearchFilterViewModelTestHelpers.CreatePersonsRepository([person]);
        var viewModel = new PersonTimelineViewModel(repo)
        {
            SelectedPerson = new PersonForSearch(1, "Alice"),
            AgeFrom = 30,
            AgeTo = 40,
            FilesPerYear = 1,
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void ApplyFilter_ExcludesFilesWithoutDatetime()
    {
        var person = SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice", "2000-01-01");
        var fileNoDate = SearchFilterViewModelTestHelpers.CreateFile(10, "photo.jpg", datetime: null);
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithPersons(A<IEnumerable<int>>._)).Returns([fileNoDate]);
        var repo = SearchFilterViewModelTestHelpers.CreatePersonsRepository([person]);
        var viewModel = new PersonTimelineViewModel(repo)
        {
            SelectedPerson = new PersonForSearch(1, "Alice"),
            AgeFrom = 0,
            AgeTo = 100,
            FilesPerYear = 1,
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void ApplyFilter_ReturnsEmpty_WhenPersonHasNoDateOfBirth()
    {
        var person = SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice", birthDate: null);
        var file = SearchFilterViewModelTestHelpers.CreateFile(10, "photo.jpg", "2020-06-01");
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithPersons(A<IEnumerable<int>>._)).Returns([file]);
        var repo = SearchFilterViewModelTestHelpers.CreatePersonsRepository([person]);
        var viewModel = new PersonTimelineViewModel(repo)
        {
            SelectedPerson = new PersonForSearch(1, "Alice"),
            AgeFrom = 0,
            AgeTo = 100,
            FilesPerYear = 1,
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void ApplyFilter_PrefersFileWithFewerPersons()
    {
        // Two files at the same age: fileAlone has only Alice, fileGroup has Alice + Bob
        var person = SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice", "2000-01-01");
        var fileAlone = SearchFilterViewModelTestHelpers.CreateFile(10, "alone.jpg", "2020-06-01");
        var fileGroup = SearchFilterViewModelTestHelpers.CreateFile(11, "group.jpg", "2020-09-01");
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithPersons(A<IEnumerable<int>>._)).Returns([fileGroup, fileAlone]);
        A.CallTo(() => dbAccess.GetPersonCountsFromFiles(A<IEnumerable<int>>._)).Returns(
            new Dictionary<int, int> { [fileAlone.Id] = 1, [fileGroup.Id] = 2 });
        var repo = SearchFilterViewModelTestHelpers.CreatePersonsRepository([person]);
        var viewModel = new PersonTimelineViewModel(repo)
        {
            SelectedPerson = new PersonForSearch(1, "Alice"),
            AgeFrom = 18,
            AgeTo = 25,
            FilesPerYear = 1,
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Single(result);
        Assert.Equal(fileAlone.Id, result[0].Id);
    }

    [Fact]
    public void SelectedPerson_Required_HasValidationError_WhenNull()
    {
        var repo = SearchFilterViewModelTestHelpers.CreatePersonsRepository([]);
        var viewModel = new PersonTimelineViewModel(repo)
        {
            SelectedPerson = null,
        };

        Assert.True(viewModel.HasErrors);
    }
}
