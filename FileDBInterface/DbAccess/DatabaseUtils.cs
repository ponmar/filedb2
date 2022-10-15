using System.Data.SQLite;
using System.IO;
using Dapper;
using FileDBInterface.Exceptions;
using System.Data;
using System;

namespace FileDBInterface.DbAccess
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

        public const string DatabaseCreationSql = @"
create table files(
    Id integer primary key autoincrement not null,
    Path text unique not null, /* Format: path/to/file/filename */
    Description text, /* Format: May contain \n line-endings */
    Datetime varchar(19), /* Format: YYYY, YYYY-MM-DD or YYYY-MM-DDTHH:MM:SS */
    Position text, /* Format: <latitude> <longitude> */
    Orientation integer // Format: null (no orientation set), 1-8 according to Exif
);

create table persons(
    Id integer primary key autoincrement not null,
    Firstname text not null,
    Lastname text not null,
    Description text,
    DateOfBirth varchar(10), /* Format: YYYY-MM-DD */
    Deceased varchar(10), /* Format: YYYY-MM-DD */
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

        public static double CalculateDistance(double point1Lat, double point1Lon, double point2Lat, double point2Long)
        {
            var d1 = point1Lat * (Math.PI / 180.0);
            var num1 = point1Lon * (Math.PI / 180.0);
            var d2 = point2Lat * (Math.PI / 180.0);
            var num2 = point2Long * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }

        public static int GetYearsAgo(DateTime now, DateTime dateTime)
        {
            int yearsAgo = now.Year - dateTime.Year;

            try
            {
                if (new DateTime(dateTime.Year, now.Month, now.Day) < dateTime)
                {
                    yearsAgo--;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Current date did not exist the year that person was born
            }

            return yearsAgo;
        }

        public static int GetDaysToNextBirthday(DateTime birthday)
        {
            var today = DateTime.Today;
            var next = birthday.AddYears(today.Year - birthday.Year);

            if (next < today)
            {
                next = next.AddYears(1);
            }

            return (next - today).Days;
        }
    }
}
