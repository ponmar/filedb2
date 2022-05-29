using FileDBApp.Model;
using Newtonsoft.Json;

namespace FileDBApp.Services
{
    public class PersonService
    {
        public static string DataFilePath => Path.Combine(FileSystem.Current.AppDataDirectory, "Data.json");

        private List<PersonModel> persons = null;

        public async Task<List<PersonModel>> GetPersons()
        {
            if (persons == null)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(DataFilePath);
                    persons = JsonConvert.DeserializeObject<ExportedDatabaseFileFormat>(json).Persons;
                }
                catch
                {
                    persons = new();
                }
            }

            return persons;
        }
    }
}
