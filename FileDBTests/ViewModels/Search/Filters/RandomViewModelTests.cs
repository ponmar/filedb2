using FakeItEasy;
using System.Linq;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class RandomViewModelTests
{
    [Fact]
    public void Constructor_SetsDefaultCount()
    {
        var viewModel = new RandomViewModel();

        Assert.Equal(10, viewModel.NumRandomFiles);
    }

    [Fact]
    public void ApplyFilter_UsesConfiguredCount()
    {
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "a.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesRandom(7)).Returns(files);
        var viewModel = new RandomViewModel { NumRandomFiles = 7 };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
