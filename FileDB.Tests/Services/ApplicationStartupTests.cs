using FakeItEasy;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Notifications;
using FileDB.Services;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;
using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace FileDB.Tests.Services;

public class ApplicationStartupTests
{
    private readonly MockFileSystem fileSystem = new();
    private readonly IDatabaseAccessFactory databaseAccessFactory = A.Fake<IDatabaseAccessFactory>();
    private readonly IFilesystemAccessFactory filesystemAccessFactory = A.Fake<IFilesystemAccessFactory>();
    private readonly IDatabaseMigrationStartupCoordinator migrationStartupCoordinator = A.Fake<IDatabaseMigrationStartupCoordinator>();
    private readonly IConfigUpdater configUpdater = A.Fake<IConfigUpdater>();
    private readonly IFilesWritePermissionCheckerFactory writePermissionCheckerFactory = A.Fake<IFilesWritePermissionCheckerFactory>();
    private readonly IFilesWritePermissionChecker writePermissionChecker = A.Fake<IFilesWritePermissionChecker>();
    private readonly IFilesystemAccess filesystemAccess = A.Fake<IFilesystemAccess>();
    private readonly IDatabaseAccess databaseAccess = A.Fake<IDatabaseAccess>();

    // Built with the real System.IO.Path (like ApplicationStartupService does) so the paths
    // are fully-qualified on the host OS. This keeps the paths registered in MockFileSystem in
    // sync with what the service computes, on both Windows and Linux.
    private readonly string collectionsDirectory = Path.Combine(Path.GetTempPath(), "filedb-tests", "collections");
    private string ConfigPath => Path.Combine(collectionsDirectory, "photos.FileDB");
    private string DatabasePath => Path.Combine(collectionsDirectory, "photos.db");
    private string InvalidExtensionPath => Path.Combine(collectionsDirectory, "photos.txt");

    public ApplicationStartupTests()
    {
        A.CallTo(() => migrationStartupCoordinator.TryHandleMigrationAsync(
                A<IDatabaseAccess>._,
                A<string>._,
                A<bool>._,
                A<IList<INotification>>._))
            .Returns(true);
        A.CallTo(() => filesystemAccessFactory.Create(A<string>._)).Returns(filesystemAccess);
        A.CallTo(() => databaseAccessFactory.Create(A<string>._)).Returns(databaseAccess);
        A.CallTo(() => writePermissionCheckerFactory.Create()).Returns(writePermissionChecker);
        A.CallTo(() => writePermissionChecker.HasWritePermission).Returns(true);
    }

