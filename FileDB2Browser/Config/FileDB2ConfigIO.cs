using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileDB2Interface;
using Newtonsoft.Json;

namespace FileDB2Browser.Config
{
    public class FileDB2BrowserConfigIO
    {
        private const string ApplicationDataSubDir = "FileDB2";
        private const string Filename = "FileDB2BrowserConfig.json";

        public static bool Write(FileDB2BrowserConfig config)
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

        public static FileDB2BrowserConfig Read()
        {
            var filePath = GetFilePath();
            if (File.Exists(filePath))
            {
                try
                {
                    var jsonString = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<FileDB2BrowserConfig>(jsonString);
                }
                catch (JsonException)
                {
                }
                catch (IOException)
                {
                }
            }

            return new FileDB2BrowserConfig();
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

        public static string GetServerLogoFilePath(string filename)
        {
            var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var directory = Path.Combine(baseDirectory, ApplicationDataSubDir);
            return Path.Combine(directory, filename);
        }
    }
}
