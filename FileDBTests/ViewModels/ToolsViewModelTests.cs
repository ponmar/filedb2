using FakeItEasy;
using Xunit;
using FileDB;
using FileDB.Notifications;
using FileDB.ViewModels;
using FileDB.Dialogs;
using FileDB.Model;
using FileDBInterface.FilesystemAccess;

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

        var viewModel = CreateViewModel();
        viewModel.FileIdsInput = "1;2";

        // Arrange progress dialog to execute work
        A.CallTo(() => dialogs.ShowProgressDialogAsync(A<Action<IProgress<string>>>._))
            .Invokes((Action<IProgress<string>> work) => work(new Progress<string>(s => { })))
            .Returns(Task.CompletedTask);

        // Act
        viewModel.ReloadExifForFilesCommand.Execute(null);

        // Assert
        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFileFromMetaData(1, A<IFilesystemAccess>._)).MustHaveHappened();
        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFileFromMetaData(2, A<IFilesystemAccess>._)).MustHaveHappened();
        A.CallTo(() => dialogs.ShowInfoDialogAsync(A<string>._)).MustHaveHappened();
    }
}
