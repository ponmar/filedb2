using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using FileDBInterface.Model;
using System.Linq;
using System.Collections.ObjectModel;
using FileDBInterface.Extensions;
using FileDB.Model;
using FileDB.Dialogs;
using System.Threading.Tasks;
using FileDB.Comparers;
using FileDB.ViewModels.Search;
using System;
using FileDB.ViewModels.Search.Filters;

namespace FileDB.ViewModels;

public partial class CriteriaViewModel : ObservableObject
{
    [ObservableProperty]
    private string numRandomFiles = "10";

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
    private ObservableCollection<FilterSelectionViewModel> filterSettings = [];

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
    private readonly IClipboardService clipboardService;
    private readonly ISearchResultRepository searchResultRepository;

    public CriteriaViewModel(IDialogs dialogs, IDatabaseAccessProvider dbAccessProvider, IClipboardService clipboardService, ISearchResultRepository searchResultRepository, IFileSelector fileSelector)
    {
        this.dialogs = dialogs;
        this.dbAccessProvider = dbAccessProvider;
        this.clipboardService = clipboardService;
        this.searchResultRepository = searchResultRepository;

        var vm = ServiceLocator.Resolve<FilterSelectionViewModel>();
        vm.IsFirstFilter = true;
        filterSettings.Add(vm);

        this.RegisterForEvent<FileSelectionChanged>(x =>
        {
            SelectedFile = fileSelector.SelectedFile;
        });

        this.RegisterForEvent<AddPersonSearchFilter>(x => AddPersonFilterFor(x.Person));

        this.RegisterForEvent<AddPersonGroupSearchFilter>(x => AddPersonGroupFilterFor(x.Persons));

        this.RegisterForEvent<AddLocationSearchFilter>(x => AddLocationFilterFor(x.Location));

        this.RegisterForEvent<AddTagSearchFilter>(x => AddTagFilterFor(x.Tag));

        this.RegisterForEvent<AddTagsSearchFilter>(x => AddTagsFilterFor(x.Tags));

        this.RegisterForEvent<AddDateSearchFilter>(x => AddAnnualDateFilterFor(x.Month, x.Day));

        this.RegisterForEvent<SearchForFiles>(async x =>
        {
            FilterSettings.Clear();
            AddFileListFilterFor(x.FileList);
            await FindFilesFromFiltersAsync();
        });

        this.RegisterForEvent<SearchForPerson>(async x =>
        {
            FilterSettings.Clear();
            AddPersonFilterFor(x.Person);
            await FindFilesFromFiltersAsync();
        });

        this.RegisterForEvent<SearchForPersonGroup>(async x =>
        {
            FilterSettings.Clear();
            AddPersonGroupFilterFor(x.Persons);
            await FindFilesFromFiltersAsync();
        });

        this.RegisterForEvent<SearchForLocation>(async x =>
        {
            FilterSettings.Clear();
            AddLocationFilterFor(x.Location);
            await FindFilesFromFiltersAsync();
        });

        this.RegisterForEvent<SearchForTag>(async x =>
        {
            FilterSettings.Clear();
            AddTagFilterFor(x.Tag);
            await FindFilesFromFiltersAsync();
        });

        this.RegisterForEvent<SearchForTags>(async x =>
        {
            FilterSettings.Clear();
            AddTagsFilterFor(x.Tags);
            await FindFilesFromFiltersAsync();
        });

        this.RegisterForEvent<SearchForAnnualDate>(async x =>
        {
            FilterSettings.Clear();
            AddAnnualDateFilterFor(x.Month, x.Day);
            await FindFilesFromFiltersAsync();
        });
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
    private void FindAllFiles()
    {
        Send(dbAccessProvider.DbAccess.GetFiles());
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
        var vm = ServiceLocator.Resolve<FilterSelectionViewModel>();
        AddFilter(vm);
    }

    private void AddFilter(FilterSelectionViewModel viewModel)
    {
        viewModel.IsFirstFilter = FilterSettings.Count == 0;
        FilterSettings.Add(viewModel);
        OnPropertyChanged(nameof(FilterCanBeRemoved));
    }

    private void AddFileListFilterFor(string fileListIds)
    {
        var vm = ServiceLocator.Resolve<FilterSelectionViewModel>();
        vm.SelectedFilterType = FilterType.FileList;
        var fileListViewModel = (FileListViewModel)vm.FilterViewModel;
        fileListViewModel.FileListIds = fileListIds;
        AddFilter(vm);
    }

    private void AddPersonFilterFor(PersonModel person)
    {
        var vm = ServiceLocator.Resolve<FilterSelectionViewModel>();
        vm.SelectedFilterType = FilterType.Person;
        var personViewModel = (PersonViewModel)vm.FilterViewModel;
        personViewModel.SelectedPerson = personViewModel.Persons.First(p => p.Id == person.Id);
        AddFilter(vm);
    }

    private void AddPersonGroupFilterFor(IEnumerable<PersonModel> persons)
    {
        var vm = ServiceLocator.Resolve<FilterSelectionViewModel>();
        vm.SelectedFilterType = FilterType.PersonGroup;
        var personGroupViewModel = (PersonGroupViewModel)vm.FilterViewModel;
        personGroupViewModel.SelectedPersons.Clear();
        foreach (var person in persons)
        {
            personGroupViewModel.SelectedPersons.Add(personGroupViewModel.Persons.First(p => p.Id == person.Id));
        }
        AddFilter(vm);
    }

    private void AddLocationFilterFor(LocationModel location)
    {
        var vm = ServiceLocator.Resolve<FilterSelectionViewModel>();
        vm.SelectedFilterType = FilterType.Location;
        var locationViewModel = (LocationViewModel)vm.FilterViewModel;
        locationViewModel.SelectedLocation = locationViewModel.Locations.First(p => p.Id == location.Id);
        AddFilter(vm);
    }

    private void AddTagFilterFor(TagModel tag)
    {
        var vm = ServiceLocator.Resolve<FilterSelectionViewModel>();
        vm.SelectedFilterType = FilterType.Tag;
        var tagViewModel = (TagViewModel)vm.FilterViewModel;
        tagViewModel.SelectedTag = tagViewModel.Tags.First(p => p.Id == tag.Id);
        AddFilter(vm);
    }

    private void AddTagsFilterFor(IEnumerable<TagModel> tags)
    {
        var vm = ServiceLocator.Resolve<FilterSelectionViewModel>();
        vm.SelectedFilterType = FilterType.Tags;
        var tagsViewModel = (Search.Filters.TagsViewModel)vm.FilterViewModel;
        tagsViewModel.SelectedTags.Clear();
        foreach (var tag in tags)
        {
            tagsViewModel.SelectedTags.Add(tagsViewModel.Tags.First(x => x.Id == tag.Id));
        }
        AddFilter(vm);
    }

    private void AddAnnualDateFilterFor(int month, int day)
    {
        var vm = ServiceLocator.Resolve<FilterSelectionViewModel>();
        vm.SelectedFilterType = FilterType.AnnualDate;
        var annualDateViewModel = (AnnualDateViewModel)vm.FilterViewModel;
        annualDateViewModel.SelectedAnnualMonthStart = month;
        annualDateViewModel.SelectedAnnualDayStart = day;
        AddFilter(vm);
    }

    [RelayCommand]
    private void RemoveFilter(FilterSelectionViewModel vm)
    {
        FilterSettings.Remove(vm);
        foreach (var filter in FilterSettings)
        {
            filter.IsFirstFilter = FilterSettings.IndexOf(filter) == 0;
        }
        OnPropertyChanged(nameof(FilterCanBeRemoved));
    }

    [RelayCommand]
    private async Task FindFilesFromFiltersAsync()
    {
        var fileModelComparer = new FileModelByIdComparer();
        var filters = FilterSettings.Select(x => (viewModel: x, filter: x.FilterViewModel.CreateFilter())).ToList();
        var filtersWithInvalidSettings = filters.Where(x => !x.filter.CanRun());
        if (filtersWithInvalidSettings.Any())
        {
            await dialogs.ShowErrorDialogAsync($"Invalid settings for filters: {string.Join(", ", filtersWithInvalidSettings.Select(x => x.viewModel.SelectedFilterType.ToFriendlyString()))}");
            return;
        }

        var result = Enumerable.Empty<FileModel>();
        foreach (var filterSettings in filters)
        {
            var filter = filterSettings.filter;
            var files = filter.Run(dbAccessProvider.DbAccess);

            if (filter == filters.First().filter)
            {
                result = files;
            }
            else
            {
                result = filterSettings.viewModel.SelectedCombineMethod switch
                {
                    CombineMethod.And => result.Intersect(files, fileModelComparer),
                    CombineMethod.Or => result.Union(files, fileModelComparer),
                    CombineMethod.Xor => result.Except(files, fileModelComparer).Union(files.Except(result, fileModelComparer), fileModelComparer),
                    _ => throw new NotImplementedException(),
                };
            }
        }

        Send(result);
    }
}
