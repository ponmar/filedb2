using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Model;
using FileDB.ViewModels;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;
using System.IO.Abstractions;
using Xunit;

namespace FileDBTests.ViewModels;

public class FilesViewModelTests
{
    private readonly IConfigProvider configProvider = A.Fake<IConfigProvider>();
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IDatabaseAccess dbAccess = A.Fake<IDatabaseAccess>();
    private readonly IFilesystemAccessProvider filesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
    private readonly IFilesystemAccess filesystemAccess = A.Fake<IFilesystemAccess>();
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly IFileSystem fileSystem = A.Fake<IFileSystem>();
    private readonly ICriteriaViewModel criteriaViewModel = A.Fake<ICriteriaViewModel>();

    public FilesViewModelTests()
    {
        A.CallTo(() => dbAccessProvider.DbAccess).Returns(dbAccess);
        A.CallTo(() => filesystemAccessProvider.FilesystemAccess).Returns(filesystemAccess);
        A.CallTo(() => filesystemAccess.FileSystem).Returns(fileSystem);
        A.CallTo(() => configProvider.FilePaths).Returns(new ApplicationFilePaths("/files", "/config.json", "/db.db"));
        A.CallTo(() => configProvider.Config).Returns(new ConfigBuilder().Build());
    }

    [Fact]
    public void Constructor_UsesConfiguredRootDirectory()
    {
        var viewModel = new FilesViewModel(configProvider, dbAccessProvider, filesystemAccessProvider, dialogs, fileSystem, criteriaViewModel);

        Assert.Equal("/files", viewModel.SubdirToScan);
        Assert.True(viewModel.FindFileMetadata);
    }

    [Fact]
    public void SelectAllAndSelectNoneCommands_MoveFilesBetweenCollections()
    {
        var viewModel = new FilesViewModel(configProvider, dbAccessProvider, filesystemAccessProvider, dialogs, fileSystem, criteriaViewModel);
        viewModel.NewFiles.Add(new NewFile("a.jpg", "2025-01-01"));
        viewModel.NewFiles.Add(new NewFile("b.jpg", "2025-01-02"));

        viewModel.SelectAllCommand.Execute(null);

        Assert.Equal(2, viewModel.SelectedFiles.Count);

        viewModel.SelectNoneCommand.Execute(null);

        Assert.Empty(viewModel.SelectedFiles);
    }

    [Fact]
    public async Task BrowseSubDirectoryAsync_SelectedDirectory_AppendsToRoot()
    {
        A.CallTo(() => dialogs.ShowBrowseExistingSubDirectoryDialogAsync(A<string>._, A<string>._)).Returns("subdir");

        var viewModel = new FilesViewModel(configProvider, dbAccessProvider, filesystemAccessProvider, dialogs, fileSystem, criteriaViewModel);

        await viewModel.BrowseSubDirectoryCommand.ExecuteAsync(null);

        Assert.Equal(Path.Combine("/files", "subdir"), viewModel.SubdirToScan);
    }

    [Fact]
    public async Task BrowseSubDirectoryAsync_Cancelled_KeepsRoot()
    {
        A.CallTo(() => dialogs.ShowBrowseExistingSubDirectoryDialogAsync(A<string>._, A<string>._)).Returns((string?)null);

        var viewModel = new FilesViewModel(configProvider, dbAccessProvider, filesystemAccessProvider, dialogs, fileSystem, criteriaViewModel);

        await viewModel.BrowseSubDirectoryCommand.ExecuteAsync(null);

        Assert.Equal("/files", viewModel.SubdirToScan);
    }

    [Fact]
    public async Task ScanNewFilesInDirectoryAsync_EmptyDirectory_ShowsError()
    {
        var viewModel = new FilesViewModel(configProvider, dbAccessProvider, filesystemAccessProvider, dialogs, fileSystem, criteriaViewModel)
        {
            SubdirToScan = string.Empty,
        };

        await viewModel.ScanNewFilesInDirectoryCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ScanNewFilesInDirectoryAsync_DirectoryDoesNotExist_ShowsError()
    {
        A.CallTo(() => fileSystem.Directory.Exists(A<string>._)).Returns(false);
        var viewModel = new FilesViewModel(configProvider, dbAccessProvider, filesystemAccessProvider, dialogs, fileSystem, criteriaViewModel)
        {
            SubdirToScan = "/files/subdir",
        };

        await viewModel.ScanNewFilesInDirectoryCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ScanNewFilesInDirectoryAsync_DirectoryOutsideRoot_ShowsError()
    {
        A.CallTo(() => fileSystem.Directory.Exists(A<string>._)).Returns(true);
        var viewModel = new FilesViewModel(configProvider, dbAccessProvider, filesystemAccessProvider, dialogs, fileSystem, criteriaViewModel)
        {
            SubdirToScan = @"C:\other",
        };

        await viewModel.ScanNewFilesInDirectoryCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ScanNewFilesAsync_UserDeclines_DoesNothing()
    {
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(false);
        var viewModel = new FilesViewModel(configProvider, dbAccessProvider, filesystemAccessProvider, dialogs, fileSystem, criteriaViewModel);

        await viewModel.ScanNewFilesCommand.ExecuteAsync(null);

        A.CallTo(() => filesystemAccess.ListNewFilesystemFiles(A<string>._, A<IEnumerable<string>>._, A<IEnumerable<string>>._, A<bool>._, A<IDatabaseAccess>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ImportNewFilesAsync_UserDeclines_DoesNothing()
    {
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(false);
        var viewModel = new FilesViewModel(configProvider, dbAccessProvider, filesystemAccessProvider, dialogs, fileSystem, criteriaViewModel);

        await viewModel.ImportNewFilesCommand.ExecuteAsync(null);

        A.CallTo(() => dbAccess.InsertFile(A<string>._, A<string?>._, A<IFilesystemAccess>._, A<bool>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task RemoveFileListMethod_InvalidInput_ShowsError()
    {
        var viewModel = new FilesViewModel(configProvider, dbAccessProvider, filesystemAccessProvider, dialogs, fileSystem, criteriaViewModel)
        {
            RemoveFileList = "abc",
        };

        await viewModel.RemoveFileListMethodCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemoveFileListMethod_ValidInput_UserDeclines_DoesNotDelete()
    {
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(false);
        var viewModel = new FilesViewModel(configProvider, dbAccessProvider, filesystemAccessProvider, dialogs, fileSystem, criteriaViewModel)
        {
            RemoveFileList = "1;2",
        };

        await viewModel.RemoveFileListMethodCommand.ExecuteAsync(null);

        A.CallTo(() => dbAccessProvider.DbAccess.DeleteFile(A<int>._)).MustNotHaveHappened();
    }
}
