using Dapper;
using FileDBInterface.DatabaseAccess.SQLite;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Xunit;

namespace FileDBInterface.Tests.DatabaseAccess.SQLite;

public class SqLiteDatabaseMigratorTests : IDisposable
{
    private readonly string databasePath;

    public SqLiteDatabaseMigratorTests()
    {
        databasePath = Path.Combine(Path.GetTempPath(), $"filedb-migrator-tests-{Guid.NewGuid():N}.db");
    }

    public void Dispose()
    {
        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }
    }

    private static IDbConnection OpenConnection(string path)
    {
        var connection = new SQLiteConnection($"Data Source={path};foreign keys=true");
        connection.Open();
        return connection;
    }

    [Fact]
    public void Migrate_LegacyVersion0Database_MigratesToSupportedVersion()
    {
        SQLiteConnection.CreateFile(databasePath);
        using (var setupConnection = OpenConnection(databasePath))
        {
            setupConnection.Execute("""
                create table persons(
                    Id integer primary key autoincrement not null,
                    Firstname text not null,
                    Lastname text not null
                );

                create table filepersons(
                    FileId integer not null,
                    PersonId integer not null,
                    primary key(FileId, PersonId)
                );
                """);
            setupConnection.Execute("insert into persons (Firstname, Lastname) values ('John', 'Doe')");
            setupConnection.Execute("pragma user_version = 0;");
        }

        var migrator = new SqLiteDatabaseMigrator(databasePath);
        var migrationResult = migrator.Migrate();

        Assert.Equal(2, migrationResult.Count);
        Assert.Equal(0, migrationResult[0].FromVersion);
        Assert.Equal(1, migrationResult[0].ToVersion);
        Assert.Null(migrationResult[0].Exception);
        Assert.Equal(1, migrationResult[1].FromVersion);
        Assert.Equal(2, migrationResult[1].ToVersion);
        Assert.Null(migrationResult[1].Exception);

        using var verifyConnection = OpenConnection(databasePath);
        var dbVersion = verifyConnection.ExecuteScalar<int>("pragma user_version;");
        Assert.Equal(SqLiteDatabaseMigrator.SupportedVersion, dbVersion);

        var migratedFullName = verifyConnection.ExecuteScalar<string>("select FullName from persons where Id = 1");
        Assert.Equal("John Doe", migratedFullName);

        var columns = verifyConnection.Query("pragma table_info(filepersons)")
            .Select(x => (string)x.name)
            .ToHashSet();

        Assert.Contains("BBoxX", columns);
        Assert.Contains("BBoxY", columns);
        Assert.Contains("BBoxWidth", columns);
        Assert.Contains("BBoxHeight", columns);
    }

    [Fact]
    public void Migrate_FirstMigrationStepFails_DoesNotContinueWithNextIteration()
    {
        SQLiteConnection.CreateFile(databasePath);
        using (var setupConnection = OpenConnection(databasePath))
        {
            setupConnection.Execute("""
                create table persons(
                    Id integer primary key autoincrement not null,
                    ShortName text not null,
                    FullName text not null
                );

                create table filepersons(
                    FileId integer not null,
                    PersonId integer not null,
                    primary key(FileId, PersonId)
                );
                """);
            setupConnection.Execute("pragma user_version = 0;");
        }

        var migrator = new SqLiteDatabaseMigrator(databasePath);
        var migrationResult = migrator.Migrate();

        Assert.Single(migrationResult);
        Assert.Equal(0, migrationResult[0].FromVersion);
        Assert.Equal(1, migrationResult[0].ToVersion);
        Assert.NotNull(migrationResult[0].Exception);

        using var verifyConnection = OpenConnection(databasePath);
        var dbVersion = verifyConnection.ExecuteScalar<int>("pragma user_version;");
        Assert.Equal(0, dbVersion);

        var columns = verifyConnection.Query("pragma table_info(filepersons)")
            .Select(x => (string)x.name)
            .ToList();

        Assert.DoesNotContain("BBoxX", columns);
        Assert.DoesNotContain("BBoxY", columns);
        Assert.DoesNotContain("BBoxWidth", columns);
        Assert.DoesNotContain("BBoxHeight", columns);
    }

    [Fact]
    public void IsTooNew_WhenVersionExceedsSupportedVersion_ReturnsTrue()
    {
        SQLiteConnection.CreateFile(databasePath);
        using (var setupConnection = new SQLiteConnection($"Data Source={databasePath};foreign keys=true"))
        {
            setupConnection.Open();
            setupConnection.Execute($"pragma user_version = {SqLiteDatabaseMigrator.SupportedVersion + 1};");
        }

        var migrator = new SqLiteDatabaseMigrator(databasePath);

        Assert.True(migrator.IsTooNew);
        Assert.False(migrator.NeedsMigration);
    }

    [Fact]
    public void IsTooNew_WhenVersionEqualsOrBelowSupportedVersion_ReturnsFalse()
    {
        SQLiteConnection.CreateFile(databasePath);
        using (var setupConnection = new SQLiteConnection($"Data Source={databasePath};foreign keys=true"))
        {
            setupConnection.Open();
            setupConnection.Execute($"pragma user_version = {SqLiteDatabaseMigrator.SupportedVersion};");
        }

        var migrator = new SqLiteDatabaseMigrator(databasePath);

        Assert.False(migrator.IsTooNew);
    }
}