    [Fact]
    public async Task StartAsync_MissingConfig_UsesDefaultsAndAddsGetStartedNotification()
    {
        var result = await CreateService().StartAsync(ConfigPath);

        Assert.True(result.Succeeded);
        Assert.Equal(DefaultConfigs.Default, result.Config);
        Assert.Contains(result.Notifications, x => x is CollectionGetStartedNotification);
        Assert.IsType<NoDatabaseAccess>(result.DatabaseAccess);
        A.CallTo(() => configUpdater.InitConfig(
            A<ApplicationFilePaths>._,
            DefaultConfigs.Default,
            A<IDatabaseAccess>._,
            filesystemAccess)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task StartAsync_EmptyConfig_UsesDefaultsAndAddsGetStartedNotification()
    {
        fileSystem.AddFile(ConfigPath, new MockFileData(string.Empty));

        var result = await CreateService().StartAsync(ConfigPath);

        Assert.True(result.Succeeded);
        Assert.Contains(result.Notifications, x => x is CollectionGetStartedNotification);
    }

    [Fact]
    public async Task StartAsync_ValidConfig_UsesDatabaseAndFilesystemFactories()
    {
        fileSystem.AddFile(
            ConfigPath,
            new MockFileData(JsonConvert.SerializeObject(DefaultConfigs.Default)));
        fileSystem.AddFile(DatabasePath, new MockFileData(string.Empty));

        var result = await CreateService().StartAsync(ConfigPath);

        Assert.True(result.Succeeded);
        Assert.Same(databaseAccess, result.DatabaseAccess);
        A.CallTo(() => databaseAccessFactory.Create(DatabasePath)).MustHaveHappenedOnceExactly();
        A.CallTo(() => filesystemAccessFactory.Create(collectionsDirectory)).MustHaveHappenedOnceExactly();
        Assert.DoesNotContain(result.Notifications, x => x is CollectionGetStartedNotification);
    }

    [Fact]
    public async Task StartAsync_ConfiguredLanguage_SetsUiCulture()
    {
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var originalDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        try
        {
            var config = DefaultConfigs.Default with { Language = "sv-SE" };
            fileSystem.AddFile(
                ConfigPath,
                new MockFileData(JsonConvert.SerializeObject(config)));

            var result = await CreateService().StartAsync(ConfigPath);

            Assert.True(result.Succeeded);
            Assert.Equal("sv-SE", CultureInfo.CurrentUICulture.Name);
            Assert.Equal("sv-SE", CultureInfo.DefaultThreadCurrentUICulture?.Name);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalUiCulture;
            CultureInfo.DefaultThreadCurrentUICulture = originalDefaultUiCulture;
        }
    }

    [Fact]
    public async Task StartAsync_ConfiguredEnglishLanguage_SetsUiCulture()
    {
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var originalDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        try
        {
            var config = DefaultConfigs.Default with { Language = "en" };
            fileSystem.AddFile(
                ConfigPath,
                new MockFileData(JsonConvert.SerializeObject(config)));

            var result = await CreateService().StartAsync(ConfigPath);

            Assert.True(result.Succeeded);
            Assert.Equal("en", CultureInfo.CurrentUICulture.Name);
            Assert.Equal("en", CultureInfo.DefaultThreadCurrentUICulture?.Name);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalUiCulture;
            CultureInfo.DefaultThreadCurrentUICulture = originalDefaultUiCulture;
        }
    }

    [Fact]
    public async Task StartAsync_NoConfiguredLanguage_LeavesUiCultureUnchanged()
    {
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var originalDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        try
        {
            var result = await CreateService().StartAsync(ConfigPath);

            Assert.True(result.Succeeded);
            Assert.Equal(originalUiCulture.Name, CultureInfo.CurrentUICulture.Name);
            Assert.Equal(originalDefaultUiCulture?.Name, CultureInfo.DefaultThreadCurrentUICulture?.Name);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalUiCulture;
            CultureInfo.DefaultThreadCurrentUICulture = originalDefaultUiCulture;
        }
    }

    [Fact]
    public async Task StartAsync_InvalidConfig_ReturnsValidationResultWithoutInitializing()
    {
        var invalidConfig = DefaultConfigs.Default with { SlideshowDelay = 0 };
        fileSystem.AddFile(
            ConfigPath,
            new MockFileData(JsonConvert.SerializeObject(invalidConfig)));

        var result = await CreateService().StartAsync(ConfigPath);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.ValidationResult);
        A.CallTo(() => migrationStartupCoordinator.TryHandleMigrationAsync(
            A<IDatabaseAccess>._,
            A<string>._,
            A<bool>._,
            A<IList<INotification>>._)).MustNotHaveHappened();
        A.CallTo(() => configUpdater.InitConfig(
            A<ApplicationFilePaths>._,
            A<Config>._,
            A<IDatabaseAccess>._,
            A<IFilesystemAccess>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task StartAsync_NoWritePermission_SwitchesToReadOnlyAndAddsNotification()
    {
        A.CallTo(() => writePermissionChecker.HasWritePermission).Returns(false);

        var result = await CreateService().StartAsync(ConfigPath);

        Assert.True(result.Succeeded);
        Assert.True(result.Config!.ReadOnly);
        Assert.Contains(result.Notifications, x => x is CollectionNoWritePermissionNotification);
        A.CallTo(() => configUpdater.UpdateConfig(A<Config>.That.Matches(x => x.ReadOnly)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task StartAsync_MigrationFailure_ReturnsFailureWithoutInitializingModel()
    {
        A.CallTo(() => migrationStartupCoordinator.TryHandleMigrationAsync(
                A<IDatabaseAccess>._,
                A<string>._,
                A<bool>._,
                A<IList<INotification>>._))
            .Returns(false);

        var result = await CreateService().StartAsync(ConfigPath);

        Assert.False(result.Succeeded);
        A.CallTo(() => configUpdater.InitConfig(
            A<ApplicationFilePaths>._,
            A<Config>._,
            A<IDatabaseAccess>._,
            A<IFilesystemAccess>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task StartAsync_MigrationNotifications_AreReturnedForDispatch()
    {
        A.CallTo(() => migrationStartupCoordinator.TryHandleMigrationAsync(
                A<IDatabaseAccess>._,
                A<string>._,
                A<bool>._,
                A<IList<INotification>>._))
            .Invokes((IDatabaseAccess _, string _, bool _, IList<INotification> notifications) =>
                notifications.Add(new CollectionGetStartedNotification()))
            .Returns(true);

        var result = await CreateService().StartAsync(ConfigPath);

        Assert.Contains(result.Notifications, x => x is CollectionGetStartedNotification);
    }

    [Fact]
    public async Task StartAsync_InvalidExtension_ReturnsErrorWithoutAccessingFiles()
    {
        var result = await CreateService().StartAsync(InvalidExtensionPath);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.ErrorMessage);
        A.CallTo(() => databaseAccessFactory.Create(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => filesystemAccessFactory.Create(A<string>._)).MustNotHaveHappened();
    }

    private ApplicationStartupService CreateService()
    {
        return new(
            fileSystem,
            databaseAccessFactory,
            filesystemAccessFactory,
            migrationStartupCoordinator,
            configUpdater,
            writePermissionCheckerFactory);
    }
}
