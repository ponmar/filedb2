using FakeItEasy;
using System.Collections.Generic;
using System.Linq;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class LocationViewModelTests
{
    [Fact]
    public void Constructor_SelectsLocationFromCurrentFile()
    {
        var locations = new[]
        {
            SearchFilterViewModelTestHelpers.CreateLocation(1, "Home"),
            SearchFilterViewModelTestHelpers.CreateLocation(2, "Work"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.GetLocationsFromFile(99)).Returns([SearchFilterViewModelTestHelpers.CreateLocation(2, "Work")]);

        var viewModel = new LocationViewModel(
            SearchFilterViewModelTestHelpers.CreateLocationsRepository(locations),
            SearchFilterViewModelTestHelpers.CreateFileSelector(SearchFilterViewModelTestHelpers.CreateFile(99, "photo.jpg")),
            SearchFilterViewModelTestHelpers.CreateDatabaseAccessProvider(dbAccess));

        Assert.Equal("Work", viewModel.SelectedLocation!.Name);
    }

    [Fact]
    public void ApplyFilter_UsesSelectedLocation()
    {
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "location.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithLocations(A<IEnumerable<int>>.That.Matches(ids => ids.SequenceEqual(new[] { 2 })))).Returns(files);
        var viewModel = new LocationViewModel(
            SearchFilterViewModelTestHelpers.CreateLocationsRepository([
                SearchFilterViewModelTestHelpers.CreateLocation(2, "Work"),
            ]),
            SearchFilterViewModelTestHelpers.CreateFileSelector(),
            SearchFilterViewModelTestHelpers.CreateDatabaseAccessProvider(dbAccess))
        {
            SelectedLocation = new LocationForSearch(2, "Work"),
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
