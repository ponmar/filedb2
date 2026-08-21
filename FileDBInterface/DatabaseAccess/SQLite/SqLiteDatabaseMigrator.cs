using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Dapper;
using FileDBInterface.Model;
using Microsoft.Extensions.Logging;

namespace FileDBInterface.DatabaseAccess.SQLite;

public record DatabaseMigrationResult(int FromVersion, int ToVersion, Exception? Exception = null);

public class SqLiteDatabaseMigrator(string dbPath, ILogger? logger = null)
{
    // Note: add migration code below when the version is increased
    public const int SupportedVersion = 2;
    private const int MigrationBusyTimeoutMs = 30000;

    public int CurrentVersion => GetDatabaseVersion();

    public bool NeedsMigration => CurrentVersion < SupportedVersion;

    public bool IsTooNew => CurrentVersion > SupportedVersion;

    public List<DatabaseMigrationResult> Migrate()
    {
        WarnIfStaleLockFiles();

        // Clear all pooled connections in this process so no stale handle can
        // block the write transaction that migration requires.
        SQLiteConnection.ClearAllPools();

        var result = new List<DatabaseMigrationResult>();
        for (var dbVersion = GetDatabaseVersion(); dbVersion < SupportedVersion; dbVersion++)
        {
            var toVersion = dbVersion + 1;
            try
            {
                MigrateIteration(toVersion);
                result.Add(new(dbVersion, toVersion));
            }
            catch (Exception e)
            {
                result.Add(new(dbVersion, toVersion, e));
                break;
            }
        }
        return result;
    }

    private int GetDatabaseVersion()
    {
        using var connection = SqLiteDatabaseCreator.CreateMigrationConnection(dbPath);
        return connection.ExecuteScalar<int>("pragma user_version;");
    }

    private void MigrateIteration(int newVersion)
    {
        using var connection = SqLiteDatabaseCreator.CreateMigrationConnection(dbPath);
        connection.Execute($"pragma busy_timeout = {MigrationBusyTimeoutMs};");

        using var transaction = connection.BeginTransaction();
        switch (newVersion)
        {
            case 0:
                // First version (no migration needed)
                break;

            case 1:
                // Persons table update: replace Firstname and Lastname with Shortname and Fullname
                connection.Execute("ALTER TABLE Persons RENAME COLUMN Firstname TO ShortName", transaction: transaction);
                connection.Execute("ALTER TABLE Persons RENAME COLUMN Lastname TO FullName", transaction: transaction);
                var persons = connection.Query<PersonModel>("select * from [persons]", transaction: transaction).ToList();
                foreach (var person in persons)
                {
                    person.FullName = $"{person.ShortName} {person.FullName}";
                    var sql = "update [persons] set FullName = @FullName where Id = @Id";
                    connection.Execute(sql, person, transaction: transaction);
                }
                break;

            case 2:
                // filepersons table: add bounding box columns
                connection.Execute("ALTER TABLE filepersons ADD COLUMN BBoxX real", transaction: transaction);
                connection.Execute("ALTER TABLE filepersons ADD COLUMN BBoxY real", transaction: transaction);
                connection.Execute("ALTER TABLE filepersons ADD COLUMN BBoxWidth real", transaction: transaction);
                connection.Execute("ALTER TABLE filepersons ADD COLUMN BBoxHeight real", transaction: transaction);
                break;

            default:
                throw new NotSupportedException($"Unable to migrate database {dbPath} to unknown database version {newVersion}");
        }

        // Set new version
        connection.Execute($"pragma user_version = {newVersion};", transaction: transaction);

        transaction.Commit();
    }

    private void WarnIfStaleLockFiles()
    {
        var journalPath = dbPath + "-journal";
        var walPath = dbPath + "-wal";
        if (File.Exists(journalPath))
        {
            logger?.LogWarning("Stale SQLite journal file found before migration: {JournalPath}. This may indicate an unclean shutdown and could cause locking issues.", journalPath);
        }
        if (File.Exists(walPath))
        {
            logger?.LogWarning("SQLite WAL file found before migration: {WalPath}. Another process may have the database open in WAL mode.", walPath);
        }
    }
}
