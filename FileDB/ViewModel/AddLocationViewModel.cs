using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
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

    private readonly Model.Model model = Model.Model.Instance;

    public LocationModel? AffectedLocation { get; private set; }

    public AddLocationViewModel(int? locationId = null)
    {
        this.locationId = locationId;

        title = locationId.HasValue ? "Edit Location" : "Add Location";

        if (locationId.HasValue)
        {
            var locationModel = model.DbAccess.GetLocationById(locationId.Value);
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
                model.DbAccess.UpdateLocation(location);
                AffectedLocation = model.DbAccess.GetLocationById(location.Id);
            }
            else
            {
                if (model.DbAccess.GetLocations().Any(x => x.Name == location.Name))
                {
                    Dialogs.Instance.ShowErrorDialog($"Location '{location.Name}' already added");
                    return;
                }

                model.DbAccess.InsertLocation(location);
                AffectedLocation = model.DbAccess.GetLocations().First(x => x.Name == location.Name);
            }

            WeakReferenceMessenger.Default.Send(new CloseModalDialogRequested());
            WeakReferenceMessenger.Default.Send(new LocationsUpdated());
        }
        catch (DataValidationException e)
        {
            Dialogs.Instance.ShowErrorDialog(e.Message);
        }
    }
}
