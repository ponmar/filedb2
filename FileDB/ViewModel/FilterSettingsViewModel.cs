using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;
using FileDB.Model;
using FileDBShared.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileDB.ViewModel;

public partial class FilterSettingsViewModel : ObservableObject
{
    public static IEnumerable<FilterType> FilterTypes => Enum.GetValues<FilterType>().OrderBy(x => x.ToFriendlyString());

    [ObservableProperty]
    private FilterType selectedFilterType = FilterTypes.First();

    [ObservableProperty]
    private string textFilterSearchPattern = string.Empty;

    [ObservableProperty]
    private string fileListIds = string.Empty;

    public static IEnumerable<Model.FileType> FileTypes => Enum.GetValues<Model.FileType>().OrderBy(x => x.ToFriendlyString());

    [ObservableProperty]
    private Model.FileType selectedFileType = FileTypes.First();

    [ObservableProperty]
    private ObservableCollection<PersonModel> persons = [];

    [ObservableProperty]
    private PersonModel? selectedPerson;

    private readonly IPersonsRepository personsRepository;

    public FilterSettingsViewModel(IPersonsRepository personsRepository)
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
            Persons.Add(person);
        }
    }

    public IFilesFilter Create()
    {
        return SelectedFilterType switch
        {
            FilterType.NoDateTime => new WithoutDateTime(),
            FilterType.NoMetaData => new WithoutMetaData(),
            FilterType.Text => new Text(TextFilterSearchPattern),
            FilterType.FileList => new FileList(FileListIds),
            FilterType.FileType => new FilesFilter.FileType(SelectedFileType),
            FilterType.Person => new FilesFilter.Person(SelectedPerson!), // TODO: error handling for no persons
            _ => throw new NotImplementedException(),
        };
    }
}
