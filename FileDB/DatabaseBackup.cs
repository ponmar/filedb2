using System;
using System.Collections.Generic;
using System.IO;

namespace FileDB
{
    public class BackupFile
    {
        public string Filename { get; }

        public DateTime Timestamp { get; }

        public TimeSpan Age => DateTime.Now - Timestamp;

        public BackupFile(string filename, DateTime timestamp)
        {
            Filename = filename;
            Timestamp = timestamp;
        }
    }

    public class DatabaseBackup
    {
        public string BackupDirectory => Path.GetDirectoryName(Utils.Config.Database);

        private const string BackupFileTimestampFormat = "yyyy-MM-ddTHHmmss";

        public List<BackupFile> ListAvailableBackupFiles()
        {
            var backupFiles = new List<BackupFile>();

            foreach (var filePath in Directory.GetFiles(BackupDirectory, "backup_*.db"))
            {
                var filenameParts = filePath.Split("_");
                if (filenameParts.Length >= 2)
                {
                    var timestampString = filenameParts[filenameParts.Length - 1].Replace(".db", "");
                    try
                    {
                        var timestamp = DateTime.ParseExact(timestampString, BackupFileTimestampFormat, null);
                        backupFiles.Add(new BackupFile(filePath, timestamp));
                    }
                    catch (FormatException)
                    {
                    }
                }
            }

            return backupFiles;
        }

        public void CreateBackup()
        {
            var db = Utils.Config.Database;
            if (!File.Exists(db))
            {
                throw new IOException($"Database to backup does not exist: {db}");
            }

            var directoryPath = Path.GetDirectoryName(db);
            var timestamp = DateTime.Now.ToString(BackupFileTimestampFormat);
            var backupFilename = $"backup_{timestamp}.db";
            var backupFilePath = Path.Combine(directoryPath, backupFilename);

            if (File.Exists(backupFilePath))
            {
                throw new IOException($"Backup file already exists: {backupFilePath}");
            }

            File.Copy(db, backupFilePath);
        }
    }
}
