using FakeItEasy;
using Xunit;
using FileDB;
using FileDB.Notifications;
using FileDB.ViewModels;
using FileDB.Dialogs;
using FileDB.Model;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FileDBTests.ViewModels;

public class ToolsViewModelTests
{
    private readonly IConfigProvider configProvider = A.Fake<IConfigProvider>();
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IFilesystemAccessProvider filesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly System.IO.Abstractions.IFileSystem fileSystem = A.Fake<System.IO.Abstractions.IFileSystem>();
    private readonly IClipboardService clipboardService = A.Fake<IClipboardService>();
    private readonly INotificationManagement notificationManagement = A.Fake<INotificationManagement>();
    private readonly IConfigUpdater configUpdater = A.Fake<IConfigUpdater>();

    private ToolsViewModel CreateViewModel()
    {
        return new ToolsViewModel(configProvider, dbAccessProvider, filesystemAccessProvider, dialogs, fileSystem, clipboardService, notificationManagement, configUpdater);
    }

    private void SetupFilePaths(string filesRootDir = "/files", string dbPath = "/db.db")
    {
        A.CallTo(() => configProvider.FilePaths).Returns(new ApplicationFilePaths(filesRootDir, "/config.json", dbPath));
    }

    private void SetupConfig(string blacklisted = "", string whitelisted = "*", bool includeHidden = false)
    {
        A.CallTo(() => configProvider.Config).Returns(
            new ConfigBuilder
            {
                BlacklistedFilePathPatterns = blacklisted,
                WhitelistedFilePathPatterns = whitelisted,
                IncludeHiddenDirectories = includeHidden,
            }.Build());
    }

    private void SetupProgressDialogToExecuteWork()
    {
        A.CallTo(() => dialogs.ShowProgressDialogAsync(A<Action<IProgress<string>>>._))
            .Invokes((Action<IProgress<string>> work) => work(new Progress<string>(_ => { })))
            .Returns(Task.CompletedTask);
    }

    private void SetupCancellableProgressDialogToExecuteWork()
    {
        A.CallTo(() => dialogs.ShowProgressDialogAsync(A<Action<IProgress<string>, CancellationToken>>._))
            .Invokes((Action<IProgress<string>, CancellationToken> work) => work(new Progress<string>(_ => { }), CancellationToken.None))
            .Returns(Task.CompletedTask);
    }

    // --- CreateDatabaseCommand ---

