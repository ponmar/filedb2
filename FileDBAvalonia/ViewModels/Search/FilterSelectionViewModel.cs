using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels.Search.Filters;

namespace FileDBAvalonia.ViewModels.Search;

public partial class FilterSelectionViewModel : ObservableObject
{
    public static IEnumerable<FilterType> FilterTypes { get; } = Enum.GetValues<FilterType>().OrderBy(x => x.ToFriendlyString(), StringComparer.Ordinal);

    [ObservableProperty]
    private FilterType selectedFilterType;

    partial void OnSelectedFilterTypeChanged(FilterType value)
    {
        FilterViewModel = CreateFilterFromType(value);
    }

    [ObservableProperty]
    private IFilterViewModel filterViewModel;

    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IPersonsRepository personsRepository;
    private readonly ILocationsRepository locationsRepository;
    private readonly ITagsRepository tagsRepository;
    private readonly IFileSelector fileSelector;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public FilterSelectionViewModel(IDatabaseAccessProvider dbAccessProvider, IPersonsRepository personsRepository, ILocationsRepository locationsRepository, ITagsRepository tagsRepository, IFileSelector fileSelector)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        this.dbAccessProvider = dbAccessProvider;
        this.personsRepository = personsRepository;
        this.locationsRepository = locationsRepository;
        this.tagsRepository = tagsRepository;
        this.fileSelector = fileSelector;
        SelectedFilterType = FilterType.Text;
    }

    private IFilterViewModel CreateFilterFromType(FilterType filterType)
    {
        return filterType switch
        {
            FilterType.AnnualDate => new AnnualDateViewModel(fileSelector),
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
}

