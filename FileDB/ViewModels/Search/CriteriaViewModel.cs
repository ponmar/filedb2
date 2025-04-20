using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using FileDBInterface.Model;
using System.Linq;
using System.Collections.ObjectModel;
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
    private ObservableCollection<FilterSelectionViewModel> filterSettings = [];

    public bool FilterCanBeRemoved => FilterSettings.Count > 1;
        
    private readonly IDialogs dialogs;
    private readonly IDatabaseAccessProvider dbAccessProvider;

    public CriteriaViewModel(IDialogs dialogs, IDatabaseAccessProvider dbAccessProvider)
    {
        this.dialogs = dialogs;
        this.dbAccessProvider = dbAccessProvider;

        var vm = ServiceLocator.Resolve<FilterSelectionViewModel>();
        vm.IsFirstFilter = true;
        filterSettings.Add(vm);

        this.RegisterForEvent<AddPersonSearchFilter>(x => AddPersonFilterFor(x.Person));

        this.RegisterForEvent<AddPersonGroupSearchFilter>(x => AddPersonGroupFilterFor(x.Persons));

        this.RegisterForEvent<AddLocationSearchFilter>(x => AddLocationFilterFor(x.Location));

        this.RegisterForEvent<AddTagSearchFilter>(x => AddTagFilterFor(x.Tag));

        this.RegisterForEvent<AddTagsSearchFilter>(x => AddTagsFilterFor(x.Tags));

        this.RegisterForEvent<AddDateSearchFilter>(x => AddDateFilterFor(x.Date));

        this.RegisterForEvent<AddAnnualDateSearchFilter>(x => AddAnnualDateFilterFor(x.Month, x.Day));

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

        this.RegisterForEvent<SearchForDate>(async x =>
        {
            FilterSettings.Clear();
            AddDateFilterFor(x.Date);
            await FindFilesFromFiltersAsync();
        });
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

    private void AddDateFilterFor(DateTime date)
    {
        var vm = ServiceLocator.Resolve<FilterSelectionViewModel>();
        vm.SelectedFilterType = FilterType.Date;
        var dateViewModel = (DateViewModel)vm.FilterViewModel;
        dateViewModel.FirstDateTime = date;
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

        Messenger.Send(new TransferSearchResult(result));
    }
}
