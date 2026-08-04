using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class TagsViewModelTests
{
    [Fact]
    public void Constructor_WithNoSelectedTags_IsInvalid()
    {
        var viewModel = new TagsViewModel(
            SearchFilterViewModelTestHelpers.CreateTagsRepository([SearchFilterViewModelTestHelpers.CreateTag(1, "Favorites")]),
            SearchFilterViewModelTestHelpers.CreateFileSelector(),
            SearchFilterViewModelTestHelpers.CreateDatabaseAccessProvider(A.Fake<IDatabaseAccess>()));

        Assert.True(viewModel.HasErrors);
        Assert.NotEmpty(viewModel.GetErrors(nameof(viewModel.SelectedTags)).Cast<object>());
    }

    [Fact]
    public void ApplyFilter_UsesSelectedTags()
    {
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "tag.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithTagGroupOnly(A<IEnumerable<int>>.That.Matches(ids => ids.SequenceEqual(new[] { 2 })))).Returns(files);
        var viewModel = new TagsViewModel(
            SearchFilterViewModelTestHelpers.CreateTagsRepository([
                SearchFilterViewModelTestHelpers.CreateTag(2, "Two"),
            ]),
            SearchFilterViewModelTestHelpers.CreateFileSelector(),
            SearchFilterViewModelTestHelpers.CreateDatabaseAccessProvider(dbAccess))
        {
            SelectedTags = [new TagForSearch(2, "Two")],
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
