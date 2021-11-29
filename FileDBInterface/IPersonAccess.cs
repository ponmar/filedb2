using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDBInterface
{
    public interface IPersonAccess
    {
        public IEnumerable<PersonModel> GetPersons();
        public int GetPersonCount();
        public PersonModel GetPersonById(int id);
        public bool HasPersonId(int id);
        public void InsertPerson(string firstname, string lastname, string description = null, string dateOfBirth = null, string deceased = null, int? profileFileId = null, Sex sex = Sex.NotApplicable);
        public void UpdatePerson(int id, string firstname, string lastname, string description = null, string dateOfBirth = null, string deceased = null, int? profileFileId = null, Sex sex = Sex.NotApplicable);
        public void DeletePerson(int id);

    }
}
