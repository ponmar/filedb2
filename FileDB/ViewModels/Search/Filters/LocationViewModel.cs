using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;
using FileDB.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileDB.ViewModels.Search.Filters;

public record LocationForSearch(int Id, string Name)
{
    public override string ToString() => Name;
}

public partial class LocationViewModel : ObservableObject, IFilterViewModel
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
    private readonly IFileSelector fileSelector;
    private readonly IDatabaseAccessProvider databaseAccessProvider;

    public LocationViewModel(ILocationsRepository locationsRepository, IFileSelector fileSelector, IDatabaseAccessProvider databaseAccessProvider)
    {
        this.locationsRepository = locationsRepository;
        this.fileSelector = fileSelector;
        this.databaseAccessProvider = databaseAccessProvider;
        ReloadLocations();
        this.RegisterForEvent<LocationsUpdated>((x) => ReloadLocations());
        TrySelectLocationFromSelectedFile();
    }

    private void TrySelectLocationFromSelectedFile()
    {
        if (fileSelector.SelectedFile is not null)
        {
            var locationsInFile = databaseAccessProvider.DbAccess.GetLocationsFromFile(fileSelector.SelectedFile.Id);
            if (locationsInFile.Any())
            {
                var firstLocationFromFile = locationsInFile.First();
                SelectedLocation = Locations.First(x => x.Id == firstLocationFromFile.Id);
            }
        }
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

    public IFilesFilter CreateFilter() =>
        Negate ? new WithoutLocationFilter(SelectedLocation) : new LocationFilter(SelectedLocation);
}
