using System.Collections.Generic;
using FileDBShared.Model;

namespace FileDBInterface.DatabaseAccess;

public interface IPersonAccess
{
    IEnumerable<PersonModel> GetPersons();
    int GetPersonCount();
    PersonModel GetPersonById(int id);
    bool HasPersonId(int id);
    void InsertPerson(PersonModel person);
    void UpdatePerson(PersonModel person);
    void DeletePerson(int id);

}
