using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class SeasonViewModelTests
{
    [Fact]
    public void Constructor_DefaultsToFirstSeason()
    {
        var viewModel = new SeasonViewModel();

        Assert.Equal(SeasonViewModel.Seasons.First(), viewModel.SelectedSeason);
    }

    [Fact]
    public void ApplyFilter_UsesSelectedSeason()
    {
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "a.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesBySeason(Season.Summer)).Returns(files);
        var viewModel = new SeasonViewModel { SelectedSeason = Season.Summer };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
