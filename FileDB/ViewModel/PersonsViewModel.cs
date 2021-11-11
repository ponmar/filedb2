using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FileDB.View;
using FileDBInterface.Model;

namespace FileDB.ViewModel
{
    public class Person
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Description { get; set; }
        public string DateOfBirth { get; set; }
        public int BornYearsAgo { get; set; }
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
        private bool readWriteMode = !Utils.BrowserConfig.ReadOnly;

        public ObservableCollection<Person> Persons { get; } = new();

        private Person selectedPerson;

        public PersonsViewModel()
        {
            ReloadPersons();
        }

        public void RemovePerson()
        {
            if (selectedPerson != null)
            {
                var filesWithPerson = Utils.FileDBHandle.GetFilesWithPersons(new List<int>() { selectedPerson.GetId() }).ToList();
                if (filesWithPerson.Count == 0 || Utils.ShowConfirmDialog($"Person is used in {filesWithPerson.Count} files, remove anyway?"))
                {
                    Utils.FileDBHandle.DeletePerson(selectedPerson.GetId());
                    ReloadPersons();
                }
            }
        }

        public void EditPerson()
        {
            if (selectedPerson != null)
            {
                var window = new AddPersonWindow(selectedPerson.GetId())
                {
                    Owner = Application.Current.MainWindow
                };
                window.ShowDialog();
                ReloadPersons();
            }
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
            selectedPerson = (Person)parameter;
        }

        private void ReloadPersons()
        {
            Persons.Clear();

            var persons = Utils.FileDBHandle.GetPersons().Select(pm => new Person(pm.id) { Firstname = pm.firstname, Lastname = pm.lastname, Description = pm.description, DateOfBirth = pm.dateofbirth, BornYearsAgo = pm.dateofbirth != null ? Utils.GetYearsAgo(DateTime.Now, Utils.FileDBHandle.ParseDateOfBirth(pm.dateofbirth)) : -1, ProfileFileId = pm.profilefileid, Sex = pm.sex });
            foreach (var person in persons)
            {
                Persons.Add(person);
            }
        }
    }
}
