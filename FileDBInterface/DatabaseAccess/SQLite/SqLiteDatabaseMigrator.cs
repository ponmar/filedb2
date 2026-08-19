using System;
using System.Collections.Generic;
using Dapper;
using FileDBInterface.Model;

namespace FileDBInterface.DatabaseAccess.SQLite;

public record DatabaseMigrationResult(int FromVersion, int ToVersion, Exception? Exception = null);

public class SqLiteDatabaseMigrator(string dbPath)
{
    // Note: add migration code below when the version is increased
    public const int SupportedVersion = 2;

    public int CurrentVersion => GetDatabaseVersion();

    public bool NeedsMigration => CurrentVersion < SupportedVersion;

    public bool IsTooNew => CurrentVersion > SupportedVersion;

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
        using var connection = SqLiteDatabaseCreator.CreateMigrationConnection(dbPath);
        return connection.ExecuteScalar<int>("pragma user_version;");
    }

    private void MigrateIteration(int newVersion)
    {
        using var connection = SqLiteDatabaseCreator.CreateMigrationConnection(dbPath);

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
                foreach (var person in connection.Query<PersonModel>("select * from [persons]"))
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
}
