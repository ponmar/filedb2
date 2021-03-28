using System;
using System.Collections;
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

        private Person selectedPerson;

        private readonly FileDB2Handle fileDB2Handle;

        public PersonsViewModel(FileDB2Handle fileDB2Handle)
        {
            this.fileDB2Handle = fileDB2Handle;
            var persons = GetPersons();
            Persons = new ObservableCollection<Person>(persons);
        }

        public void RemovePerson(object parameter)
        {
            if (selectedPerson != null)
            {
                // TODO: only delete if person not used in files?
                fileDB2Handle.DeletePerson(selectedPerson.GetId());

                Persons.Clear();
                foreach (var person in GetPersons())
                {
                    Persons.Add(person);
                }
            }
        }

        public void EditPerson(object parameter)
        {
            if (selectedPerson != null)
            {
                // TODO: edit in new window
            }
        }

        public void PersonSelectionChanged(object parameter)
        {
            selectedPerson = (Person)parameter;
        }

        private IEnumerable<Person> GetPersons()
        {
            return fileDB2Handle.GetPersons().Select(pm => new Person(pm.id) { Firstname = pm.firstname, Lastname = pm.lastname, Description = pm.description, DateOfBirth = pm.dateofbirth, BornYearsAgo = Utils.GetBornYearsAgoString(DateTime.Now, pm.dateofbirth) });
        }
    }
}
