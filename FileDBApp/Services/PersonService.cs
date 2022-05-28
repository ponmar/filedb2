using FileDBApp.Model;
using Newtonsoft.Json;

namespace FileDBApp.Services
{
    public class PersonService
    {
        private List<PersonModel> persons = null;

        public async Task<List<PersonModel>> GetPersons()
        {
            if (persons == null)
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("DatabaseExport.json");
                using var reader = new StreamReader(stream);
                var contents = reader.ReadToEnd();

                persons = JsonConvert.DeserializeObject<ExportedDatabaseFileFormat>(contents).Persons;
            }

            return persons;
        }
    }
}
