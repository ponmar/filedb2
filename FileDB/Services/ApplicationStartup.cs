using FileDB.Configuration;
using FileDB.Extensions;
using FileDB.Lang;
using FileDB.Migrators;
using FileDB.Model;
using FileDB.Notifications;
using FileDB.Validators;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.DatabaseAccess.SQLite;
using FileDBInterface.FilesystemAccess;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace FileDB.Services;

public interface IDatabaseAccessFactory
{
    IDatabaseAccess Create(string databasePath);
}

public class DatabaseAccessFactory(ILoggerFactory loggerFactory) : IDatabaseAccessFactory
{
    public IDatabaseAccess Create(string databasePath)
    {
        return new SqLiteDatabaseAccess(databasePath, loggerFactory);
    }
}

public interface IFilesystemAccessFactory
{
    IFilesystemAccess Create(string filesRootDirectory);
}

public class FilesystemAccessFactory(IFileSystem fileSystem, ILoggerFactory loggerFactory) : IFilesystemAccessFactory
{
    public IFilesystemAccess Create(string filesRootDirectory)
    {
        return new FilesystemAccess(fileSystem, loggerFactory, filesRootDirectory);
    }
}

public interface IFilesWritePermissionCheckerFactory
{
    IFilesWritePermissionChecker Create();
}

public class FilesWritePermissionCheckerFactory(
    IConfigProvider configProvider,
    IFilesystemAccessProvider filesystemAccessProvider) : IFilesWritePermissionCheckerFactory
{
    public IFilesWritePermissionChecker Create()
    {
        return new FilesWritePermissionChecker(configProvider, filesystemAccessProvider);
    }
}

public sealed class ApplicationStartupResult
{
    public bool Succeeded { get; }
    public ApplicationFilePaths? FilePaths { get; }
    public Config? Config { get; }
    public IDatabaseAccess? DatabaseAccess { get; }
    public IFilesystemAccess? FilesystemAccess { get; }
    public IReadOnlyList<INotification> Notifications { get; }
    public ValidationResult? ValidationResult { get; }
    public string? ErrorMessage { get; }

    private ApplicationStartupResult(
        bool succeeded,
        ApplicationFilePaths? filePaths,
        Config? config,
        IDatabaseAccess? databaseAccess,
        IFilesystemAccess? filesystemAccess,
        IReadOnlyList<INotification> notifications,
        ValidationResult? validationResult,
        string? errorMessage)
    {
        Succeeded = succeeded;
        FilePaths = filePaths;
        Config = config;
        DatabaseAccess = databaseAccess;
        FilesystemAccess = filesystemAccess;
        Notifications = notifications;
        ValidationResult = validationResult;
        ErrorMessage = errorMessage;
    }

    public static ApplicationStartupResult Success(
        ApplicationFilePaths filePaths,
        Config config,
        IDatabaseAccess databaseAccess,
        IFilesystemAccess filesystemAccess,
        IReadOnlyList<INotification> notifications)
    {
        return new(true, filePaths, config, databaseAccess, filesystemAccess, notifications, null, null);
    }

    public static ApplicationStartupResult Failure(
        IReadOnlyList<INotification> notifications,
        ValidationResult? validationResult = null,
        string? errorMessage = null)
    {
        return new(false, null, null, null, null, notifications, validationResult, errorMessage);
    }
}

public interface IApplicationStartupService
{
    Task<ApplicationStartupResult> StartAsync(string configPath);
}

public class ApplicationStartupService(
    IFileSystem fileSystem,
    IDatabaseAccessFactory databaseAccessFactory,
    IFilesystemAccessFactory filesystemAccessFactory,
    IDatabaseMigrationStartupCoordinator migrationStartupCoordinator,
    IConfigUpdater configUpdater,
    IFilesWritePermissionCheckerFactory writePermissionCheckerFactory) : IApplicationStartupService
{
    private const string DatabaseFileExtension = ".db";

    public async Task<ApplicationStartupResult> StartAsync(string configPath)
    {
        if (!Path.IsPathFullyQualified(configPath))
        {
            configPath = Path.GetFullPath(configPath);
        }

        if (!configPath.EndsWith(App.ConfigFileExtension))
        {
            return ApplicationStartupResult.Failure(
                [],
                errorMessage: string.Format(Strings.AppInvalidCommandLineArgument, configPath));
        }

        var filesRootDirectory = Path.GetDirectoryName(configPath)!;
        var databaseFilename = Path.GetFileNameWithoutExtension(configPath) + DatabaseFileExtension;
        var databasePath = Path.Combine(filesRootDirectory, databaseFilename);
        var applicationFilePaths = new ApplicationFilePaths(filesRootDirectory, configPath, databasePath);
        var notifications = new List<INotification>();

        Config config;
        if (fileSystem.File.Exists(configPath))
        {
            var parsedConfig = configPath.FromJson<Config>(fileSystem);
            if (parsedConfig is null)
            {
                config = DefaultConfigs.Default;
                notifications.Add(new CollectionGetStartedNotification());
            }
            else
            {
                config = new ConfigMigrator().Migrate(parsedConfig, DefaultConfigs.Default);
            }
        }
        else
        {
            config = DefaultConfigs.Default;
            notifications.Add(new CollectionGetStartedNotification());
        }

        var validationResult = new ConfigValidator().Validate(config);
        if (!validationResult.IsValid)
        {
            return ApplicationStartupResult.Failure(notifications, validationResult);
        }

        var databaseAccess = fileSystem.File.Exists(databasePath)
            ? databaseAccessFactory.Create(databasePath)
            : new NoDatabaseAccess();

        if (!await migrationStartupCoordinator.TryHandleMigrationAsync(
                databaseAccess,
                applicationFilePaths.DatabasePath,
                config.ReadOnly,
                notifications))
        {
            return ApplicationStartupResult.Failure(notifications);
        }

        var filesystemAccess = filesystemAccessFactory.Create(filesRootDirectory);
        configUpdater.InitConfig(applicationFilePaths, config, databaseAccess, filesystemAccess);

        if (!config.ReadOnly && !writePermissionCheckerFactory.Create().HasWritePermission)
        {
            config = config with { ReadOnly = true };
            configUpdater.UpdateConfig(config);
            notifications.Add(new CollectionNoWritePermissionNotification());
        }

        return ApplicationStartupResult.Success(
            applicationFilePaths,
            config,
            databaseAccess,
            filesystemAccess,
            notifications);
    }
}
