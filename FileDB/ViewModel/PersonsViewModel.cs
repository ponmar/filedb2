using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FileDB.View;
using FileDBInterface;
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

    public class PersonsViewModel : ViewModelBase
    {
        public ICommand RemovePersonCommand => removePersonCommand ??= new CommandHandler(RemovePerson);
        private ICommand removePersonCommand;

        public ICommand EditPersonCommand => editPersonCommand ??= new CommandHandler(EditPerson);
        private ICommand editPersonCommand;

        public ICommand AddPersonCommand => addPersonCommand ??= new CommandHandler(AddPerson);
        private ICommand addPersonCommand;

        public ICommand PersonSelectionCommand => personSelectionCommand ??= new CommandHandler(PersonSelectionChanged);
        private ICommand personSelectionCommand;

        public bool ReadWriteMode
        {
            get => readWriteMode;
            set => SetProperty(ref readWriteMode, value);
        }
        private bool readWriteMode = !Utils.Config.ReadOnly;

        public ObservableCollection<Person> Persons { get; } = new();

        public Person SelectedPerson
        {
            get => selectedPerson;
            set => SetProperty(ref selectedPerson, value);
        }
        private Person selectedPerson;

        public PersonsViewModel()
        {
            ReloadPersons();
        }

        public void RemovePerson()
        {
            var filesWithPerson = Utils.DatabaseWrapper.SearchFilesWithPersons(new List<int>() { selectedPerson.GetId() }).ToList();
            if (filesWithPerson.Count == 0 || Utils.ShowConfirmDialog($"Person is used in {filesWithPerson.Count} files, remove anyway?"))
            {
                Utils.DatabaseWrapper.DeletePerson(selectedPerson.GetId());
                ReloadPersons();
            }
        }

        public void EditPerson()
        {
            var window = new AddPersonWindow(selectedPerson.GetId())
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            ReloadPersons();
        }

        public void AddPerson()
        {
            var window = new AddPersonWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            ReloadPersons();
        }

        public void PersonSelectionChanged(object parameter)
        {
            SelectedPerson = (Person)parameter;
        }

        private void ReloadPersons()
        {
            Persons.Clear();

            var persons = Utils.DatabaseWrapper.GetPersons().Select(pm => new Person(pm.Id)
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
