using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class FileTypeViewModelTests
{
    [Fact]
    public void Constructor_DefaultsToFirstSupportedFileType()
    {
        var viewModel = new FileTypeViewModel();

        Assert.Equal(FileTypeViewModel.FileTypes.First(), viewModel.SelectedFileType);
    }

    [Fact]
    public void ApplyFilter_QueriesEachSupportedExtension()
    {
        var pictureFiles = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "photo.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesByExtension(".jpg")).Returns(pictureFiles);
        A.CallTo(() => dbAccess.SearchFilesByExtension(".jpeg")).Returns([]);
        A.CallTo(() => dbAccess.SearchFilesByExtension(".png")).Returns([]);
        A.CallTo(() => dbAccess.SearchFilesByExtension(".bmp")).Returns([]);
        A.CallTo(() => dbAccess.SearchFilesByExtension(".gif")).Returns([]);
        var viewModel = new FileTypeViewModel { SelectedFileType = FileType.Picture };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(pictureFiles, result);
        A.CallTo(() => dbAccess.SearchFilesByExtension(".jpg")).MustHaveHappened();
        A.CallTo(() => dbAccess.SearchFilesByExtension(".jpeg")).MustHaveHappened();
        A.CallTo(() => dbAccess.SearchFilesByExtension(".png")).MustHaveHappened();
        A.CallTo(() => dbAccess.SearchFilesByExtension(".bmp")).MustHaveHappened();
        A.CallTo(() => dbAccess.SearchFilesByExtension(".gif")).MustHaveHappened();
    }
}
