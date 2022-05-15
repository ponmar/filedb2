using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using FileDB.Comparers;
using FileDB.Configuration;
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
        public ObservableCollection<DeceasedPerson> Persons { get; set; } = new();

        private readonly Model.Model model = Model.Model.Instance;

        public RipViewModel()
        {
            UpdatePersons();
            model.PersonsUpdated += Model_PersonsUpdated;
        }

        private void Model_PersonsUpdated(object sender, EventArgs e)
        {
            UpdatePersons();
        }

        private void UpdatePersons()
        {
            var persons = new List<DeceasedPerson>();
            var configDir = new AppDataConfig<Config>(Utils.ApplicationName).ConfigDirectory;
            var cacheDir = Path.Combine(configDir, DefaultConfigs.CacheSubdir);

            foreach (var person in model.DbAccess.GetPersons())
            {
                if (person.DateOfBirth != null && person.Deceased != null)
                {
                    var dateOfBirth = DatabaseParsing.ParsePersonsDateOfBirth(person.DateOfBirth);
                    var deceased = DatabaseParsing.ParsePersonsDeceased(person.Deceased);

                    string profileFileIdPath;
                    if (person.ProfileFileId != null)
                    {
                        if (model.Config.CacheFiles)
                        {
                            profileFileIdPath = Path.Combine(cacheDir, $"{person.ProfileFileId.Value}");
                        }
                        else
                        {
                            var profileFile = model.DbAccess.GetFileById(person.ProfileFileId.Value);
                            profileFileIdPath = model.FilesystemAccess.ToAbsolutePath(profileFile.Path);
                        }
                    }
                    else
                    {
                        profileFileIdPath = string.Empty;
                    }

                    persons.Add(new DeceasedPerson()
                    {
                        Name = person.Firstname + " " + person.Lastname,
                        DateOfBirth = person.DateOfBirth,
                        DeceasedStr = person.Deceased,
                        Deceased = deceased,
                        Age = DatabaseUtils.GetYearsAgo(deceased, dateOfBirth),
                        ProfileFileIdPath = profileFileIdPath,
                    });
                }
            }

            persons.Sort(new PersonsByDeceasedSorter());
            persons.Reverse();

            Persons.Clear();
            persons.ForEach(x => Persons.Add(x));
        }
    }
}
