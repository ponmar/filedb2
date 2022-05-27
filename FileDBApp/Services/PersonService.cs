using FileDBApp.Model;
using Newtonsoft.Json;

namespace FileDBApp.Services
{
    public class PersonService
    {
        public async Task<List<PersonModel>> GetPersons()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("DatabaseExport.json");
            using var reader = new StreamReader(stream);
            var contents = reader.ReadToEnd();

            var data = JsonConvert.DeserializeObject<ExportedDatabaseFileFormat>(contents);
            return data.Persons;
        }
    }
}
