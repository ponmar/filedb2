using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Model;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDB.Tests.ViewModels.Search.Filters;

public class DirectoryViewModelTests
{
    [Fact]
    public void Constructor_WithCurrentFile_UsesParentDirectory()
    {
        var fileSelector = SearchFilterViewModelTestHelpers.CreateFileSelector(
            SearchFilterViewModelTestHelpers.CreateFile(1, "photos/sub/image.jpg"));

        var viewModel = new DirectoryViewModel(A.Fake<IDialogs>(), fileSelector, A.Fake<IConfigProvider>());

        Assert.Equal("photos/sub", viewModel.DirectoryPath);
    }

    [Fact]
    public void ApplyFilter_UsesDirectoryPath()
    {
        var dbAccess = A.Fake<IDatabaseAccess>();
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "photos/sub/image.jpg"),
        };
        A.CallTo(() => dbAccess.SearchFilesByPath("photos/sub")).Returns(files);
        var viewModel = new DirectoryViewModel(A.Fake<IDialogs>(), SearchFilterViewModelTestHelpers.CreateFileSelector(), A.Fake<IConfigProvider>())
        {
            DirectoryPath = "photos/sub",
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
