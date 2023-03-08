using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Sorters;
using FileDBInterface.DbAccess;

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

    public bool SelectedLocationHasPosition => SelectedLocation != null && !string.IsNullOrEmpty(SelectedLocation.Position);

    private Config config;
    private readonly IDbAccess dbAccess;
    private readonly IDialogs dialogs;

    public LocationsViewModel(Config config, IDbAccess dbAccess, IDialogs dialogs)
    {
        this.config = config;
        this.dbAccess = dbAccess;
        this.dialogs = dialogs;

        readWriteMode = !config.ReadOnly;

        ReloadLocations();

        this.RegisterForEvent<LocationsUpdated>((x) => ReloadLocations());

        this.RegisterForEvent<ConfigLoaded>((x) =>
        {
            this.config = x.Config;
            ReadWriteMode = !this.config.ReadOnly;
        });
    }

    [RelayCommand]
    private void RemoveLocation()
    {
        if (dialogs.ShowConfirmDialog($"Remove {SelectedLocation!.Name}?"))
        {
            var filesWithLocation = dbAccess.SearchFilesWithLocations(new List<int>() { SelectedLocation.Id }).ToList();
            if (filesWithLocation.Count == 0 || dialogs.ShowConfirmDialog($"Location is used in {filesWithLocation.Count} files, remove anyway?"))
            {
                dbAccess.DeleteLocation(SelectedLocation.Id);
                Events.Send<LocationsUpdated>();
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
        var link = Utils.CreatePositionLink(SelectedLocation!.Position, config.LocationLink);
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

        var locations = dbAccess.GetLocations().ToList();
        locations.Sort(new LocationModelByNameSorter());
        foreach (var location in locations.Select(lm => new Location(lm.Id, lm.Name, lm.Description, lm.Position)))
        {
            Locations.Add(location);
        }
    }
}