    [Fact]
    public void CreateDatabaseCommand_DatabaseAlreadyExists_ShowsError()
    {
        SetupFilePaths();
        A.CallTo(() => fileSystem.File.Exists("/db.db")).Returns(true);

        var viewModel = CreateViewModel();
        viewModel.CreateDatabaseCommand.Execute(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void CreateDatabaseCommand_UserDeclines_NoInfoDialogShown()
    {
        SetupFilePaths();
        A.CallTo(() => fileSystem.File.Exists(A<string>._)).Returns(false);
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(false);

        var viewModel = CreateViewModel();
        viewModel.CreateDatabaseCommand.Execute(null);

        A.CallTo(() => dialogs.ShowInfoDialogAsync(A<string>._)).MustNotHaveHappened();
    }

    // --- CreateDatabaseBackupCommand ---

    [Fact]
    public void CreateDatabaseBackupCommand_BackupCreated_ShowsInfoAndDismissesNotifications()
    {
        SetupFilePaths(dbPath: "/db.db");
        SetupProgressDialogToExecuteWork();
        A.CallTo(() => fileSystem.File.Exists("/db.db")).Returns(true);

        var viewModel = CreateViewModel();
        viewModel.CreateDatabaseBackupCommand.Execute(null);

        A.CallTo(() => dialogs.ShowInfoDialogAsync(A<string>._)).MustHaveHappened();
        A.CallTo(() => notificationManagement.DismissNotifications<DatabaseBackupMissingNotification>()).MustHaveHappened();
        A.CallTo(() => notificationManagement.DismissNotifications<DatabaseBackupTooLongTimeAgoNotification>()).MustHaveHappened();
    }

    [Fact]
    public void CreateDatabaseBackupCommand_BackupFails_ShowsError()
    {
        SetupFilePaths(dbPath: "/db.db");
        SetupProgressDialogToExecuteWork();
        A.CallTo(() => fileSystem.File.Exists("/db.db")).Returns(false); // source doesn't exist → IOException

        var viewModel = CreateViewModel();
        viewModel.CreateDatabaseBackupCommand.Execute(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappened();
    }

    // --- FindImportedNoLongerApplicableFilesCommand ---

    [Fact]
    public void FindImportedNoLongerApplicableFilesCommand_AllFilesApplicable_EmptyFileList()
    {
        SetupConfig();
        SetupCancellableProgressDialogToExecuteWork();
        var files = new List<FileModel>
        {
            new() { Id = 1, Path = "photos/a.jpg" },
            new() { Id = 2, Path = "photos/b.jpg" },
        };
        A.CallTo(() => dbAccessProvider.DbAccess.GetFiles()).Returns(files);
        A.CallTo(() => filesystemAccessProvider.FilesystemAccess.PathIsApplicable(A<string>._, A<IEnumerable<string>>._, A<IEnumerable<string>>._, A<bool>._)).Returns(true);

        var viewModel = CreateViewModel();
        viewModel.FindImportedNoLongerApplicableFilesCommand.Execute(null);

        Assert.Empty(viewModel.ImportedNoLongerApplicableFileList);
        A.CallTo(() => dialogs.ShowInfoDialogAsync(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void FindImportedNoLongerApplicableFilesCommand_SomeNotApplicable_FileListPopulated()
    {
        SetupConfig();
        SetupCancellableProgressDialogToExecuteWork();
        var files = new List<FileModel>
        {
            new() { Id = 1, Path = "photos/a.jpg" },
            new() { Id = 2, Path = "photos/b.jpg" },
        };
        A.CallTo(() => dbAccessProvider.DbAccess.GetFiles()).Returns(files);
        A.CallTo(() => filesystemAccessProvider.FilesystemAccess.PathIsApplicable("photos/a.jpg", A<IEnumerable<string>>._, A<IEnumerable<string>>._, A<bool>._)).Returns(false);
        A.CallTo(() => filesystemAccessProvider.FilesystemAccess.PathIsApplicable("photos/b.jpg", A<IEnumerable<string>>._, A<IEnumerable<string>>._, A<bool>._)).Returns(true);

        var viewModel = CreateViewModel();
        viewModel.FindImportedNoLongerApplicableFilesCommand.Execute(null);

        Assert.NotEmpty(viewModel.ImportedNoLongerApplicableFileList);
    }

    [Fact]
    public void CopyImportedNoLongerApplicableFilesListCommand_CallsClipboard()
    {
        var viewModel = CreateViewModel();
        viewModel.ImportedNoLongerApplicableFileList = "1";

        viewModel.CopyImportedNoLongerApplicableFilesListCommand.Execute(null);

        A.CallTo(() => clipboardService.SetTextAsync(A<string>._)).MustHaveHappened();
    }

    // --- DatabaseValidationCommand ---

    [Fact]
    public void DatabaseValidationCommand_NoErrors_EmptyErrorCollection()
    {
        SetupCancellableProgressDialogToExecuteWork();
        A.CallTo(() => dbAccessProvider.DbAccess.GetFiles()).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersons()).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetLocations()).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTags()).Returns([]);

        var viewModel = CreateViewModel();
        viewModel.DatabaseValidationCommand.Execute(null);

        Assert.Empty(viewModel.DabaseValidationErrors);
        A.CallTo(() => dialogs.ShowInfoDialogAsync(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void DatabaseValidationCommand_InvalidFile_AddsErrorAndSetsInvalidFileList()
    {
        SetupCancellableProgressDialogToExecuteWork();
        var invalidFile = new FileModel { Id = 1, Path = "\\invalid\\path" }; // backslash makes path invalid
        A.CallTo(() => dbAccessProvider.DbAccess.GetFiles()).Returns([invalidFile]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersons()).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetLocations()).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTags()).Returns([]);

        var viewModel = CreateViewModel();
        viewModel.DatabaseValidationCommand.Execute(null);

        Assert.NotEmpty(viewModel.DabaseValidationErrors);
        Assert.NotEmpty(viewModel.InvalidFileList);
    }

    [Fact]
    public void CopyInvalidFileListCommand_CallsClipboardAndShowsInfo()
    {
        var viewModel = CreateViewModel();
        viewModel.InvalidFileList = "1";

        viewModel.CopyInvalidFileListCommand.Execute(null);

        A.CallTo(() => clipboardService.SetTextAsync(A<string>._)).MustHaveHappened();
        A.CallTo(() => dialogs.ShowInfoDialogAsync(A<string>._)).MustHaveHappened();
    }

    // --- FileFinderCommand ---

    [Fact]
    public void FileFinderCommand_NoMissingFiles_EmptyMissingFilesList()
    {
        SetupCancellableProgressDialogToExecuteWork();
        A.CallTo(() => dbAccessProvider.DbAccess.GetFiles()).Returns([]);
        A.CallTo(() => filesystemAccessProvider.FilesystemAccess.GetFilesMissingInFilesystem(A<IEnumerable<FileModel>>._)).Returns([]);

        var viewModel = CreateViewModel();
        viewModel.FileFinderCommand.Execute(null);

        Assert.Empty(viewModel.MissingFilesList);
        A.CallTo(() => dialogs.ShowInfoDialogAsync(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void FileFinderCommand_SomeMissingFiles_MissingFilesListPopulated()
    {
        SetupCancellableProgressDialogToExecuteWork();
        var missingFile = new FileModel { Id = 1, Path = "photos/missing.jpg" };
        A.CallTo(() => dbAccessProvider.DbAccess.GetFiles()).Returns([missingFile]);
        A.CallTo(() => filesystemAccessProvider.FilesystemAccess.GetFilesMissingInFilesystem(A<IEnumerable<FileModel>>._)).Returns([missingFile]);

        var viewModel = CreateViewModel();
        viewModel.FileFinderCommand.Execute(null);

        Assert.NotEmpty(viewModel.MissingFilesList);
        A.CallTo(() => dialogs.ShowInfoDialogAsync(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void CopyFileFinderResultCommand_CallsClipboardAndShowsInfo()
    {
        var viewModel = CreateViewModel();
        viewModel.MissingFilesList = "1";

        viewModel.CopyFileFinderResultCommand.Execute(null);

        A.CallTo(() => clipboardService.SetTextAsync(A<string>._)).MustHaveHappened();
        A.CallTo(() => dialogs.ShowInfoDialogAsync(A<string>._)).MustHaveHappened();
    }

    // --- BrowseDatabaseExportDirectoryCommand ---

    [Fact]
    public void BrowseDatabaseExportDirectoryCommand_DirectorySelected_UpdatesDatabaseExportDirectory()
    {
        A.CallTo(() => dialogs.ShowBrowseExistingDirectoryDialogAsync(A<string>._, A<string>._)).Returns("/selected/dir");

        var viewModel = CreateViewModel();
        viewModel.BrowseDatabaseExportDirectoryCommand.Execute(null);

        Assert.Equal("/selected/dir", viewModel.DatabaseExportDirectory);
    }

    [Fact]
    public void BrowseDatabaseExportDirectoryCommand_Cancelled_DirectoryRemainsEmpty()
    {
        A.CallTo(() => dialogs.ShowBrowseExistingDirectoryDialogAsync(A<string>._, A<string>._)).Returns(Task.FromResult<string?>(null));

        var viewModel = CreateViewModel();
        viewModel.BrowseDatabaseExportDirectoryCommand.Execute(null);

        Assert.Equal(string.Empty, viewModel.DatabaseExportDirectory);
    }

    // --- DatabaseExportCommand ---

    [Fact]
    public void DatabaseExportCommand_DirectoryDoesNotExist_ShowsError()
    {
        A.CallTo(() => fileSystem.Directory.Exists(A<string>._)).Returns(false);

        var viewModel = CreateViewModel();
        viewModel.DatabaseExportDirectory = "/export/dir";
        viewModel.DatabaseExportCommand.Execute(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void DatabaseExportCommand_DirectoryNotEmpty_ShowsError()
    {
        A.CallTo(() => fileSystem.Directory.Exists(A<string>._)).Returns(true);
        A.CallTo(() => fileSystem.Directory.GetFileSystemEntries(A<string>._)).Returns(["existing_file"]);

        var viewModel = CreateViewModel();
        viewModel.DatabaseExportDirectory = "/export/dir";
        viewModel.DatabaseExportCommand.Execute(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void DatabaseExportCommand_ValidDirectory_ShowsProgressAndInfo()
    {
        A.CallTo(() => fileSystem.Directory.Exists("/export/dir")).Returns(true);
        A.CallTo(() => fileSystem.Directory.GetFileSystemEntries("/export/dir")).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersons()).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetLocations()).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTags()).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetFiles()).Returns([]);
        SetupProgressDialogToExecuteWork();

        var viewModel = CreateViewModel();
        viewModel.DatabaseExportDirectory = "/export/dir";
        viewModel.DatabaseExportCommand.Execute(null);

        A.CallTo(() => dialogs.ShowProgressDialogAsync(A<Action<IProgress<string>>>._)).MustHaveHappened();
        A.CallTo(() => dialogs.ShowInfoDialogAsync(A<string>._)).MustHaveHappened();
    }

    // --- ReloadExifForFilesCommand (existing tests) ---

    [Fact]
    public void ReloadExifForFilesCommand_InvalidInput_ShowsError()
    {
        var viewModel = CreateViewModel();
        viewModel.FileIdsInput = "1;abc";

        viewModel.ReloadExifForFilesCommand.Execute(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void ReloadExifForFilesCommand_ValidIds_CallsUpdateForEach()
    {
        // Arrange
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(true);
        SetupCancellableProgressDialogToExecuteWork();

        var viewModel = CreateViewModel();
        viewModel.FileIdsInput = "1;2";

        // Act
        viewModel.ReloadExifForFilesCommand.Execute(null);

        // Assert
        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFileFromMetaData(1, A<IFilesystemAccess>._)).MustHaveHappened();
        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFileFromMetaData(2, A<IFilesystemAccess>._)).MustHaveHappened();
        A.CallTo(() => dialogs.ShowInfoDialogAsync(A<string>._)).MustHaveHappened();
    }
}
