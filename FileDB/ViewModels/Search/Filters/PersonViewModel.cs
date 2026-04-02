using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public record PersonForSearch(int Id, string Name)
{
    public override string ToString() => Name;
}

public partial class PersonViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    public partial ObservableCollection<PersonForSearch> Persons { get; set; } = [];

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    public partial PersonForSearch? SelectedPerson { get; set; }

    [ObservableProperty]
    public partial bool Negate { get; set; }

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

        ValidateAllProperties();
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
            Persons.Add(new(person.Id, person.FullName));
        }
    }

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        return Negate ?
            dbAccess.SearchFilesWithoutPerson(SelectedPerson!.Id) :
            dbAccess.SearchFilesWithPersons([SelectedPerson!.Id]);
    }
}
