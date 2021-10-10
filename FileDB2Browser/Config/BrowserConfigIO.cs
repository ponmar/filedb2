using System;
using System.IO;
using Newtonsoft.Json;

namespace FileDB2Browser.Config
{
    public class BrowserConfigIO
    {
        private const string ApplicationDataSubDir = Utils.FileDB2BrowserTitle;
        private const string Filename = "FileDB2BrowserConfig.json";

        public static BrowserConfig GetDefaultConfig()
        {
            return new BrowserConfig(
                "filedb2.db",
                "files",
                new() { "Thumbs.db", "filedb.db", "unsorted", "TN_" },
                new() { ".jpg", ".png", ".bmp", ".gif", ".avi", ".mpg", ".mp4", ".mkv", ".mov", ".pdf" },
                false,
                TimeSpan.FromSeconds(3),
                4);
        }

        public static bool Write(BrowserConfig config)
        {
            string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);

            try
            {
                var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var directory = Path.Combine(baseDirectory, ApplicationDataSubDir);
                Directory.CreateDirectory(directory);
                var filePath = Path.Combine(directory, Filename);
                File.WriteAllText(filePath, jsonString);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public static BrowserConfig Read()
        {
            var filePath = GetFilePath();
            if (File.Exists(filePath))
            {
                try
                {
                    var jsonString = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<BrowserConfig>(jsonString);
                }
                catch (JsonException)
                {
                }
                catch (IOException)
                {
                }
            }

            return GetDefaultConfig();
        }

        public static bool FileExists()
        {
            var filePath = GetFilePath();
            return File.Exists(filePath);
        }

        public static string GetFilePath()
        {
            var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var directory = Path.Combine(baseDirectory, ApplicationDataSubDir);
            return Path.Combine(directory, Filename);
        }

        public static bool ResetConfiguration()
        {
            return Write(GetDefaultConfig());
        }
    }
}
