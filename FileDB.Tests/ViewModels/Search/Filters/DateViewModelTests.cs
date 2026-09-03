using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDB.Tests.ViewModels.Search.Filters;

public class DateViewModelTests
{
    [Fact]
    public void Constructor_WithCurrentFile_DoesNotLoseDatetime()
    {
        var fileSelector = SearchFilterViewModelTestHelpers.CreateFileSelector(
            SearchFilterViewModelTestHelpers.CreateFile(1, "photo.jpg", "2024-03-14T15:16:17"));

        var viewModel = new DateViewModel(fileSelector);

        Assert.True(viewModel.CurrentFileHasDateTime);
        Assert.Equal("2024-03-14T15:16:17", viewModel.FirstDateTime.ToString("yyyy-MM-ddTHH:mm:ss"));
        Assert.Equal("2024-03-14T15:16:17", viewModel.SecondDateTime.ToString("yyyy-MM-ddTHH:mm:ss"));
    }

    [Fact]
    public void ApplyFilter_WithRange_UsesStartAndEndDates()
    {
        var fileSelector = SearchFilterViewModelTestHelpers.CreateFileSelector();
        var dbAccess = A.Fake<IDatabaseAccess>();
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "photo.jpg"),
        };
        A.CallTo(() => dbAccess.SearchFilesByDate(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10))).Returns(files);
        var viewModel = new DateViewModel(fileSelector)
        {
            DateIsRange = true,
            FirstDateTime = new DateTimeOffset(new DateTime(2024, 1, 10)),
            SecondDateTime = new DateTimeOffset(new DateTime(2024, 1, 1)),
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
