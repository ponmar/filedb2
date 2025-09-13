using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.FilesFilter;
using FileDB.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class PersonGroupViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    private ObservableCollection<PersonForSearch> persons = [];

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MinCount(1)]
    private ObservableCollection<PersonForSearch> selectedPersons = [];

    [ObservableProperty]
    private bool allowOtherPersons;

    private readonly IPersonsRepository personsRepository;
    private readonly IFileSelector fileSelector;
    private readonly IDatabaseAccessProvider databaseAccessProvider;

    public PersonGroupViewModel(IPersonsRepository personsRepository, IFileSelector fileSelector, IDatabaseAccessProvider databaseAccessProvider)
    {
        this.personsRepository = personsRepository;
        this.fileSelector = fileSelector;
        this.databaseAccessProvider = databaseAccessProvider;
        ReloadPersons();
        this.RegisterForEvent<PersonsUpdated>(x => ReloadPersons());
        TrySelectPersonsFromSelectedFile();

        ValidateAllProperties();
    }

    private void TrySelectPersonsFromSelectedFile()
    {
        SelectedPersons.Clear();
        if (fileSelector.SelectedFile is not null)
        {
            var personsInFile = databaseAccessProvider.DbAccess.GetPersonsFromFile(fileSelector.SelectedFile.Id);
            if (personsInFile.Any())
            {
                foreach (var personInFile in personsInFile)
                {
                    SelectedPersons.Add(Persons.First(x => x.Id == personInFile.Id));
                }
            }
        }
    }

    private void ReloadPersons()
    {
        Persons.Clear();
        foreach (var person in personsRepository.Persons)
        {
            Persons.Add(new(person.Id, $"{person.Firstname} {person.Lastname}"));
        }
    }

    [RelayCommand]
    private void UsePersonsFromCurrentFile() => TrySelectPersonsFromSelectedFile();

    public IFilesFilter CreateFilter() => new PersonGroupFilter(SelectedPersons, AllowOtherPersons);
}

public class MinCountAttribute : ValidationAttribute
{
    public int Min { get; }

    public MinCountAttribute(int min)
    {
        Min = min;
    }

    public override bool IsValid(object? value)
    {
        if (value is ICollection collection)
        {
            return collection.Count >= Min;
        }
        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The number of items in {name} must be at least {Min}.";
    }
}
