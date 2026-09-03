using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDB.Tests.ViewModels.Search.Filters;

public class NoDateTimeViewModelTests
{
    [Fact]
    public void ApplyFilter_ReturnsFilesWithoutDate()
    {
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "a.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithoutDate()).Returns(files);
        var viewModel = new NoDateTimeViewModel();

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
