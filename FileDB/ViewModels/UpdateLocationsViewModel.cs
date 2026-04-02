using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.Extensions;
using FileDB.Lang;
using FileDB.Model;
using FileDBInterface.Extensions;
using FileDBInterface.Model;

namespace FileDB.ViewModels;

public partial class UpdateLocationsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string FilterText { get; set; } = string.Empty;

    partial void OnFilterTextChanged(string value)
    {
        FilterLocations();
    }

    [ObservableProperty]
    public partial bool ReadOnly { get; set; }
    public ObservableCollection<LocationModel> Locations { get; } = [];

    private readonly List<LocationModel> allLocations = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedLocationHasPosition))]
    public partial LocationModel? SelectedLocation { get; set; }

    public bool SelectedLocationHasPosition => SelectedLocation is not null && SelectedLocation.Position.HasContent();

    private readonly IConfigProvider configProvider;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;
    private readonly ILocationsRepository locationsRepository;

    public UpdateLocationsViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IDialogs dialogs, ILocationsRepository locationsRepository)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.dialogs = dialogs;
        this.locationsRepository = locationsRepository;
        ReadOnly = configProvider.Config.ReadOnly;

        ReloadLocations();

        this.RegisterForEvent<LocationsUpdated>((x) => ReloadLocations());

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            ReadOnly = configProvider.Config.ReadOnly;
        });
    }

    [RelayCommand]
    private async Task RemoveLocationAsync()
    {
        if (!await dialogs.ShowConfirmDialogAsync(string.Format(Strings.UpdateLocationsRemoveSelectedLocation, SelectedLocation!.Name)))
        {
            return;
        }

        
        var filesWithLocation = dbAccessProvider.DbAccess.SearchFilesWithLocations([SelectedLocation.Id]).ToList();
        if (filesWithLocation.Count == 0 || await dialogs.ShowConfirmDialogAsync(string.Format(Strings.UpdateLocationsRemoveUsedLocation, filesWithLocation.Count)))
        {
            dbAccessProvider.DbAccess.DeleteLocation(SelectedLocation.Id);
            Messenger.Send<LocationEdited>();
        }
    }

    [RelayCommand]
    private void EditLocation()
    {
        dialogs.ShowAddLocationDialogAsync(SelectedLocation!.Id);
    }

    [RelayCommand]
    private void AddLocation()
    {
        dialogs.ShowAddLocationDialogAsync(locationName: FilterText);
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
    private void LocationSelection(LocationModel parameter)
    {
        SelectedLocation = parameter;
    }

    private void ReloadLocations()
    {
        allLocations.Clear();
        allLocations.AddRange(locationsRepository.Locations);
        FilterLocations();
    }

    private void FilterLocations()
    {
        Locations.Clear();
        foreach (var tag in allLocations.Where(x => x.MatchesTextFilter(FilterText)))
        {
            Locations.Add(tag);
        }
    }

    [RelayCommand]
    private void ClearFilterText() => FilterText = string.Empty;
}
