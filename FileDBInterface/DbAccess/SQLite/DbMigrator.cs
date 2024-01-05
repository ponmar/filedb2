using Dapper;
using System;

namespace FileDBInterface.DbAccess.SQLite;

public class DbMigrator(string dbPath)
{
    // Note: add migration code below when the version is increased
    private const int SupportedVersion = 0;

    private readonly string dbPath = dbPath;

    public void Migrate()
    {
        for (var dbVersion = GetDatabaseVersion(); dbVersion < SupportedVersion; dbVersion++)
        {
            MigrateIteration(dbVersion + 1);
        }
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
