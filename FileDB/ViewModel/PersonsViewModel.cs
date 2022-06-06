using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.View;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModel
{
    public class Person
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Description { get; set; }
        public string DateOfBirth { get; set; }
        public string Deceased { get; set; }
        public int Age { get; set; }
        public int? ProfileFileId { get; set; }
        public Sex Sex { get; set; }

        private readonly int id;

        public Person(int id)
        {
            this.id = id;
        }

        public int GetId()
        {
            return id;
        }
    }

    public partial class PersonsViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool readWriteMode = !Model.Model.Instance.Config.ReadOnly;

        public ObservableCollection<Person> Persons { get; } = new();

        [ObservableProperty]
        private Person selectedPerson;

        private readonly Model.Model model = Model.Model.Instance;

        public PersonsViewModel()
        {
            ReloadPersons();
            model.PersonsUpdated += Model_PersonsUpdated;
            model.ConfigLoaded += Model_ConfigLoaded;
        }

        private void Model_ConfigLoaded(object sender, EventArgs e)
        {
            ReadWriteMode = !model.Config.ReadOnly;
        }

        private void Model_PersonsUpdated(object sender, EventArgs e)
        {
            ReloadPersons();
        }

        [ICommand]
        private void RemovePerson()
        {
            if (Dialogs.ShowConfirmDialog($"Remove {selectedPerson.Firstname} {selectedPerson.Lastname}?"))
            {
                var filesWithPerson = model.DbAccess.SearchFilesWithPersons(new List<int>() { selectedPerson.GetId() }).ToList();
                if (filesWithPerson.Count == 0 || Dialogs.ShowConfirmDialog($"Person is used in {filesWithPerson.Count} files, remove anyway?"))
                {
                    model.DbAccess.DeletePerson(selectedPerson.GetId());
                    model.NotifyPersonsUpdated();
                }
            }
        }

        [ICommand]
        private void EditPerson()
        {
            var window = new AddPersonWindow(selectedPerson.GetId())
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        [ICommand]
        private void AddPerson()
        {
            var window = new AddPersonWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        [ICommand]
        private void PersonSelection(Person parameter)
        {
            SelectedPerson = parameter;
        }

        private void ReloadPersons()
        {
            Persons.Clear();

            var persons = model.DbAccess.GetPersons().Select(pm => new Person(pm.Id)
            {
                Firstname = pm.Firstname,
                Lastname = pm.Lastname,
                Description = pm.Description,
                DateOfBirth = pm.DateOfBirth,
                Deceased = pm.Deceased,
                Age = GetPersonAge(pm),
                ProfileFileId = pm.ProfileFileId,
                Sex = pm.Sex,
            });
            foreach (var person in persons)
            {
                Persons.Add(person);
            }
        }

        private int GetPersonAge(PersonModel person)
        {
            if (person.DateOfBirth != null)
            {
                var dateOfBirth = DatabaseParsing.ParsePersonsDateOfBirth(person.DateOfBirth);
                if (person.Deceased != null)
                {
                    var deceased = DatabaseParsing.ParsePersonsDeceased(person.Deceased);
                    return DatabaseUtils.GetYearsAgo(deceased, dateOfBirth);
                }
                else
                {
                    return DatabaseUtils.GetYearsAgo(DateTime.Now, dateOfBirth);
                }
            }

            return -1;
        }
    }
}
