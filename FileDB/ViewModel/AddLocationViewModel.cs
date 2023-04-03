using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    private readonly IDbAccessRepository dbAccessRepository;
    private readonly IDialogs dialogs;

    public LocationModel? AffectedLocation { get; private set; }

    public AddLocationViewModel(IDbAccessRepository dbAccessRepository, IDialogs dialogs, int? locationId = null)
    {
        this.dbAccessRepository = dbAccessRepository;
        this.dialogs = dialogs;
        this.locationId = locationId;

        title = locationId.HasValue ? "Edit Location" : "Add Location";

        if (locationId.HasValue)
        {
            var locationModel = dbAccessRepository.DbAccess.GetLocationById(locationId.Value);
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
                dbAccessRepository.DbAccess.UpdateLocation(location);
                AffectedLocation = dbAccessRepository.DbAccess.GetLocationById(location.Id);
            }
            else
            {
                if (dbAccessRepository.DbAccess.GetLocations().Any(x => x.Name == location.Name))
                {
                    dialogs.ShowErrorDialog($"Location '{location.Name}' already added");
                    return;
                }

                dbAccessRepository.DbAccess.InsertLocation(location);
                AffectedLocation = dbAccessRepository.DbAccess.GetLocations().First(x => x.Name == location.Name);
            }

            Events.Send<CloseModalDialogRequest>();
            Events.Send<LocationEdited>();
        }
        catch (DataValidationException e)
        {
            dialogs.ShowErrorDialog(e.Message);
        }
    }
}
