using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDB.Tests.ViewModels.Search.Filters;

public class AllFilesViewModelTests
{
    [Fact]
    public void ApplyFilter_ReturnsAllFiles()
    {
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "a.jpg"),
            SearchFilterViewModelTestHelpers.CreateFile(2, "b.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.GetFiles()).Returns(files);
        var viewModel = new AllFilesViewModel();

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
