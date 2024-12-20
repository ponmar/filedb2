﻿using Dapper;
using System;
using System.Collections.Generic;

namespace FileDBInterface.DatabaseAccess.SQLite;

public record DatabaseMigrationResult(int FromVersion, int ToVersion, Exception? Exception = null);

public class DatabaseMigrator(string dbPath)
{
    // Note: add migration code below when the version is increased
    private const int SupportedVersion = 0;

    public bool NeedsMigration => GetDatabaseVersion() < SupportedVersion;

    public List<DatabaseMigrationResult> Migrate()
    {
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
        using var connection = DatabaseSetup.CreateConnection(dbPath);
        return connection.ExecuteScalar<int>("pragma user_version;");
    }

    private void MigrateIteration(int newVersion)
    {
        using var connection = DatabaseSetup.CreateConnection(dbPath);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        switch (newVersion)
        {
            case 0:
                // First version (no migration needed)
                break;

            default:
                throw new NotSupportedException($"Unable to migrate database {dbPath} to unknown database version {newVersion}");
        }

        // Set new version
        connection.Execute($"pragma user_version = {newVersion};", transaction: transaction);

        transaction.Commit();
    }
}
