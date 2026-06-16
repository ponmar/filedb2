using FakeItEasy;
using System.Collections.Generic;
using System.Linq;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class FileListViewModelTests
{
    [Fact]
    public void Constructor_HasSearchResultReflectsRepository()
    {
        var viewModel = new FileListViewModel(SearchFilterViewModelTestHelpers.CreateSearchResultRepository([
            SearchFilterViewModelTestHelpers.CreateFile(1, "one.jpg"),
        ]));

        Assert.True(viewModel.HasSearchResult);
    }

    [Fact]
    public void ApplyFilter_WithNegateFalse_ReturnsMatchingFiles()
    {
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "one.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesFromIds(A<IEnumerable<int>>.That.Matches(ids => ids.SequenceEqual(new[] { 1 })))).Returns(files);
        var viewModel = new FileListViewModel(SearchFilterViewModelTestHelpers.CreateSearchResultRepository([]))
        {
            FileListIds = "1",
            Negate = false,
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
