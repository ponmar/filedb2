using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Extensions;
using FileDB.Model;

namespace FileDB.ViewModel;

public record Location(int Id, string Name, string? Description, string? Position);

public partial class LocationsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool readWriteMode;

    public ObservableCollection<Location> Locations { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedLocationHasPosition))]
    private Location? selectedLocation;

    public bool SelectedLocationHasPosition => SelectedLocation != null && SelectedLocation.Position.HasContent();

    private readonly IConfigProvider configProvider;
    private readonly IDbAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;
    private readonly ILocationsRepository locationsRepository;

    public LocationsViewModel(IConfigProvider configProvider, IDbAccessProvider dbAccessProvider, IDialogs dialogs, ILocationsRepository locationsRepository)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.dialogs = dialogs;
        this.locationsRepository = locationsRepository;

        readWriteMode = !configProvider.Config.ReadOnly;

        ReloadLocations();

        this.RegisterForEvent<LocationsUpdated>((x) => ReloadLocations());

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            ReadWriteMode = !configProvider.Config.ReadOnly;
        });
    }

    [RelayCommand]
    private void RemoveLocation()
    {
        if (dialogs.ShowConfirmDialog($"Remove {SelectedLocation!.Name}?"))
        {
            var filesWithLocation = dbAccessProvider.DbAccess.SearchFilesWithLocations(new List<int>() { SelectedLocation.Id }).ToList();
            if (filesWithLocation.Count == 0 || dialogs.ShowConfirmDialog($"Location is used in {filesWithLocation.Count} files, remove anyway?"))
            {
                dbAccessProvider.DbAccess.DeleteLocation(SelectedLocation.Id);
                Events.Send<LocationEdited>();
            }
        }
    }

    [RelayCommand]
    private void EditLocation()
    {
        dialogs.ShowAddLocationDialog(SelectedLocation!.Id);
    }

    [RelayCommand]
    private void AddLocation()
    {
        dialogs.ShowAddLocationDialog();
    }

    [RelayCommand]
    private void ShowLocationOnMap()
    {
        var link = Utils.CreatePositionLink(SelectedLocation!.Position, configProvider.Config.LocationLink);
        if (link != null)
        {
            Utils.OpenUriInBrowser(link);
        }
    }

    [RelayCommand]
    private void LocationSelection(Location parameter)
    {
        SelectedLocation = parameter;
    }

    private void ReloadLocations()
    {
        Locations.Clear();
        foreach (var location in locationsRepository.Locations.Select(lm => new Location(lm.Id, lm.Name, lm.Description, lm.Position)))
        {
            Locations.Add(location);
        }
    }
}
