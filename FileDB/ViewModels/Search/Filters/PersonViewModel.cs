using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;
using FileDB.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileDB.ViewModels.Search.Filters;

public record PersonForSearch(int Id, string Name)
{
    public override string ToString() => Name;
}

public partial class PersonViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private ObservableCollection<PersonForSearch> persons = [];

    [ObservableProperty]
    private PersonForSearch? selectedPerson;

    [ObservableProperty]
    private bool negate;

    private readonly IPersonsRepository personsRepository;
    private readonly IFileSelector fileSelector;
    private readonly IDatabaseAccessProvider databaseAccessProvider;

    public PersonViewModel(IPersonsRepository personsRepository, IFileSelector fileSelector, IDatabaseAccessProvider databaseAccessProvider)
    {
        this.personsRepository = personsRepository;
        this.fileSelector = fileSelector;
        this.databaseAccessProvider = databaseAccessProvider;
        ReloadPersons();
        this.RegisterForEvent<PersonsUpdated>((x) => ReloadPersons());
        TrySelectPersonFromSelectedFile();
    }

    private void TrySelectPersonFromSelectedFile()
    {
        if (fileSelector.SelectedFile is not null)
        {
            var personsInFile = databaseAccessProvider.DbAccess.GetPersonsFromFile(fileSelector.SelectedFile.Id);
            if (personsInFile.Any())
            {
                var firstPersonFromFile = personsInFile.First();
                SelectedPerson = Persons.First(x => x.Id == firstPersonFromFile.Id);
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

    public IFilesFilter CreateFilter() =>
        Negate ? new WithoutPersonFilter(SelectedPerson) : new PersonFilter(SelectedPerson);
}
