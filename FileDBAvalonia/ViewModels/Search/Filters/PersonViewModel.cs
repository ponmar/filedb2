using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;
using System.Collections.ObjectModel;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public record PersonForSearch(int Id, string Name)
{
    public override string ToString() => Name;
}

public partial class PersonViewModel : AbstractFilterViewModel
{
    [ObservableProperty]
    private ObservableCollection<PersonForSearch> persons = [];

    [ObservableProperty]
    private PersonForSearch? selectedPerson;

    [ObservableProperty]
    private bool negate;

    private readonly IPersonsRepository personsRepository;

    public PersonViewModel(IPersonsRepository personsRepository) : base(FilterType.Person)
    {
        this.personsRepository = personsRepository;
        ReloadPersons();
        this.RegisterForEvent<PersonsUpdated>((x) => ReloadPersons());
    }

    private void ReloadPersons()
    {
        Persons.Clear();
        foreach (var person in personsRepository.Persons)
        {
            Persons.Add(new(person.Id, $"{person.Firstname} {person.Lastname}"));
        }
    }

    protected override IFilesFilter DoCreate() =>
        Negate ? new FilterWithoutPerson(SelectedPerson) : new FilterPerson(SelectedPerson);
}
