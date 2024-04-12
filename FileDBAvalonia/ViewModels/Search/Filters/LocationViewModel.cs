using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;
using System.Collections.ObjectModel;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public record LocationForSearch(int Id, string Name)
{
    public override string ToString() => Name;
}

public partial class LocationViewModel : AbstractFilterViewModel
{
    [ObservableProperty]
    private ObservableCollection<LocationForSearch> locations = [];

    [ObservableProperty]
    private ObservableCollection<LocationForSearch> locationsWithPosition = [];

    [ObservableProperty]
    private LocationForSearch? selectedLocation;

    [ObservableProperty]
    private bool negate;

    private readonly ILocationsRepository locationsRepository;

    public LocationViewModel(ILocationsRepository locationsRepository) : base(FilterType.Location)
    {
        this.locationsRepository = locationsRepository;
        ReloadLocations();
        this.RegisterForEvent<LocationsUpdated>((x) => ReloadLocations());
    }

    private void ReloadLocations()
    {
        Locations.Clear();
        LocationsWithPosition.Clear();
        foreach (var location in locationsRepository.Locations)
        {
            Locations.Add(new(location.Id, location.Name));
            if (location.Position is not null)
            {
                LocationsWithPosition.Add(new(location.Id, location.Name));
            }
        }
    }

    protected override IFilesFilter DoCreate() =>
        Negate ? new FilterWithoutLocation(SelectedLocation) : new FilterLocation(SelectedLocation);
}
