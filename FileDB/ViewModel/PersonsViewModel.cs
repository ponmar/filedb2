using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDB.Sorters;
using FileDB.View;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModel;

public record Person(int Id, string Firstname, string Lastname, string? Description, string? DateOfBirth, string? Deceased, int Age, int? ProfileFileId, Sex Sex);

public partial class PersonsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool readWriteMode = !Model.Model.Instance.Config.ReadOnly;

    public ObservableCollection<Person> Persons { get; } = new();

    [ObservableProperty]
    private Person? selectedPerson;

    private readonly Model.Model model = Model.Model.Instance;

    public PersonsViewModel()
    {
        ReloadPersons();

        WeakReferenceMessenger.Default.Register<ConfigLoaded>(this, (r, m) =>
        {
            ReadWriteMode = !model.Config.ReadOnly;
        });

        WeakReferenceMessenger.Default.Register<PersonsUpdated>(this, (r, m) =>
        {
            ReloadPersons();
        });
    }

    [RelayCommand]
    private void RemovePerson()
    {
        if (Dialogs.ShowConfirmDialog($"Remove {selectedPerson!.Firstname} {selectedPerson.Lastname}?"))
        {
            var filesWithPerson = model.DbAccess.SearchFilesWithPersons(new List<int>() { selectedPerson.Id }).ToList();
            if (filesWithPerson.Count == 0 || Dialogs.ShowConfirmDialog($"Person is used in {filesWithPerson.Count} files, remove anyway?"))
            {
                model.DbAccess.DeletePerson(selectedPerson.Id);
                WeakReferenceMessenger.Default.Send(new PersonsUpdated());
            }
        }
    }

    [RelayCommand]
    private void EditPerson()
    {
        var window = new AddPersonWindow(selectedPerson!.Id)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
    }

    [RelayCommand]
    private void AddPerson()
    {
        var window = new AddPersonWindow
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
    }

    [RelayCommand]
    private void PersonSelection(Person parameter)
    {
        SelectedPerson = parameter;
    }

    private void ReloadPersons()
    {
        Persons.Clear();
        var persons = model.DbAccess.GetPersons().ToList();
        persons.Sort(new PersonModelByNameSorter());
        var personVms = persons.Select(pm => new Person(pm.Id, pm.Firstname, pm.Lastname, pm.Description, pm.DateOfBirth, pm.Deceased, GetPersonAge(pm), pm.ProfileFileId, pm.Sex));
        foreach (var personVm in personVms)
        {
            Persons.Add(personVm);
        }
    }

    private int GetPersonAge(PersonModel person)
    {
        if (person.DateOfBirth != null)
        {
            var dateOfBirth = DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth);
            if (person.Deceased != null)
            {
                var deceased = DatabaseParsing.ParsePersonDeceasedDate(person.Deceased);
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
