using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using Dapper;
using FileDB2Interface.Exceptions;
using System.Data;

namespace FileDB2Interface
{
    public static class FileDB2Utils
    {
        public static void CreateDatabase(string database)
        {
            if (File.Exists(database))
            {
                throw new FileDB2Exception($"Database already created: {database}");
            }

            var scriptPath = "filedb2.sql";
            string sql = null;
            try
            {
                sql = File.ReadAllText(scriptPath);
            }
            catch (IOException e)
            {
                throw new FileDB2Exception("Unable to load database creation script", e);
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
