using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDB.Tests.ViewModels.Search.Filters;

public class NumPersonsViewModelTests
{
    [Fact]
    public void Constructor_InitialisesDefaultRange()
    {
        var viewModel = new NumPersonsViewModel();

        Assert.Equal(1, viewModel.NumPersonsMin);
        Assert.Equal(1, viewModel.NumPersonsMax);
    }

    [Fact]
    public void ApplyFilter_UsesSelectedRange()
    {
        var dbAccess = A.Fake<IDatabaseAccess>();
        var viewModel = new NumPersonsViewModel
        {
            NumPersonsMin = 2,
            NumPersonsMax = 4,
        };

        viewModel.ApplyFilter(dbAccess);

        A.CallTo(() => dbAccess.SearchFilesByNumPersons(new Range(2, 4))).MustHaveHappened();
    }

    [Fact]
    public void MinGreaterThanMax_AlignsMax()
    {
        var viewModel = new NumPersonsViewModel
        {
            NumPersonsMax = 2,
        };

        viewModel.NumPersonsMin = 5;

        Assert.Equal(5, viewModel.NumPersonsMax);
    }
}
