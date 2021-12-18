using System;
using System.IO;
using Newtonsoft.Json;

namespace FileDB.Configuration
{
    public class AppDataConfig<T>
    {
        public string AppName { get; }
        public string Filename { get; }

        public string FilePath { get; private set; }

        public AppDataConfig(string appName)
        {
            AppName = appName;
            Filename = typeof(T).Name + ".json";

            var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var directory = Path.Combine(baseDirectory, AppName);
            FilePath = Path.Combine(directory, Filename);
        }

        public bool Write(T config)
        {
            string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);

            try
            {
                var directory = Path.GetDirectoryName(FilePath);
                Directory.CreateDirectory(directory);
                File.WriteAllText(FilePath, jsonString);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public T Read()
        {
            var filePath = GetFilePath();
            if (File.Exists(filePath))
            {
                try
                {
                    var jsonString = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<T>(jsonString);
                }
                catch (JsonException)
                {
                }
                catch (IOException)
                {
                }
            }

            return default;
        }

        public bool FileExists()
        {
            var filePath = GetFilePath();
            return File.Exists(filePath);
        }

        public string GetFilePath()
        {
            var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var directory = Path.Combine(baseDirectory, AppName);
            return Path.Combine(directory, Filename);
        }
    }
}
