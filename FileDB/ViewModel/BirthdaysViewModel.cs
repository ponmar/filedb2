using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using FileDB.Configuration;
using FileDBInterface.DbAccess;

namespace FileDB.ViewModel
{
    public class PersonBirthday
    {
        public string Name { get; set; }
        public string Birthday { get; set; }
        public int DaysLeft { get; set; }
        public string DaysLeftStr { get; set; }
        public int Age { get; set; }
        public string ProfileFileIdPath { get; set; }
    }

    public class PersonsByDaysLeftUntilBirthdaySorter : IComparer<PersonBirthday>
    {
        public int Compare(PersonBirthday x, PersonBirthday y)
        {
            if (x.DaysLeft == y.DaysLeft)
            {
                return x.Name.CompareTo(y.Name);
            }

            return x.DaysLeft.CompareTo(y.DaysLeft);
        }
    }

    public class BirthdaysViewModel : ViewModelBase
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

        private readonly List<PersonBirthday> allPersons = new();

        public ObservableCollection<PersonBirthday> Persons { get; } = new();

        private readonly Model.Model model = Model.Model.Instance;

        public BirthdaysViewModel()
        {
            UpdatePersons();
            model.PersonsUpdated += Model_PersonsUpdated;
            model.DateChanged += Model_DateChanged;
        }

        private void Model_DateChanged(object sender, EventArgs e)
        {
            UpdatePersons();
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
                if (person.DateOfBirth != null && person.Deceased == null)
                {
                    var dateOfBirth = DatabaseParsing.ParsePersonsDateOfBirth(person.DateOfBirth);

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

                    var p = new PersonBirthday()
                    {
                        Name = person.Firstname + " " + person.Lastname,
                        Birthday = dateOfBirth.ToString("d MMMM"),
                        DaysLeft = DatabaseUtils.GetDaysToNextBirthday(dateOfBirth),
                        Age = DatabaseUtils.GetYearsAgo(DateTime.Now, dateOfBirth),
                        ProfileFileIdPath = profileFileIdPath,
                    };

                    if (p.DaysLeft == 0)
                    {
                        p.DaysLeftStr = $"Turned {p.Age} today!";
                    }
                    else if (p.DaysLeft == 1)
                    {
                        p.DaysLeftStr = $"Turns {p.Age + 1} tomorrow!";
                    }
                    else if (p.DaysLeft <= 14)
                    {
                        p.DaysLeftStr = p.DaysLeft <= 14 ? $"Turns {p.Age + 1} in {p.DaysLeft} days" : string.Empty;
                    }
                    else
                    {
                        p.DaysLeftStr = string.Empty;
                    }

                    allPersons.Add(p);
                }
            }

            allPersons.Sort(new PersonsByDaysLeftUntilBirthdaySorter());

            FilterPersons();
        }

        private void FilterPersons()
        {
            Persons.Clear();

            if (string.IsNullOrEmpty(FilterText))
            {
                allPersons.ForEach(x => Persons.Add(x));
            }
            else
            {
                foreach (var person in allPersons)
                {
                    if (person.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                    {
                        Persons.Add(person);
                    }
                }
            }
        }
    }
}
