using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDB.Tests.ViewModels.Search.Filters;

public class PositionViewModelTests
{
    [Fact]
    public void Constructor_LoadsOnlyLocationsWithPosition()
    {
        var locations = new[]
        {
            SearchFilterViewModelTestHelpers.CreateLocation(1, "No position"),
            SearchFilterViewModelTestHelpers.CreateLocation(2, "With position", "1 2"),
        };

        var viewModel = new PositionViewModel(
            SearchFilterViewModelTestHelpers.CreateLocationsRepository(locations),
            SearchFilterViewModelTestHelpers.CreateDatabaseAccessProvider(A.Fake<IDatabaseAccess>()),
            SearchFilterViewModelTestHelpers.CreateFileSelector());

        Assert.Single(viewModel.LocationsWithPosition);
        Assert.Equal("With position", viewModel.LocationsWithPosition[0].Name);
    }

    [Fact]
    public void ApplyFilter_CombinesFileAndLocationMatches()
    {
        var dbAccess = A.Fake<IDatabaseAccess>();
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "near-file.jpg"),
        };
        var locations = new[]
        {
            SearchFilterViewModelTestHelpers.CreateLocation(2, "Near location"),
        };
        A.CallTo(() => dbAccess.SearchFilesNearGpsPosition(34.123, 75.321, 500)).Returns(files);
        A.CallTo(() => dbAccess.SearchLocationsNearGpsPosition(34.123, 75.321, 500)).Returns(locations);
        A.CallTo(() => dbAccess.SearchFilesWithLocations(A<IEnumerable<int>>.That.Matches(ids => ids.SequenceEqual(new[] { 2 })))).Returns([
            SearchFilterViewModelTestHelpers.CreateFile(2, "near-location.jpg"),
        ]);

        var viewModel = new PositionViewModel(
            SearchFilterViewModelTestHelpers.CreateLocationsRepository([]),
            SearchFilterViewModelTestHelpers.CreateDatabaseAccessProvider(dbAccess),
            SearchFilterViewModelTestHelpers.CreateFileSelector())
        {
            PositionText = "34.123 75.321",
            Radius = 500,
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ApplyFilter_DeduplicatesMatchingFiles()
    {
        var file = SearchFilterViewModelTestHelpers.CreateFile(1, "near-file.jpg");
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesNearGpsPosition(34.123, 75.321, 500)).Returns([file]);
        A.CallTo(() => dbAccess.SearchLocationsNearGpsPosition(34.123, 75.321, 500)).Returns([
            SearchFilterViewModelTestHelpers.CreateLocation(2, "Near location", "1 2"),
        ]);
        A.CallTo(() => dbAccess.SearchFilesWithLocations(A<IEnumerable<int>>.That.Matches(ids => ids.SequenceEqual(new[] { 2 })))).Returns([file]);

        var viewModel = new PositionViewModel(
            SearchFilterViewModelTestHelpers.CreateLocationsRepository([]),
            SearchFilterViewModelTestHelpers.CreateDatabaseAccessProvider(dbAccess),
            SearchFilterViewModelTestHelpers.CreateFileSelector())
        {
            PositionText = "34.123 75.321",
            Radius = 500,
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Single(result);
    }
}
