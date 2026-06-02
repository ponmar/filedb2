using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.Infrastructure;
using FileDB.Lang;
using FileDB.Model;
using FileDBInterface.Exceptions;
using FileDBInterface.Extensions;
using FileDBInterface.Model;
using System.Linq;
using System.Threading.Tasks;

namespace FileDB.ViewModels.Dialogs;

public partial class AddLocationViewModel : ObservableObject
{
    private readonly int? locationId;

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Position { get; set; } = string.Empty;

    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;

    public LocationModel? AffectedLocation { get; private set; }

    public AddLocationViewModel(IDatabaseAccessProvider dbAccessProvider, IDialogs dialogs, int? locationId = null)
    {
        this.dbAccessProvider = dbAccessProvider;
        this.dialogs = dialogs;
        this.locationId = locationId;
        Title = locationId.HasValue ? Strings.AddLocationEditTitle : Strings.AddLocationAddTitle;

        if (locationId.HasValue)
        {
            var locationModel = dbAccessProvider.DbAccess.GetLocationById(locationId.Value);
            Name = locationModel.Name;
            Description = locationModel.Description ?? string.Empty;
            Position = locationModel.Position ?? string.Empty;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
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
                    await dialogs.ShowErrorDialogAsync(string.Format(Strings.AddLocationLocationAlreadyAdded, location.Name));
                    return;
                }

                var newLocationId = dbAccessProvider.DbAccess.InsertLocation(location);
                AffectedLocation = dbAccessProvider.DbAccess.GetLocationById(newLocationId);
            }

            Messenger.Send<LocationEdited>();
            Messenger.Send<CloseModalDialogRequest>();
        }
        catch (DataValidationException e)
        {
            await dialogs.ShowErrorDialogAsync(e.Message);
        }
    }
}
