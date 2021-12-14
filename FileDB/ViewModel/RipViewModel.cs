using System;
using System.Collections.Generic;
using FileDB.Comparers;
using FileDBInterface.DbAccess;

namespace FileDB.ViewModel
{
    public class DeceasedPerson
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public string DeceasedStr { get; set; }
        public DateTime Deceased { get; set; }
        public int Age { get; set; }
        public string ProfileFileIdPath { get; set; }
    }

    public class RipViewModel : ViewModelBase
    {
        public List<DeceasedPerson> Persons { get; set; } = new();

        private readonly Model.Model model = Model.Model.Instance;

        public RipViewModel()
        {
            var persons = model.DbAccess.GetPersons();
            foreach (var person in persons)
            {
                if (person.DateOfBirth != null && person.Deceased != null)
                {
                    var dateOfBirth = DatabaseParsing.ParsePersonsDateOfBirth(person.DateOfBirth);
                    var deceased = DatabaseParsing.ParsePersonsDeceased(person.Deceased);

                    Persons.Add(new DeceasedPerson()
                    {
                        Name = person.Firstname + " " + person.Lastname,
                        DateOfBirth = person.DateOfBirth,
                        DeceasedStr = person.Deceased,
                        Deceased = deceased,
                        Age = DatabaseUtils.GetYearsAgo(deceased, dateOfBirth),
                        ProfileFileIdPath = person.ProfileFileId != null ? model.FilesystemAccess.ToAbsolutePath(model.DbAccess.GetFileById(person.ProfileFileId.Value).Path) : string.Empty,
                    }) ;
                }
            }

            Persons.Sort(new PersonsByDeceasedSorter());
            Persons.Reverse();
        }
    }
}
