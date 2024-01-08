using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Extensions;
using FileDB.Model;
using FileDB.Resources;
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

    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;

    public LocationModel? AffectedLocation { get; private set; }

    public AddLocationViewModel(IDatabaseAccessProvider dbAccessProvider, IDialogs dialogs, int? locationId = null)
    {
        this.dbAccessProvider = dbAccessProvider;
        this.dialogs = dialogs;
        this.locationId = locationId;

        title = locationId.HasValue ? Strings.AddLocationEditTitle : Strings.AddLocationAddTitle;

        if (locationId.HasValue)
        {
            var locationModel = dbAccessProvider.DbAccess.GetLocationById(locationId.Value);
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
            string? newDescription = Description.HasContent() ? Description : null;
            string? newPosition = Position.HasContent() ? Position : null;

            var location = new LocationModel()
            {
                Id = locationId ?? default,
                Name = Name,
                Description = newDescription,
                Position = newPosition
            };

            if (locationId.HasValue)
            {
                dbAccessProvider.DbAccess.UpdateLocation(location);
                AffectedLocation = dbAccessProvider.DbAccess.GetLocationById(location.Id);
            }
            else
            {
                if (dbAccessProvider.DbAccess.GetLocations().Any(x => x.Name == location.Name))
                {
                    dialogs.ShowErrorDialog($"Location '{location.Name}' already added");
                    return;
                }

                dbAccessProvider.DbAccess.InsertLocation(location);
                AffectedLocation = dbAccessProvider.DbAccess.GetLocations().First(x => x.Name == location.Name);
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
