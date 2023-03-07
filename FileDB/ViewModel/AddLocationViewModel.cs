using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using FileDBShared.Model;
using System.Linq;

namespace FileDB.ViewModel;

public partial class AddLocationViewModel : ObservableObject
{
    private readonly int? locationId;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string? description = string.Empty;

    [ObservableProperty]
    private string? position = string.Empty;

    private readonly IDbAccess dbAccess;
    private readonly IDialogs dialogs;

    public LocationModel? AffectedLocation { get; private set; }

    public AddLocationViewModel(IDbAccess dbAccess, IDialogs dialogs, int? locationId = null)
    {
        this.dbAccess = dbAccess;
        this.dialogs = dialogs;
        this.locationId = locationId;

        title = locationId.HasValue ? "Edit Location" : "Add Location";

        if (locationId.HasValue)
        {
            var locationModel = dbAccess.GetLocationById(locationId.Value);
            Name = locationModel.Name;
            Description = locationModel.Description ?? string.Empty;
            Position = locationModel.Position ?? string.Empty;
        }
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            string? newDescription = string.IsNullOrEmpty(Description) ? null : Description;
            string? newPosition = string.IsNullOrEmpty(Position) ? null : Position;

            var location = new LocationModel()
            {
                Id = locationId ?? default,
                Name = Name,
                Description = newDescription,
                Position = newPosition
            };

            if (locationId.HasValue)
            {
                dbAccess.UpdateLocation(location);
                AffectedLocation = dbAccess.GetLocationById(location.Id);
            }
            else
            {
                if (dbAccess.GetLocations().Any(x => x.Name == location.Name))
                {
                    dialogs.ShowErrorDialog($"Location '{location.Name}' already added");
                    return;
                }

                dbAccess.InsertLocation(location);
                AffectedLocation = dbAccess.GetLocations().First(x => x.Name == location.Name);
            }

            WeakReferenceMessenger.Default.Send(new CloseModalDialogRequested());
            WeakReferenceMessenger.Default.Send(new LocationsUpdated());
        }
        catch (DataValidationException e)
        {
            dialogs.ShowErrorDialog(e.Message);
        }
    }
}
