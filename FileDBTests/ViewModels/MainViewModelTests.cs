using FakeItEasy;
using FileDB.Configuration;
using FileDB.Infrastructure;
using FileDB.Model;
using FileDB.Notifications;
using FileDB.ViewModels;
using FileDB.ViewModels.Search;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.ViewModels;

public class MainViewModelTests
{
    private readonly IConfigProvider configProvider = A.Fake<IConfigProvider>();
    private readonly INotificationRepository notificationRepository = A.Fake<INotificationRepository>();
    private readonly ISearchResultRepository searchResultRepository = A.Fake<ISearchResultRepository>();
    private readonly List<INotification> notifications =
    [
        new NotificationForTest(NotificationSeverity.Info),
        new NotificationForTest(NotificationSeverity.Error),
    ];
    private readonly List<FileModel> files =
    [
        new() { Id = 1, Path = "a.jpg" },
        new() { Id = 2, Path = "b.jpg" },
    ];

    public MainViewModelTests()
    {
        A.CallTo(() => configProvider.FilePaths).Returns(new ApplicationFilePaths("/files", "/config.json", "/db.db"));
        A.CallTo(() => configProvider.Config).Returns(new ConfigBuilder
        {
            ReadOnly = true,
            WindowMode = WindowMode.Normal,
        }.Build());
        A.CallTo(() => notificationRepository.Notifications).Returns(notifications);
        A.CallTo(() => searchResultRepository.Files).Returns(files);
    }

    [Fact]
    public void Constructor_InitialisesStateFromDependencies()
    {
        var viewModel = new MainViewModel(configProvider, notificationRepository, searchResultRepository);

        Assert.True(viewModel.ReadOnly);
        Assert.Equal(2, viewModel.NumNotifications);
        Assert.Equal(NotificationSeverity.Error, viewModel.HighlightedNotificationSeverity);
        Assert.Equal(0, viewModel.NumSearchResultFiles);
        Assert.Contains("/config.json", viewModel.Title);
    }

    [Fact]
    public void SearchResultRepositoryUpdatedMessage_RefreshesCount()
    {
        var viewModel = new MainViewModel(configProvider, notificationRepository, searchResultRepository);

        Messenger.Send<SearchResultRepositoryUpdated>();

        Assert.Equal(2, viewModel.NumSearchResultFiles);
    }

    [Fact]
    public void FunctionKeyPressedCommand_SendsMessage()
    {
        var recorder = new SingleEventRecorder<CategorizationFunctionKeyPressed>();
        var viewModel = new MainViewModel(configProvider, notificationRepository, searchResultRepository);

        viewModel.FunctionKeyPressedCommand.Execute("4");

        var message = recorder.AssertEventRecorded();
        Assert.Equal(4, message.FunctionKey);
    }

    [Fact]
    public void QuitSelected_SendsQuitMessage()
    {
        var recorder = new SingleEventRecorder<Quit>();
        var viewModel = new MainViewModel(configProvider, notificationRepository, searchResultRepository);

        viewModel.QuitSelected = true;

        recorder.AssertEventRecorded();
    }

    [Fact]
    public void FunctionKeyPressed_WhenSearchTabNotSelected_DoesNothing()
    {
        var recorder = new EventRecorder();
        recorder.Record<CategorizationFunctionKeyPressed>();
        var viewModel = new MainViewModel(configProvider, notificationRepository, searchResultRepository)
        {
            SearchTabSelected = false,
        };

        viewModel.FunctionKeyPressedCommand.Execute("4");

        Assert.Empty(recorder.GetRecording<CategorizationFunctionKeyPressed>());
    }

    private sealed class NotificationForTest : INotification
    {
        public NotificationForTest(NotificationSeverity severity, string message = "", DateTime? dateTime = null)
        {
            Severity = severity;
            Message = message;
            DateTime = dateTime ?? System.DateTime.Now;
        }

        public NotificationSeverity Severity { get; }
        public string Message { get; }
        public DateTime DateTime { get; }
    }
}
