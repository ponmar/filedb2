using FileDB.Dialogs;
using FileDB.Lang;
using FileDB.Notifications;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.DatabaseAccess.SQLite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileDB.Services;

public interface IDatabaseMigrationStartupCoordinator
{
    Task<bool> TryHandleMigrationAsync(IDatabaseAccess dbAccess, string databasePath, bool readOnly, IList<INotification> notifications);
}

public class DatabaseMigrationStartupCoordinator : IDatabaseMigrationStartupCoordinator
{
    private readonly IDialogs dialogs;
    private readonly IFileBackup fileBackup;
    private readonly ILogger<DatabaseMigrationStartupCoordinator> logger;

    public DatabaseMigrationStartupCoordinator(IDialogs dialogs, IFileBackup fileBackup, ILoggerFactory loggerFactory)
    {
        this.dialogs = dialogs;
        this.fileBackup = fileBackup;
        logger = loggerFactory.CreateLogger<DatabaseMigrationStartupCoordinator>();
    }

    public async Task<bool> TryHandleMigrationAsync(IDatabaseAccess dbAccess, string databasePath, bool readOnly, IList<INotification> notifications)
    {
        if (!dbAccess.NeedsMigration)
        {
            return true;
        }

        var currentVersion = dbAccess is SqLiteDatabaseAccess sqLiteAccess ? sqLiteAccess.CurrentVersion : (int?)null;
        logger.LogInformation(
            "Database migration required: {DatabasePath}; CurrentVersion={CurrentVersion}; TargetVersion={TargetVersion}; ReadOnly={ReadOnly}",
            databasePath,
            currentVersion,
            SqLiteDatabaseMigrator.SupportedVersion,
            readOnly);

        if (readOnly)
        {
            logger.LogError(
                "Migration blocked by read-only mode: {DatabasePath}; CurrentVersion={CurrentVersion}; TargetVersion={TargetVersion}",
                databasePath,
                currentVersion,
                SqLiteDatabaseMigrator.SupportedVersion);
            await dialogs.ShowErrorDialogAsync(Strings.AppMigrationRequiredInReadOnlyMode);
            return false;
        }

        try
        {
            fileBackup.CreateBackup(databasePath);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Database backup failed before migration: {DatabasePath}", databasePath);
            await dialogs.ShowErrorDialogAsync(Strings.AppUnableToCreateDatabaseBackupBeforeMigration, e);
            return false;
        }

        var migrationResults = dbAccess.Migrate();
        foreach (var migrationResult in migrationResults)
        {
            if (migrationResult.Exception is null)
            {
                logger.LogInformation(
                    "Database migrated: {DatabasePath}; FromVersion={FromVersion}; ToVersion={ToVersion}",
                    databasePath,
                    migrationResult.FromVersion,
                    migrationResult.ToVersion);
                notifications.Add(new DatabaseMigrationNotification(migrationResult.FromVersion, migrationResult.ToVersion));
                continue;
            }

            logger.LogError(
                migrationResult.Exception,
                "Database migration failed: {DatabasePath}; FromVersion={FromVersion}; ToVersion={ToVersion}",
                databasePath,
                migrationResult.FromVersion,
                migrationResult.ToVersion);

            notifications.Add(new DatabaseMigrationErrorNotification(
                migrationResult.FromVersion,
                migrationResult.ToVersion,
                migrationResult.Exception.Message));

            var message = string.Format(
                Strings.NotificationDatabaseMigrationError,
                migrationResult.FromVersion,
                migrationResult.ToVersion,
                migrationResult.Exception.Message);
            await dialogs.ShowErrorDialogAsync(message);
            return false;
        }

        logger.LogInformation(
            "Database migration completed: {DatabasePath}; CurrentVersion={CurrentVersion}",
            databasePath,
            SqLiteDatabaseMigrator.SupportedVersion);
        return true;
    }
}
