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

            /*
            var persons = data.Persons.Where(x => x.DateOfBirth != null && x.Deceased == null).Select(x => new Person(x)).ToList();
            persons.Sort(new PersonsByDaysLeftUntilBirthdaySorter());
            persons.ForEach(x => Persons.Add(x));
            */
        }
    }
}
