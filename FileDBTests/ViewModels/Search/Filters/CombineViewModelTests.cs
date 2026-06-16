using FakeItEasy;
using System.Collections.Generic;
using System.Linq;
using FileDB.Services;
using FileDB.ViewModels.Search;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class CombineViewModelTests
{
    [Fact]
    public void ApplyFilter_ReturnsFilesFromResultList()
    {
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "one.jpg"),
            SearchFilterViewModelTestHelpers.CreateFile(2, "two.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesFromIds(A<IEnumerable<int>>.That.Matches(ids => ids.SequenceEqual(new[] { 1, 2 })))).Returns(files);
        var viewModel = new CombineViewModel(
            SearchFilterViewModelTestHelpers.CreateSearchResultRepository([]),
            A.Fake<IClipboardService>(),
            SearchFilterViewModelTestHelpers.CreateFileSelector())
        {
            CombineSearchResult = "1;2",
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }

    [Fact]
    public void CombineSearchResultPossible_ReturnsTrueWhenBothInputsHaveContent()
    {
        var viewModel = new CombineViewModel(
            SearchFilterViewModelTestHelpers.CreateSearchResultRepository([]),
            A.Fake<IClipboardService>(),
            SearchFilterViewModelTestHelpers.CreateFileSelector())
        {
            CombineSearch1 = "1",
            CombineSearch2 = "2",
        };

        Assert.True(viewModel.CombineSearchResultPossible);
    }
}
