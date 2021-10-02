using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SQLite;

namespace FileDB2Interface
{
    public static class SqLiteUtils
    {
        public static IDbConnection CreateConnection(string database)
        {
            var connectionString = $"Data Source={database};foreign keys = true";
            return new SQLiteConnection(connectionString);
        }
    }
}
