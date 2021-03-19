﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public ObservableCollection<Person> Persons { get; }

        public PersonsViewModel(FileDB2Handle fileDB2Handle)
        {
            var persons = fileDB2Handle.GetPersons().Select(pm => new Person() { Firstname = pm.firstname, Lastname = pm.lastname, Description = pm.description, DateOfBirth = pm.dateofbirth, BornYearsAgo = Utils.GetBornYearsAgo(pm.dateofbirth) });
            Persons = new ObservableCollection<Person>(persons);
        }
    }
}
