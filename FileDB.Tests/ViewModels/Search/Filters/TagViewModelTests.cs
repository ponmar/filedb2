using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDB.Tests.ViewModels.Search.Filters;

public class TagViewModelTests
{
    [Fact]
    public void Constructor_SelectsTagFromCurrentFile()
    {
        var tags = new[]
        {
            SearchFilterViewModelTestHelpers.CreateTag(1, "One"),
            SearchFilterViewModelTestHelpers.CreateTag(2, "Two"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.GetTagsFromFile(99)).Returns([SearchFilterViewModelTestHelpers.CreateTag(2, "Two")]);

        var viewModel = new TagViewModel(
            SearchFilterViewModelTestHelpers.CreateTagsRepository(tags),
            SearchFilterViewModelTestHelpers.CreateFileSelector(SearchFilterViewModelTestHelpers.CreateFile(99, "photo.jpg")),
            SearchFilterViewModelTestHelpers.CreateDatabaseAccessProvider(dbAccess));

        Assert.Equal("Two", viewModel.SelectedTag!.Name);
    }

    [Fact]
    public void ApplyFilter_UsesSelectedTag()
    {
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "tag.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesWithTags(A<IEnumerable<int>>.That.Matches(ids => ids.SequenceEqual(new[] { 2 })))).Returns(files);
        var viewModel = new TagViewModel(
            SearchFilterViewModelTestHelpers.CreateTagsRepository([
                SearchFilterViewModelTestHelpers.CreateTag(2, "Two"),
            ]),
            SearchFilterViewModelTestHelpers.CreateFileSelector(),
            SearchFilterViewModelTestHelpers.CreateDatabaseAccessProvider(dbAccess))
        {
            SelectedTag = new TagForSearch(2, "Two"),
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
