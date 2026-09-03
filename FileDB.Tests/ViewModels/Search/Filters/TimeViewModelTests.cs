using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDB.Tests.ViewModels.Search.Filters;

public class TimeViewModelTests
{
    [Fact]
    public void Constructor_WithCurrentFile_SetsCurrentTime()
    {
        var fileSelector = SearchFilterViewModelTestHelpers.CreateFileSelector(
            SearchFilterViewModelTestHelpers.CreateFile(1, "photo.jpg", "2024-03-14T15:16:17"));

        var viewModel = new TimeViewModel(fileSelector);

        Assert.True(viewModel.CurrentFileHasTime);
        Assert.Equal(TimeSpan.FromHours(15) + TimeSpan.FromMinutes(16) + TimeSpan.FromSeconds(17), viewModel.StartTime);
        Assert.Equal(TimeSpan.FromHours(15) + TimeSpan.FromMinutes(16) + TimeSpan.FromSeconds(17), viewModel.EndTime);
    }

    [Fact]
    public void ApplyFilter_SwapsReversedTimes()
    {
        var fileSelector = SearchFilterViewModelTestHelpers.CreateFileSelector();
        var dbAccess = A.Fake<IDatabaseAccess>();
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "photo.jpg"),
        };
        var start = new TimeOnly(8, 0);
        var end = new TimeOnly(18, 0);
        A.CallTo(() => dbAccess.SearchFilesByTime(start, end)).Returns(files);
        var viewModel = new TimeViewModel(fileSelector)
        {
            StartTime = end.ToTimeSpan(),
            EndTime = start.ToTimeSpan(),
        };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
