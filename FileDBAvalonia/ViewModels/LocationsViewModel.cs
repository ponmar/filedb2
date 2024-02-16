using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBAvalonia.Dialogs;
using FileDBAvalonia.Model;
using FileDBInterface.Extensions;

namespace FileDBAvalonia.ViewModels;

public record Location(int Id, string Name, string? Description, string? Position);

public partial class LocationsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool readWriteMode;

    public ObservableCollection<Location> Locations { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedLocationHasPosition))]
    private Location? selectedLocation;

    public bool SelectedLocationHasPosition => SelectedLocation is not null && SelectedLocation.Position.HasContent();

    private readonly IConfigProvider configProvider;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;
    private readonly ILocationsRepository locationsRepository;

    public LocationsViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IDialogs dialogs, ILocationsRepository locationsRepository)
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
    private async Task RemoveLocationAsync()
    {
        if (!await dialogs.ShowConfirmDialogAsync($"Remove {SelectedLocation!.Name}?"))
        {
            return;
        }

        
        var filesWithLocation = dbAccessProvider.DbAccess.SearchFilesWithLocations(new List<int>() { SelectedLocation.Id }).ToList();
        if (filesWithLocation.Count == 0 || await dialogs.ShowConfirmDialogAsync($"Location is used in {filesWithLocation.Count} files, remove anyway?"))
        {
            dbAccessProvider.DbAccess.DeleteLocation(SelectedLocation.Id);
            Messenger.Send<LocationEdited>();
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
        if (link is not null)
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
