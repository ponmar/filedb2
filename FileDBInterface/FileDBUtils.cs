using System.Data.SQLite;
using System.IO;
using Dapper;
using FileDBInterface.Exceptions;
using System.Data;

namespace FileDBInterface
{
    public static class FileDBUtils
    {
        public static void CreateDatabase(string database)
        {
            if (File.Exists(database))
            {
                throw new FileDBException($"Database already created: {database}");
            }

            var scriptPath = "filedb.sql";
            string sql = null;
            try
            {
                sql = File.ReadAllText(scriptPath);
            }
            catch (IOException e)
            {
                throw new FileDBException("Unable to load database creation script", e);
            }

            SQLiteConnection.CreateFile(database);
            using var connection = CreateConnection(database);
            connection.Query(sql);
        }

        internal static IDbConnection CreateConnection(string database)
        {
            var connectionString = $"Data Source={database};foreign keys = true";
            return new SQLiteConnection(connectionString);
        }
    }
}
