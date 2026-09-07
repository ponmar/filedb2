using FakeItEasy;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Notifications;
using FileDB.Services;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;
using Newtonsoft.Json;
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
        var result = await CreateService().StartAsync(@"C:\collections\photos.FileDB");

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
        fileSystem.AddFile(@"C:\collections\photos.FileDB", new MockFileData(string.Empty));

        var result = await CreateService().StartAsync(@"C:\collections\photos.FileDB");

        Assert.True(result.Succeeded);
        Assert.Contains(result.Notifications, x => x is CollectionGetStartedNotification);
    }

    [Fact]
    public async Task StartAsync_ValidConfig_UsesDatabaseAndFilesystemFactories()
    {
        fileSystem.AddFile(
            @"C:\collections\photos.FileDB",
            new MockFileData(JsonConvert.SerializeObject(DefaultConfigs.Default)));
        fileSystem.AddFile(@"C:\collections\photos.db", new MockFileData(string.Empty));

        var result = await CreateService().StartAsync(@"C:\collections\photos.FileDB");

        Assert.True(result.Succeeded);
        Assert.Same(databaseAccess, result.DatabaseAccess);
        A.CallTo(() => databaseAccessFactory.Create(@"C:\collections\photos.db")).MustHaveHappenedOnceExactly();
        A.CallTo(() => filesystemAccessFactory.Create(@"C:\collections")).MustHaveHappenedOnceExactly();
        Assert.DoesNotContain(result.Notifications, x => x is CollectionGetStartedNotification);
    }

    [Fact]
    public async Task StartAsync_InvalidConfig_ReturnsValidationResultWithoutInitializing()
    {
        var invalidConfig = DefaultConfigs.Default with { SlideshowDelay = 0 };
        fileSystem.AddFile(
            @"C:\collections\photos.FileDB",
            new MockFileData(JsonConvert.SerializeObject(invalidConfig)));

        var result = await CreateService().StartAsync(@"C:\collections\photos.FileDB");

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

        var result = await CreateService().StartAsync(@"C:\collections\photos.FileDB");

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

        var result = await CreateService().StartAsync(@"C:\collections\photos.FileDB");

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

        var result = await CreateService().StartAsync(@"C:\collections\photos.FileDB");

        Assert.Contains(result.Notifications, x => x is CollectionGetStartedNotification);
    }

    [Fact]
    public async Task StartAsync_InvalidExtension_ReturnsErrorWithoutAccessingFiles()
    {
        var result = await CreateService().StartAsync(@"C:\collections\photos.txt");

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
