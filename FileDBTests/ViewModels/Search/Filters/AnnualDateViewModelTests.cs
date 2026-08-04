using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class AnnualDateViewModelTests
{
    [Fact]
    public void Constructor_WithCurrentFile_UsesMonthAndDayFromFile()
    {
        var fileSelector = SearchFilterViewModelTestHelpers.CreateFileSelector(
            SearchFilterViewModelTestHelpers.CreateFile(1, "photo.jpg", "2024-03-14T15:16:17"));

        var viewModel = new AnnualDateViewModel(fileSelector);

        Assert.True(viewModel.CurrentFileHasDateTime);
        Assert.Equal(3, viewModel.SelectedAnnualMonthStart);
        Assert.Equal(14, viewModel.SelectedAnnualDayStart);
        Assert.Equal(3, viewModel.SelectedAnnualMonthEnd);
        Assert.Equal(14, viewModel.SelectedAnnualDayEnd);
    }

    [Fact]
    public void ApplyFilter_WithRange_UsesConfiguredRange()
    {
        var dbAccess = A.Fake<IDatabaseAccess>();
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "photo.jpg"),
        };
        A.CallTo(() => dbAccess.SearchFilesByAnnualDate(3, 1, 5, 31)).Returns(files);
        var viewModel = new AnnualDateViewModel(SearchFilterViewModelTestHelpers.CreateFileSelector())
        {
            AnnualDateIsRange = true,
            SelectedAnnualMonthStart = 3,
            SelectedAnnualDayStart = 1,
            SelectedAnnualMonthEnd = 5,
            SelectedAnnualDayEnd = 31,
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
