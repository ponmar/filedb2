using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Lang;
using FileDB.Model;
using FileDB.ViewModels.Dialogs;
using FileDB.ViewModels.Search;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace FileDBTests.ViewModels.Dialogs;

public class ExportSearchResultViewModelTests
{
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IFilesystemAccessProvider filesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
    private readonly IFileSystem fileSystem = A.Fake<IFileSystem>();
    private readonly IConfigProvider configProvider = A.Fake<IConfigProvider>();

    [Fact]
    public void Constructor_DefaultsExportToDisabled()
    {
        var viewModel = new ExportSearchResultViewModel(dialogs, dbAccessProvider, filesystemAccessProvider, fileSystem, configProvider);

        Assert.False(viewModel.ExportEnabled);
        Assert.False(viewModel.ExportIncludesFiles);
        Assert.False(viewModel.ExportIncludesM3u);
    }

    [Fact]
    public async Task ExportCommand_WithNoSelections_ShowsError()
    {
        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddDirectory("/export");
        var viewModel = new ExportSearchResultViewModel(dialogs, dbAccessProvider, filesystemAccessProvider, mockFileSystem, configProvider)
        {
            SearchResult = new SearchResult { Files = [new() { Id = 1, Path = "a.jpg" }] },
            ExportFilesDestinationDirectory = "/export",
        };

        await viewModel.ExportCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void ExportIncludesM3u_SetsFilesFlag()
    {
        var viewModel = new ExportSearchResultViewModel(dialogs, dbAccessProvider, filesystemAccessProvider, fileSystem, configProvider);

        viewModel.ExportIncludesM3u = true;

        Assert.True(viewModel.ExportIncludesFiles);
        viewModel.ExportIncludesFiles = false;
        Assert.False(viewModel.ExportIncludesM3u);
    }

    [Fact]
    public async Task BrowseDestinationDirectoryCommand_UsesSelectedDirectory()
    {
        A.CallTo(() => dialogs.ShowBrowseExistingDirectoryDialogAsync(A<string>._, A<string>._)).Returns("/export");
        var viewModel = new ExportSearchResultViewModel(dialogs, dbAccessProvider, filesystemAccessProvider, fileSystem, configProvider);

        await viewModel.BrowseDestinationDirectoryCommand.ExecuteAsync(null);

        Assert.Equal("/export", viewModel.ExportFilesDestinationDirectory);
    }

    [Fact]
    public async Task ExportCommand_DirectoryMissing_ShowsError()
    {
        A.CallTo(() => fileSystem.Directory.Exists(A<string>._)).Returns(false);
        var viewModel = new ExportSearchResultViewModel(dialogs, dbAccessProvider, filesystemAccessProvider, fileSystem, configProvider)
        {
            SearchResult = new SearchResult { Files = [new() { Id = 1, Path = "a.jpg" }] },
            ExportFilesDestinationDirectory = "/export",
            ExportIncludesFiles = true,
        };

        await viewModel.ExportCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExportCommand_NonEmptyDirectory_ShowsError()
    {
        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddDirectory("/export");
        mockFileSystem.AddFile("/export/existing.txt", new MockFileData("x"));
        var viewModel = new ExportSearchResultViewModel(dialogs, dbAccessProvider, filesystemAccessProvider, mockFileSystem, configProvider)
        {
            SearchResult = new SearchResult { Files = [new() { Id = 1, Path = "a.jpg" }] },
            ExportFilesDestinationDirectory = "/export",
            ExportIncludesFiles = true,
        };

        await viewModel.ExportCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(Strings.ExportDestinationDirectoryIsNotEmpty)).MustHaveHappenedOnceExactly();
    }
}
