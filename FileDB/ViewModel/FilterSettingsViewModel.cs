using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;
using FileDB.Model;
using FileDB.Sorters;
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

    public static IEnumerable<FileType> FileTypes => Enum.GetValues<FileType>().OrderBy(x => x.ToFriendlyString());

    [ObservableProperty]
    private Model.FileType selectedFileType = FileTypes.First();

    [ObservableProperty]
    private ObservableCollection<PersonModel> persons = [];

    [ObservableProperty]
    private PersonModel? selectedPerson;

    [ObservableProperty]
    private ObservableCollection<LocationModel> locations = [];

    [ObservableProperty]
    private LocationModel? selectedLocation;

    [ObservableProperty]
    private ObservableCollection<TagModel> tags = [];

    [ObservableProperty]
    private TagModel? selectedTag;

    private readonly IPersonsRepository personsRepository;
    private readonly ILocationsRepository locationsRepository;
    private readonly ITagsRepository tagsRepository;

    public FilterSettingsViewModel(IPersonsRepository personsRepository, ILocationsRepository locationsRepository, ITagsRepository tagsRepository)
    {
        this.personsRepository = personsRepository;
        this.locationsRepository = locationsRepository;
        this.tagsRepository = tagsRepository;

        ReloadPersons();
        ReloadLocations();
        ReloadTags();

        this.RegisterForEvent<PersonsUpdated>((x) => ReloadPersons());
        this.RegisterForEvent<LocationsUpdated>((x) => ReloadLocations());
        this.RegisterForEvent<TagsUpdated>((x) => ReloadTags());
    }

    private void ReloadPersons()
    {
        Persons.Clear();
        foreach (var person in personsRepository.Persons)
        {
            Persons.Add(person);
        }
    }

    private void ReloadLocations()
    {
        Locations.Clear();
        foreach (var location in locationsRepository.Locations)
        {
            Locations.Add(location);
        }
    }

    private void ReloadTags()
    {
        Tags.Clear();
        foreach (var tag in tagsRepository.Tags)
        {
            Tags.Add(tag);
        }
    }

    public IFilesFilter Create()
    {
        return SelectedFilterType switch
        {
            FilterType.NoDateTime => new FilterWithoutDateTime(),
            FilterType.NoMetaData => new FilterWithoutMetaData(),
            FilterType.Text => new FilterText(TextFilterSearchPattern),
            FilterType.FileList => new FilterFileList(FileListIds),
            FilterType.FileType => new FilterFileType(SelectedFileType),
            FilterType.Person => new FilterPerson(SelectedPerson),
            FilterType.Location => new FilterLocation(SelectedLocation),
            FilterType.Tag => new FilterTag(SelectedTag),
            _ => throw new NotImplementedException(),
        };
    }
}
