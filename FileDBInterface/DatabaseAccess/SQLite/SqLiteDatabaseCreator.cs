using System.Data.SQLite;
using System.Data;
using Dapper;

namespace FileDBInterface.DatabaseAccess.SQLite;

public class SqLiteDatabaseCreator : IDatabaseCreator
{
    public void CreateDatabase(string databasePath)
    {
        SQLiteConnection.CreateFile(databasePath);
        using var connection = CreateConnection(databasePath);
        connection.Query(DatabaseCreationSql);
        connection.Execute($"pragma user_version = {SqLiteDatabaseMigrator.SupportedVersion};");
    }

    public IDbConnection CreateInMemory()
    {
        var connectionString = "Data Source=:memory:;foreign keys=true";
        var connection = new SQLiteConnection(connectionString);
        connection.Open();
        connection.Query(DatabaseCreationSql);
        connection.Execute($"pragma user_version = {SqLiteDatabaseMigrator.SupportedVersion};");
        return connection;
    }

    internal static IDbConnection CreateConnection(string database)
    {
        var connectionString = database.Contains('=')
            ? database
            : $"Data Source={database};foreign keys = true";
        return new SQLiteConnection(connectionString);
    }

    public const string DatabaseCreationSql = @"
create table files(
    Id integer primary key autoincrement not null,
    Path text unique not null, /* Format: path/to/file/filename */
    Description text, /* Format: May contain \n line-endings */
    Datetime varchar(19), /* Format: YYYY, YYYY-MM, YYYY-MM-DD or YYYY-MM-DDTHH:MM:SS */
    Position text, /* Format: <latitude> <longitude> */
    Orientation integer /* Format: null (no orientation set), 1-8 according to Exif */
);

create table persons(
    Id integer primary key autoincrement not null,
    ShortName text not null,
    FullName text not null,
    Description text,
    DateOfBirth varchar(10), /* Format: YYYY-MM-DD, YYYY-MM, or YYYY */
    Deceased varchar(10), /* Format: YYYY-MM-DD, YYYY-MM, or YYYY */
    ProfileFileId integer references files(Id) on delete set null,
    Sex integer not null default 0 /* Values according to ISO/IEC 5218 (0=Not known, 1=Male, 2=Female, 9=Not applicable) */
);

create table locations(
    Id integer primary key autoincrement not null,
    Name text unique not null,
    Description text,
    Position text /* Format: <latitude> <longitude> */
);

create table tags(
    Id integer primary key autoincrement not null,
    Name text unique not null
);

create table filepersons(
    FileId integer references files(Id) on delete cascade,
    PersonId integer references persons(Id) on delete cascade,
    BBoxX real, /* Format: null or value in range [0.0, 1.0] - normalized x coordinate relative to rotated image dimensions */
    BBoxY real, /* Format: null or value in range [0.0, 1.0] - normalized y coordinate relative to rotated image dimensions */
    BBoxWidth real, /* Format: null or value in range [0.0, 1.0] - normalized width relative to rotated image dimensions */
    BBoxHeight real, /* Format: null or value in range [0.0, 1.0] - normalized height relative to rotated image dimensions */
    primary key(FileId, PersonId)
);

create table filelocations(
    FileId integer references files(Id) on delete cascade,
    LocationId integer references locations(Id) on delete cascade,
    primary key(FileId, LocationId)
);

create table filetags(
    FileId integer references files(Id) on delete cascade,
    TagId integer references tags(Id) on delete cascade,
    primary key(FileId, TagId)
);
";
}
