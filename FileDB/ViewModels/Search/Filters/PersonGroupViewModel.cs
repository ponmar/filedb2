using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;
using FileDB.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class PersonGroupViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private ObservableCollection<PersonForSearch> persons = [];

    [ObservableProperty]
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
    }

    private void TrySelectPersonsFromSelectedFile()
    {
        if (fileSelector.SelectedFile is not null)
        {
            var personsInFile = databaseAccessProvider.DbAccess.GetPersonsFromFile(fileSelector.SelectedFile.Id);
            if (personsInFile.Any())
            {
                SelectedPersons.Clear();
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

    public IFilesFilter CreateFilter() => new PersonGroupFilter(SelectedPersons, AllowOtherPersons);
}
