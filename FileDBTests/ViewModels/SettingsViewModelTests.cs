using FakeItEasy;
using FileDB.Configuration;
using FileDB.Dialogs;
using FileDB.Model;
using FileDB.Services;
using FileDB.ViewModels;
using FluentValidation.Results;
using System.Globalization;
using System.IO.Abstractions;
using Xunit;

namespace FileDBTests.ViewModels;

public class SettingsViewModelTests
{
    private readonly IConfigProvider configProvider = A.Fake<IConfigProvider>();
    private readonly IConfigUpdater configUpdater = A.Fake<IConfigUpdater>();
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly IFileSystem fileSystem = A.Fake<IFileSystem>();
    private readonly IFilesWritePermissionChecker filesWritePermissionChecker = A.Fake<IFilesWritePermissionChecker>();

    public SettingsViewModelTests()
    {
        A.CallTo(() => configProvider.Config).Returns(new ConfigBuilder
        {
            SlideshowDelay = 3,
            SearchHistorySize = 10,
            DefaultSortMethod = SortMethod.Date,
            KeepSelectionAfterSort = true,
            IncludeHiddenDirectories = false,
            BlacklistedFilePathPatterns = "tmp",
            WhitelistedFilePathPatterns = "*",
            ReadOnly = false,
            BackupReminder = true,
            BirthdayReminder = true,
            BirthdayReminderForDeceased = false,
            RipReminder = true,
            MissingFilesRootDirNotification = true,
            LocationLink = "https://maps.example/?q={0},{1}",
            WindowMode = WindowMode.Normal,
            ImageMemoryCacheCount = 5,
            NumImagesToPreload = 2,
            OverlayTextSize = 12,
            OverlayTextSizeLarge = 16,
            ShortItemNameMaxLength = 20,
            Language = "sv-SE",
            Theme = Theme.Dark,
            LoadExifOrientationFromFileWhenMissingInDatabase = true,
            InitialSearchFilterType = FilterType.AllFiles,
        }.Build());
        A.CallTo(() => filesWritePermissionChecker.HasWritePermission).Returns(true);
    }

    [Fact]
    public void Constructor_LoadsConfigurationValues()
    {
        var viewModel = new SettingsViewModel(configProvider, configUpdater, dialogs, fileSystem, filesWritePermissionChecker, A.Fake<IFileBackup>());

        Assert.Equal(3, viewModel.SlideshowDelay);
        Assert.Equal(Theme.Dark, viewModel.Theme);
        Assert.False(viewModel.CanSave);
        Assert.True(viewModel.SelectedLanguage!.Name == CultureInfo.GetCultureInfo("sv-SE").Name);
    }

    [Fact]
    public void SetDefaultThemeCommand_ResetsTheme()
    {
        var viewModel = new SettingsViewModel(configProvider, configUpdater, dialogs, fileSystem, filesWritePermissionChecker, A.Fake<IFileBackup>());
        viewModel.Theme = Theme.Light;

        viewModel.SetDefaultThemeCommand.Execute(null);

        Assert.Equal(Theme.Default, viewModel.Theme);
    }

    [Fact]
    public async Task SaveConfigurationAsync_ValidationFails_ShowsError()
    {
        var viewModel = new SettingsViewModel(configProvider, configUpdater, dialogs, fileSystem, filesWritePermissionChecker, A.Fake<IFileBackup>())
        {
            SlideshowDelay = 0,
        };

        await viewModel.SaveConfigurationCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<ValidationResult>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => configUpdater.UpdateConfig(A<Config>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task SaveConfigurationAsync_UserDeclines_DoesNotSave()
    {
        var configPath = "/config.json";
        A.CallTo(() => configProvider.FilePaths).Returns(new ApplicationFilePaths("/files", configPath, "/db.db"));
        A.CallTo(() => fileSystem.File.Exists(configPath)).Returns(true);
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(false);

        var viewModel = new SettingsViewModel(configProvider, configUpdater, dialogs, fileSystem, filesWritePermissionChecker, A.Fake<IFileBackup>())
        {
            IsDirty = true,
        };

        await viewModel.SaveConfigurationCommand.ExecuteAsync(null);

        A.CallTo(() => fileSystem.File.WriteAllText(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => configUpdater.UpdateConfig(A<Config>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Theme_Change_SendsSetThemeMessage()
    {
        var recorder = new EventRecorder();
        recorder.Record<SetTheme>();
        var viewModel = new SettingsViewModel(configProvider, configUpdater, dialogs, fileSystem, filesWritePermissionChecker, A.Fake<IFileBackup>());

        viewModel.Theme = Theme.Light;

        Assert.Equal(2, recorder.GetRecording<SetTheme>().Count());
    }
}




