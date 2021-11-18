using System.Data.SQLite;
using System.IO;
using Dapper;
using FileDBInterface.Exceptions;
using System.Data;
using System;
using System.Globalization;
using MetadataExtractor;

namespace FileDBInterface
{
    public static class DatabaseUtils
    {
        public static void CreateDatabase(string database)
        {
            if (File.Exists(database))
            {
                throw new DatabaseWrapperException($"Database already created: {database}");
            }

            SQLiteConnection.CreateFile(database);
            using var connection = CreateConnection(database);
            connection.Query(DatabaseCreationSql);
        }

        internal static IDbConnection CreateConnection(string database)
        {
            var connectionString = $"Data Source={database};foreign keys = true";
            return new SQLiteConnection(connectionString);
        }

        private const string DatabaseCreationSql = @"
create table files(
    id integer primary key autoincrement not null,
    path text unique not null, /* Format: path/to/file/filename */
    description text,
    datetime varchar(19), /* Format: YYYY, YYYY-MM-DD or YYYY-MM-DDTHH:MM:SS */
    position text /* Format: <latitude> <longitude> */
);

create table persons(
    id integer primary key autoincrement not null,
    firstname text not null,
    lastname text not null,
    description text,
    dateofbirth varchar(10), /* Format: YYYY-MM-DD */
    deceased varchar(10), /* Format: YYYY-MM-DD */
    profilefileid integer references files(id) on delete set null,
    sex integer not null default 0 /* Values according to ISO/IEC 5218 (0=Not known, 1=Male, 2=Female, 9=Not applicable) */
);

create table locations(
    id integer primary key autoincrement not null,
    name text unique not null,
    description text,
    position text /* Format: <latitude> <longitude> */
);

create table tags(
    id integer primary key autoincrement not null,
    name text unique not null
);

create table filepersons(
    fileid integer references files(id) on delete cascade,
    personid integer references persons(id) on delete cascade,
    primary key(fileid, personid)
);

create table filelocations(
    fileid integer references files(id) on delete cascade,
    locationid integer references locations(id) on delete cascade,
    primary key(fileid, locationid)
);

create table filetags(
    fileid integer references files(id) on delete cascade,
    tagid integer references tags(id) on delete cascade,
    primary key(fileid, tagid)
);
";

        public static DateTime ParseDateOfBirth(string dateOfBirthStr)
        {
            return DateTime.ParseExact(dateOfBirthStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static DateTime ParseDeceased(string deceasedStr)
        {
            return DateTime.ParseExact(deceasedStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static (double lat, double lon)? ParseGpsPosition(string positionString)
        {
            var positionParts = positionString.Split(" ");
            if (positionParts.Length != 2)
            {
                return null;
            }

            if (!double.TryParse(positionParts[0], out var latitude))
            {
                return null;
            }

            if (!double.TryParse(positionParts[1], out var longitude))
            {
                return null;
            }

            return (latitude, longitude);
        }
    }
}
