using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Extensions;
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
    private FilterType selectedFilterType = FilterTypes.First(x => x == FilterType.Person);

    [ObservableProperty]
    private DateTime dateTimeStart = DateTime.Now;

    [ObservableProperty]
    private DateTime dateTimeEnd = DateTime.Now;

    [ObservableProperty]
    private string textFilterSearchPattern = string.Empty;

    [ObservableProperty]
    private string fileListIds = string.Empty;

    [ObservableProperty]
    private string exceptFileListIds = string.Empty;

    public static IEnumerable<FileType> FileTypes => Enum.GetValues<FileType>().OrderBy(x => x.ToFriendlyString());

    [ObservableProperty]
    private FileType selectedFileType = FileTypes.First();

    [ObservableProperty]
    private ObservableCollection<PersonModel> persons = [];

    [ObservableProperty]
    private PersonModel? selectedPerson;

    [ObservableProperty]
    private ObservableCollection<LocationModel> locations = [];

    [ObservableProperty]
    private ObservableCollection<LocationModel> locationsWithPosition = [];

    [ObservableProperty]
    private LocationModel? selectedLocation;

    [ObservableProperty]
    private ObservableCollection<TagModel> tags = [];

    [ObservableProperty]
    private TagModel? selectedTag;

    [ObservableProperty]
    private string personAgeFrom = string.Empty;

    [ObservableProperty]
    private string personAgeTo = string.Empty;

    public static IEnumerable<Sex> PersonSexValues => Enum.GetValues<Sex>().OrderBy(x => x.ToFriendlyString());

    [ObservableProperty]
    private Sex selectedPersonSex = PersonSexValues.First();

    [ObservableProperty]
    private string positionText = string.Empty;

    [ObservableProperty]
    private string radiusText = "500";

    [ObservableProperty]
    private string numPersonsMin = "1";

    [ObservableProperty]
    private string numPersonsMax = "1";

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
        LocationsWithPosition.Clear();
        foreach (var location in locationsRepository.Locations)
        {
            Locations.Add(location);
            if (location.Position is not null)
            {
                LocationsWithPosition.Add(location);
            }
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
            FilterType.DateTime => new FilterDateTime(DateTimeStart, DateTimeEnd),
            FilterType.NoDateTime => new FilterWithoutDateTime(),
            FilterType.NoMetaData => new FilterWithoutMetaData(),
            FilterType.Text => new FilterText(TextFilterSearchPattern),
            FilterType.FileList => new FilterFileList(FileListIds),
            FilterType.ExceptFileList => new FilterExceptFileList(ExceptFileListIds),
            FilterType.FileType => new FilterFileType(SelectedFileType),
            FilterType.Person => new FilterPerson(SelectedPerson),
            FilterType.PersonAge => new FilterPersonAge(PersonAgeFrom, PersonAgeTo),
            FilterType.PersonSex => new FilterPersonSex(SelectedPersonSex),
            FilterType.Location => new FilterLocation(SelectedLocation),
            FilterType.Tag => new FilterTag(SelectedTag),
            FilterType.Position => new FilterPosition(PositionText, RadiusText),
            FilterType.NumPersons => new FilterNumberOfPersons(NumPersonsMin, NumPersonsMax),
            _ => throw new NotImplementedException(),
        };
    }
}
