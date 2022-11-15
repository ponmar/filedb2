using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDB.Sorters;
using FileDB.View;

namespace FileDB.ViewModel;

public record Location(int Id, string Name, string? Description, string? Position);

public partial class LocationsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool readWriteMode = !Model.Model.Instance.Config.ReadOnly;

    public ObservableCollection<Location> Locations { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedLocationHasPosition))]
    private Location? selectedLocation;

    public bool SelectedLocationHasPosition => selectedLocation != null && !string.IsNullOrEmpty(selectedLocation.Position);

    private readonly Model.Model model = Model.Model.Instance;

    public LocationsViewModel()
    {
        ReloadLocations();

        WeakReferenceMessenger.Default.Register<LocationsUpdated>(this, (r, m) =>
        {
            ReloadLocations();
        });

        WeakReferenceMessenger.Default.Register<ConfigLoaded>(this, (r, m) =>
        {
            ReadWriteMode = !model.Config.ReadOnly;
        });
    }

    [RelayCommand]
    private void RemoveLocation()
    {
        if (Dialogs.Default.ShowConfirmDialog($"Remove {selectedLocation!.Name}?"))
        {
            var filesWithLocation = model.DbAccess.SearchFilesWithLocations(new List<int>() { selectedLocation.Id }).ToList();
            if (filesWithLocation.Count == 0 || Dialogs.Default.ShowConfirmDialog($"Location is used in {filesWithLocation.Count} files, remove anyway?"))
            {
                model.DbAccess.DeleteLocation(selectedLocation.Id);
                WeakReferenceMessenger.Default.Send(new LocationsUpdated());
            }
        }
    }

    [RelayCommand]
    private void EditLocation()
    {
        Dialogs.Default.ShowAddLocationDialog(selectedLocation!.Id);
    }

    [RelayCommand]
    private void AddLocation()
    {
        Dialogs.Default.ShowAddLocationDialog();
    }

    [RelayCommand]
    private void ShowLocationOnMap()
    {
        var link = Utils.CreatePositionLink(selectedLocation!.Position, model.Config.LocationLink);
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

        var locations = model.DbAccess.GetLocations().ToList();
        locations.Sort(new LocationModelByNameSorter());
        foreach (var location in locations.Select(lm => new Location(lm.Id, lm.Name, lm.Description, lm.Position)))
        {
            Locations.Add(location);
        }
    }
}
