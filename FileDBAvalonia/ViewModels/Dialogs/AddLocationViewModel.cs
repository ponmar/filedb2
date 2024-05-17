using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBAvalonia.Dialogs;
using FileDBAvalonia.Lang;
using FileDBAvalonia.Model;
using FileDBInterface.Exceptions;
using FileDBInterface.Extensions;
using FileDBInterface.Model;
using System.Linq;
using System.Threading.Tasks;

namespace FileDBAvalonia.ViewModels.Dialogs;

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
                    await dialogs.ShowErrorDialogAsync($"Location '{location.Name}' already added");
                    return;
                }

                dbAccessProvider.DbAccess.InsertLocation(location);
                AffectedLocation = dbAccessProvider.DbAccess.GetLocations().First(x => x.Name == location.Name);
            }

            Messenger.Send<CloseModalDialogRequest>();
            Messenger.Send<LocationEdited>();
        }
        catch (DataValidationException e)
        {
            await dialogs.ShowErrorDialogAsync(e.Message);
        }
    }
}
