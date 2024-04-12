using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class PositionViewModel : AbstractFilterViewModel
{
    [ObservableProperty]
    private string positionText = string.Empty;

    [ObservableProperty]
    private string radiusText = "500";

    public ObservableCollection<LocationForSearch> LocationsWithPosition { get; } = [];

    [ObservableProperty]
    private LocationForSearch? selectedLocationsWithPosition = null;

    partial void OnSelectedLocationsWithPositionChanged(LocationForSearch? value)
    {
        if (value is not null)
        {
            var location = databaseAccessProvider.DbAccess.GetLocationById(value.Id);
            PositionText = location.Position!;
        }
    }

    [ObservableProperty]
    private bool currentFileHasPosition; // TODO: update value

    private readonly ILocationsRepository locationsRepository;
    private readonly IDatabaseAccessProvider databaseAccessProvider;

    public PositionViewModel(ILocationsRepository locationsRepository, IDatabaseAccessProvider databaseAccessProvider) : base(FilterType.Position)
    {
        this.locationsRepository = locationsRepository;
        this.databaseAccessProvider = databaseAccessProvider;
        ReloadLocations();
        this.RegisterForEvent<LocationsUpdated>(x => ReloadLocations());
    }

    private void ReloadLocations()
    {
        LocationsWithPosition.Clear();
        foreach (var location in locationsRepository.Locations)
        {
            var locationToUpdate = new LocationForSearch(location.Id, location.Name);
            if (location.Position is not null)
            {
                LocationsWithPosition.Add(locationToUpdate);
            }
        }
    }

    [RelayCommand]
    private void UsePositionFromCurrentFile()
    {
        // TODO
    }

    protected override IFilesFilter DoCreate() => new FilterPosition(PositionText, RadiusText);
}
