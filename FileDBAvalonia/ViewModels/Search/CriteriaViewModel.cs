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
using FileDBAvalonia.ViewModels.Search;

namespace FileDBAvalonia.ViewModels;

public partial class CriteriaViewModel : ObservableObject
{
    [ObservableProperty]
    private string numRandomFiles = "10";

    [ObservableProperty]
    private string addedFiles = string.Empty;

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
    private ObservableCollection<IFilterViewModel> filterSettings = [];

    public bool FilterCanBeRemoved => FilterSettings.Count > 1;

    [ObservableProperty]
    private FileModel? selectedFile;

    partial void OnSelectedFileChanged(FileModel? value)
    {
        HasFiles = value is not null;
    }

    [ObservableProperty]
    public bool hasFiles;
        
    private readonly IDialogs dialogs;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IPersonsRepository personsRepository;
    private readonly ILocationsRepository locationsRepository;
    private readonly ITagsRepository tagsRepository;
    private readonly IClipboardService clipboardService;
    private readonly IFileSelector fileSelector;
    private readonly ISearchResultRepository searchResultRepository;

    public CriteriaViewModel(IDialogs dialogs, IDatabaseAccessProvider dbAccessProvider, IPersonsRepository personsRepository, ILocationsRepository locationsRepository, ITagsRepository tagsRepository, IClipboardService clipboardService, IFileSelector fileSelector, ISearchResultRepository searchResultRepository)
    {
        this.dialogs = dialogs;
        this.dbAccessProvider = dbAccessProvider;
        this.personsRepository = personsRepository;
        this.locationsRepository = locationsRepository;
        this.tagsRepository = tagsRepository;
        this.clipboardService = clipboardService;
        this.fileSelector = fileSelector;
        this.searchResultRepository = searchResultRepository;

        filterSettings.Add(CreateDefaultFilter());

        this.RegisterForEvent<FilesAdded>(x =>
        {
            AddedFiles = Utils.CreateFileList(x.Files);
        });

        this.RegisterForEvent<FileSelectionChanged>(x =>
        {
            SelectedFile = fileSelector.SelectedFile;
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
            FilterType.Position => new PositionViewModel(locationsRepository, dbAccessProvider, fileSelector),
            FilterType.Season => new SeasonViewModel(),
            FilterType.NumPersons => new NumPersonsViewModel(),
            FilterType.Tag => new TagViewModel(tagsRepository),
            _ => throw new NotImplementedException(),
        };
    }

    private static void Send(IEnumerable<FileModel> files) => Messenger.Send(new TransferSearchResult(files));

    [RelayCommand]
    private void FindRandomFiles()
    {
        if (int.TryParse(NumRandomFiles, out var value))
        {
            Send(dbAccessProvider.DbAccess.SearchFilesRandom(value));
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
        Send(dbAccessProvider.DbAccess.SearchFilesByPath(dir));
    }

    [RelayCommand]
    private void FindAllFiles()
    {
        Send(dbAccessProvider.DbAccess.GetFiles());
    }

    [RelayCommand]
    private void FindAddedFiles()
    {
        if (AddedFiles.HasContent())
        {
            var fileIds = Utils.CreateFileIds(AddedFiles);
            Send(dbAccessProvider.DbAccess.SearchFilesFromIds(fileIds));
        }
    }

    [RelayCommand]
    private async Task FindBrowsedFilesAsync()
    {
        var selectedDir = await dialogs.ShowBrowseDirectoriesDialogAsync();
        if (selectedDir is not null)
        {
            Send(dbAccessProvider.DbAccess.SearchFilesByPath(selectedDir));
        }
    }

    [RelayCommand]
    private void SetCombineSearch1()
    {
        CombineSearch1 = Utils.CreateFileList(searchResultRepository.Files);
    }

    [RelayCommand]
    private void SetCombineSearch2()
    {
        CombineSearch2 = Utils.CreateFileList(searchResultRepository.Files);
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
        Send(dbAccessProvider.DbAccess.SearchFilesFromIds(fileIds));
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

        Send(result);
    }
}
