using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using FileDB.Validators;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

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
            Persons.Add(new(person.Id, person.FullName));
        }
    }

    [RelayCommand]
    private void UsePersonsFromCurrentFile() => TrySelectPersonsFromSelectedFile();

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        return AllowOtherPersons ?
            dbAccess.SearchFilesWithPersonGroup(SelectedPersons.Select(x => x.Id)) :
            dbAccess.SearchFilesWithPersonGroupOnly(SelectedPersons.Select(x => x.Id));
    }
}
