using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDBInterface.Access
{
    public interface IPersonAccess
    {
        public IEnumerable<PersonModel> GetPersons();
        public int GetPersonCount();
        public PersonModel GetPersonById(int id);
        public bool HasPersonId(int id);
        public void InsertPerson(PersonModel person);
        public void UpdatePerson(PersonModel person);
        public void DeletePerson(int id);

    }
}
