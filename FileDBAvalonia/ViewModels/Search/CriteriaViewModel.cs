using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System;
using FileDBShared.Model;
using System.Linq;
using System.IO;
using System.Collections.ObjectModel;
using FileDBInterface.Extensions;
using FileDBAvalonia.Model;
using FileDBAvalonia.Dialogs;
using System.Threading.Tasks;
using FileDBAvalonia.Comparers;
using FileDBAvalonia.ViewModels.Search.Filters;

namespace FileDBAvalonia.ViewModels;

public interface ISearchResultRepository
{
    IEnumerable<FileModel> Files { get; }

    FileModel? SelectedFile { get; } // TODO: how does repo users know that the selected file has been changed?
}

public partial class CriteriaViewModel : ObservableObject, ISearchResultRepository
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFiles))]
    private IEnumerable<FileModel> files = [];

    partial void OnFilesChanged(IEnumerable<FileModel> value)
    {
        Messenger.Send<NewSearchResult>();
    }

    public bool HasFiles => Files.Any();

    [ObservableProperty]
    private string numRandomFiles = "10";

    [ObservableProperty]
    private string importedFileList = string.Empty;

    [ObservableProperty]
    private LocationForSearch? selectedLocationForPositionSearch;

    partial void OnSelectedLocationForPositionSearchChanged(LocationForSearch? value)
    {
        if (value is not null)
        {
            var location = dbAccessProvider.DbAccess.GetLocationById(value.Id);
            if (location.Position is not null)
            {
                SearchFileGpsPosition = location.Position;
            }
            else
            {
                SearchFileGpsPosition = string.Empty;
                dialogs.ShowInfoDialogAsync("This location has no GPS position set.");
            }
        }
    }

    [ObservableProperty]
    private string? searchFileGpsPosition;

    [ObservableProperty]
    private string? searchFileGpsPositionUrl;

    partial void OnSearchFileGpsPositionUrlChanged(string? value)
    {
        if (value.HasContent())
        {
            var gpsPos = DatabaseParsing.ParseFilesPositionFromUrl(SearchFileGpsPositionUrl);
            if (gpsPos is not null)
            {
                SearchFileGpsPosition = $"{gpsPos.Value.lat} {gpsPos.Value.lon}";
                return;
            }
        }
        SearchFileGpsPosition = string.Empty;
    }

    [ObservableProperty]
    private string searchFileGpsRadius = "500";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CombineSearchResultPossible))]
    private string combineSearch1 = string.Empty;

    partial void OnCombineSearch1Changed(string value)
    {
        CombineSearchResult = string.Empty;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CombineSearchResultPossible))]
    private string combineSearch2 = string.Empty;

    partial void OnCombineSearch2Changed(string value)
    {
        CombineSearchResult = string.Empty;
    }

    [ObservableProperty]
    private string combineSearchResult = string.Empty;

    public bool CombineSearchResultPossible => CombineSearch1.HasContent() && CombineSearch2.HasContent();

    [ObservableProperty]
    private PersonForSearch? selectedPersonSearch;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Person1And2Selected))]
    private PersonForSearch? selectedPerson1Search;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Person1And2Selected))]
    private PersonForSearch? selectedPerson2Search;

    public bool Person1And2Selected => SelectedPerson1Search is not null && SelectedPerson2Search is not null;

    [ObservableProperty]
    private TagForSearch? selectedTagSearch;

    [ObservableProperty]
    private LocationForSearch? selectedLocationSearch;

    [ObservableProperty]
    private ObservableCollection<IFilterViewModel> filterSettings = [];

    public bool FilterCanBeRemoved => FilterSettings.Count > 1;

    public bool FileSelected => SelectedFile is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileSelected))]
    [NotifyPropertyChangedFor(nameof(CurrentFileHasPosition))]
    private FileModel? selectedFile;

    public bool CurrentFileHasPosition => SelectedFile is not null && SelectedFile.Position.HasContent();

    public ObservableCollection<PersonForSearch> Persons { get; } = [];
    public ObservableCollection<LocationForSearch> Locations { get; } = [];
    public ObservableCollection<LocationForSearch> LocationsWithPosition { get; } = [];
    public ObservableCollection<TagForSearch> Tags { get; } = [];

    private readonly IDialogs dialogs;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IPersonsRepository personsRepository;
    private readonly ILocationsRepository locationsRepository;
    private readonly ITagsRepository tagsRepository;
    private readonly IClipboardService clipboardService;

    public CriteriaViewModel(IDialogs dialogs, IDatabaseAccessProvider dbAccessProvider, IPersonsRepository personsRepository, ILocationsRepository locationsRepository, ITagsRepository tagsRepository, IClipboardService clipboardService)
    {
        this.dialogs = dialogs;
        this.dbAccessProvider = dbAccessProvider;
        this.personsRepository = personsRepository;
        this.locationsRepository = locationsRepository;
        this.tagsRepository = tagsRepository;
        this.clipboardService = clipboardService;

        filterSettings.Add(CreateDefaultFilter());

        ReloadPersons();
        ReloadLocations();
        ReloadTags();

        this.RegisterForEvent<PersonsUpdated>(x => ReloadPersons());
        this.RegisterForEvent<LocationsUpdated>(x => ReloadLocations());
        this.RegisterForEvent<TagsUpdated>(x => ReloadTags());

        this.RegisterForEvent<FilesImported>(x =>
        {
            ImportedFileList = Utils.CreateFileList(x.Files);
        });

        this.RegisterForEvent<SelectSearchResultFile>(x =>
        {
            SelectedFile = x.File;
        });

        this.RegisterForEvent<CloseSearchResultFile>(x =>
        {
            SelectedFile = null;
        });

        this.RegisterForEvent<SearchFilterSelectionChanged>(x =>
        {
            var filterIndex = filterSettings.IndexOf(x.CurrentFilter);
            filterSettings[filterIndex] = CreateFilterFromType(x.NewFilterType);
        });
    }

    private IFilterViewModel CreateFilterFromType(FilterType filterType)
    {
        return filterType switch
        {
            FilterType.AnnualDate => new AnnualDateViewModel(),
            FilterType.Date => new DateViewModel(),
            FilterType.NoMetaData => new NoMetaDataViewModel(),
            FilterType.NoDateTime => new NoDateTimeViewModel(),
            FilterType.Text => new TextViewModel(),
            FilterType.FileList => new FileListViewModel(),
            FilterType.FileType => new FileTypeViewModel(),
            FilterType.Person => new PersonViewModel(personsRepository),
            FilterType.PersonAge => new PersonAgeViewModel(),
            FilterType.PersonSex => new PersonSexViewModel(),
            FilterType.Location => new LocationViewModel(locationsRepository),
            FilterType.Position => new PositionViewModel(),
            FilterType.Season => new SeasonViewModel(),
            FilterType.NumPersons => new NumPersonsViewModel(),
            FilterType.Tag => new TagViewModel(tagsRepository),
            _ => throw new NotImplementedException(),
        };
    }

    private void ReloadPersons()
    {
        Persons.Clear();
        foreach (var person in personsRepository.Persons.Select(p => new PersonForSearch(p.Id, $"{p.Firstname} {p.Lastname}")))
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
            var locationToUpdate = new LocationForSearch(location.Id, location.Name);
            Locations.Add(locationToUpdate);
            if (location.Position is not null)
            {
                LocationsWithPosition.Add(locationToUpdate);
            }
        }
    }

    private void ReloadTags()
    {
        Tags.Clear();
        foreach (var tag in tagsRepository.Tags.Select(t => new TagForSearch(t.Id, t.Name)))
        {
            Tags.Add(tag);
        }
    }

    [RelayCommand]
    private void FindRandomFiles()
    {
        if (int.TryParse(NumRandomFiles, out var value))
        {
            Files = dbAccessProvider.DbAccess.SearchFilesRandom(value);
        }
    }

    [RelayCommand]
    private async Task FindCurrentDirectoryFilesAsync()
    {
        if (SelectedFile is null)
        {
            await dialogs.ShowErrorDialogAsync("No file opened");
            return;
        }

        var path = SelectedFile.Path;
        var dir = Path.GetDirectoryName(path)!.Replace('\\', '/');
        Files = dbAccessProvider.DbAccess.SearchFilesByPath(dir);
    }

    [RelayCommand]
    private void FindAllFiles()
    {
        Files = dbAccessProvider.DbAccess.GetFiles();
    }

    [RelayCommand]
    private void FindImportedFiles()
    {
        if (ImportedFileList.HasContent())
        {
            var fileIds = Utils.CreateFileIds(ImportedFileList);
            Files = dbAccessProvider.DbAccess.SearchFilesFromIds(fileIds);
        }
    }

    [RelayCommand]
    private async Task FindBrowsedFilesAsync()
    {
        var selectedDir = await dialogs.ShowBrowseDirectoriesDialogAsync();
        if (selectedDir is not null)
        {
            Files = dbAccessProvider.DbAccess.SearchFilesByPath(selectedDir);
        }
    }

    [RelayCommand]
    private void SearchFilePositionFromCurrentFile()
    {
        SearchFileGpsPosition = SelectedFile is not null ? SelectedFile.Position : string.Empty;
    }

    [RelayCommand]
    private async Task FindFilesByGpsPositionAsync()
    {
        if (!SearchFileGpsPosition.HasContent())
        {
            await dialogs.ShowErrorDialogAsync("No position specified");
            return;
        }

        if (!SearchFileGpsRadius.HasContent())
        {
            await dialogs.ShowErrorDialogAsync("No radius specified");
            return;
        }
        if (!double.TryParse(SearchFileGpsRadius, out var radius) || radius < 1)
        {
            await dialogs.ShowErrorDialogAsync("Invalid radius");
            return;
        }

        var gpsPos = DatabaseParsing.ParseFilesPosition(SearchFileGpsPosition);
        if (gpsPos is null)
        {
            await dialogs.ShowErrorDialogAsync("Invalid GPS position");
            return;
        }

        var nearFiles = dbAccessProvider.DbAccess.SearchFilesNearGpsPosition(gpsPos.Value.lat, gpsPos.Value.lon, radius).ToList();

        // TODO: checkbox for selecting if this should be included?
        var nearLocations = dbAccessProvider.DbAccess.SearchLocationsNearGpsPosition(gpsPos.Value.lat, gpsPos.Value.lon, radius);
        nearFiles.AddRange(dbAccessProvider.DbAccess.SearchFilesWithLocations(nearLocations.Select(x => x.Id)));

        Files = nearFiles;
    }

    [RelayCommand]
    private void SetCombineSearch1()
    {
        CombineSearch1 = Utils.CreateFileList(Files);
    }

    [RelayCommand]
    private void SetCombineSearch2()
    {
        CombineSearch2 = Utils.CreateFileList(Files);
    }

    [RelayCommand]
    private void CombineSearchIntersection()
    {
        var files1 = Utils.CreateFileIds(CombineSearch1);
        var files2 = Utils.CreateFileIds(CombineSearch2);
        var result = files1.Intersect(files2);
        CombineSearchResult = Utils.CreateFileList(result);
    }

    [RelayCommand]
    private void CombineSearchUnion()
    {
        var files1 = Utils.CreateFileIds(CombineSearch1);
        var files2 = Utils.CreateFileIds(CombineSearch2);
        var result = files1.Union(files2);
        CombineSearchResult = Utils.CreateFileList(result);
    }

    [RelayCommand]
    private void CombineSearchDifference()
    {
        var files1 = Utils.CreateFileIds(CombineSearch1);
        var files2 = Utils.CreateFileIds(CombineSearch2);
        var uniqueFiles1 = files1.Except(files2);
        var uniqueFiles2 = files2.Except(files1);
        var result = uniqueFiles1.Union(uniqueFiles2);
        CombineSearchResult = Utils.CreateFileList(result);
    }

    [RelayCommand]
    private void CombineSearchResultCopy()
    {
        clipboardService.SetTextAsync(CombineSearchResult);
    }

    [RelayCommand]
    private void CombineSearchResultShow()
    {
        var fileIds = Utils.CreateFileIds(CombineSearchResult);
        Files = dbAccessProvider.DbAccess.SearchFilesFromIds(fileIds);
    }

    [RelayCommand]
    private void AddFilter()
    {
        FilterSettings.Add(CreateDefaultFilter());
        OnPropertyChanged(nameof(FilterCanBeRemoved));
    }

    private static IFilterViewModel CreateDefaultFilter() => new TextViewModel();

    [RelayCommand]
    private void RemoveFilter(IFilterViewModel vm)
    {
        FilterSettings.Remove(vm);
        OnPropertyChanged(nameof(FilterCanBeRemoved));
    }

    [RelayCommand]
    private async Task FindFilesFromFiltersAsync()
    {
        var fileModelComparer = new FileModelByIdComparer();
        var filters = FilterSettings.Select(x => (viewModel: x, filter: x.Create())).ToList();
        var filtersWithInvalidSettings = filters.Where(x => !x.filter.CanRun());
        if (filtersWithInvalidSettings.Any())
        {
            await dialogs.ShowErrorDialogAsync($"Invalid settings for filters: {string.Join(", ", filtersWithInvalidSettings.Select(x => x.viewModel.SelectedFilterType.ToFriendlyString()))}");
            return;
        }

        var result = Enumerable.Empty<FileModel>();
        foreach (var filter in filters.Select(x => x.filter))
        {
            var files = filter.Run(dbAccessProvider.DbAccess);
            result = filter == filters.First().filter ? files : result.Intersect(files, fileModelComparer);

            if (!result.Any())
            {
                // No need to continue with db queries because the next intersection will throw them away
                break;
            }
        }

        Files = result;
    }
}
