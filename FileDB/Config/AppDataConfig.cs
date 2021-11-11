using System;
using System.IO;
using Newtonsoft.Json;

namespace FileDB.Config
{
    public class AppDataConfig<T>
    {
        private string AppName { get; }
        private string Filename { get; }

        public AppDataConfig(string appName)
        {
            AppName = appName;
            Filename = typeof(T).Name + ".json";
        }

        public bool Write(T config)
        {
            string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);

            try
            {
                var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var directory = Path.Combine(baseDirectory, AppName);
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
