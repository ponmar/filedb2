using System.Data.SQLite;
using System.Data;
using Dapper;

namespace FileDBInterface.DatabaseAccess;

public static class DatabaseSetup
{
    public static void CreateDatabase(string databasePath)
    {
        SQLiteConnection.CreateFile(databasePath);
        using var connection = CreateConnection(databasePath);
        connection.Query(DatabaseCreationSql);
    }

    internal static IDbConnection CreateConnection(string databasePath)
    {
        var connectionString = $"Data Source={databasePath};foreign keys = true";
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
    Firstname text not null,
    Lastname text not null,
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
