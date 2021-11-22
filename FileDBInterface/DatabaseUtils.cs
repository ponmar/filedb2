using System.Data.SQLite;
using System.IO;
using Dapper;
using FileDBInterface.Exceptions;
using System.Data;
using System;

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
                next = next.AddYears(1);

            return (next - today).Days;
        }
    }
}
