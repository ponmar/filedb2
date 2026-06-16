using FakeItEasy;
using System.Linq;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class UncategorizedViewModelTests
{
    [Fact]
    public void ApplyFilter_ReturnsFilesWithMissingData()
    {
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "a.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithMissingData()).Returns(files);
        var viewModel = new UncategorizedViewModel();

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
