using System;
using System.IO;
using Newtonsoft.Json;

namespace FileDB.Configuration
{
    public class AppDataConfig<T>
    {
        public string AppName { get; }
        public string Filename { get; }
        public string ConfigDirectory { get; }
        public string FilePath { get;  }

        public AppDataConfig(string appName)
        {
            AppName = appName;
            Filename = typeof(T).Name + ".json";

            var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ConfigDirectory = Path.Combine(baseDirectory, AppName);
            FilePath = Path.Combine(ConfigDirectory, Filename);
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
            if (File.Exists(FilePath))
            {
                try
                {
                    var jsonString = File.ReadAllText(FilePath);
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
            return File.Exists(FilePath);
        }
    }
}
