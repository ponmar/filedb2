using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using FileDB.Comparers;
using FileDB.Configuration;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModel
{
    public class DeceasedPerson
    {
        public string Name => $"{person.Firstname} {person.Lastname}";
        public string DateOfBirth => person.DateOfBirth;
        public string DeceasedStr => person.Deceased;
        public DateTime Deceased => DatabaseParsing.ParsePersonsDeceased(person.Deceased);
        public int Age => DatabaseUtils.GetYearsAgo(Deceased, DatabaseParsing.ParsePersonsDateOfBirth(person.DateOfBirth));
        public string ProfileFileIdPath { get; }

        private readonly PersonModel person;

        public DeceasedPerson(PersonModel person, string profileFileIdPath)
        {
            this.person = person;
            ProfileFileIdPath = profileFileIdPath;
        }

        public bool MatchesTextFilter(string textFilter)
        {
            return string.IsNullOrEmpty(textFilter) || 
                Name.Contains(textFilter, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(person.Description) && person.Description.Contains(textFilter, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class RipViewModel : ViewModelBase
    {
        public string FilterText
        {
            get => filterText;
            set
            {
                if (SetProperty(ref filterText, value))
                {
                    FilterPersons();
                }
            }
        }
        private string filterText;

        private readonly List<DeceasedPerson> allPersons = new();

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
            allPersons.Clear();
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

                    allPersons.Add(new DeceasedPerson(person, profileFileIdPath));
                }
            }

            allPersons.Sort(new PersonsByDeceasedSorter());
            allPersons.Reverse();

            FilterPersons();
        }

        private void FilterPersons()
        {
            Persons.Clear();

            foreach (var person in allPersons)
            {
                if (person.MatchesTextFilter(FilterText))
                {
                    Persons.Add(person);
                }
            }
        }
    }
}
