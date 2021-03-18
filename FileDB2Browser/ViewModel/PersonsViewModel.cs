using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileDB2Interface;
using FileDB2Interface.Model;

namespace FileDB2Browser.ViewModel
{
    public class PersonsViewModel
    {
        private readonly FileDB2Handle fileDB2Handle;

        public ObservableCollection<PersonModel> Persons { get; } = new ObservableCollection<PersonModel>();

        public PersonsViewModel(FileDB2Handle fileDB2Handle)
        {
            this.fileDB2Handle = fileDB2Handle;

            foreach (var person in fileDB2Handle.GetPersons())
            {
                Persons.Add(person);
            }
        }
    }
}
