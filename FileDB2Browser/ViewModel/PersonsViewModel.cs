using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileDB2Interface;
using FileDB2Interface.Model;

namespace FileDB2Browser.ViewModel
{
    public class Person
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Description { get; set; }
        public string DateOfBirth { get; set; }
        public string BornYearsAgo { get; set; }
    }

    public class PersonsViewModel
    {
        public ICommand AddPersonCommand
        {
            get
            {
                return addPersonCommand ??= new CommandHandler(AddPerson);
            }
        }
        private ICommand addPersonCommand;

        public ICommand RemovePersonCommand
        {
            get
            {
                return removePersonCommand ??= new CommandHandler(RemovePerson);
            }
        }
        private ICommand removePersonCommand;

        public ICommand EditPersonCommand
        {
            get
            {
                return editPersonCommand ??= new CommandHandler(EditPerson);
            }
        }
        private ICommand editPersonCommand;

        public ICommand PersonSelectionCommand
        {
            get
            {
                return personSelectionCommand ??= new CommandHandler(PersonSelectionChanged);
            }
        }
        private ICommand personSelectionCommand;

        public ObservableCollection<Person> Persons { get; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string Description { get; set; }

        public string DateOfBirth { get; set; }

        private Person selectedPerson;

        private readonly FileDB2Handle fileDB2Handle;

        public PersonsViewModel(FileDB2Handle fileDB2Handle)
        {
            this.fileDB2Handle = fileDB2Handle;
            var persons = fileDB2Handle.GetPersons().Select(pm => new Person() { Firstname = pm.firstname, Lastname = pm.lastname, Description = pm.description, DateOfBirth = pm.dateofbirth, BornYearsAgo = Utils.GetBornYearsAgo(pm.dateofbirth) });
            Persons = new ObservableCollection<Person>(persons);
        }

        public void AddPerson(object parameter)
        {
            fileDB2Handle.InsertPerson(Firstname, Lastname, Description, DateOfBirth);
            Firstname = string.Empty;
            Lastname = string.Empty;
            Description = string.Empty;
            DateOfBirth = string.Empty;
        }

        public void RemovePerson(object parameter)
        {
            if (selectedPerson != null)
            {
                // TODO: find id somehow and remove person
            }
        }

        public void EditPerson(object parameter)
        {
            if (selectedPerson != null)
            {
                // TODO: find id somehow and edit person
            }
        }

        public void PersonSelectionChanged(object parameter)
        {
            selectedPerson = (Person)parameter;
        }
    }
}
