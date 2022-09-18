using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBInterface.Exceptions;
using FileDBInterface.Model;
using System.Linq;

namespace FileDB.ViewModel
{
    public partial class AddLocationViewModel : ObservableObject
    {
        private int? locationId;

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
                string? newDescription = string.IsNullOrEmpty(description) ? null : description;
                string? newPosition = string.IsNullOrEmpty(position) ? null : position;

                var location = new LocationModel()
                {
                    Id = locationId ?? default,
                    Name = name,
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
                    model.DbAccess.InsertLocation(location);
                    AffectedLocation = model.DbAccess.GetLocations().First(x => x.Name == location.Name);
                }                

                model.NotifyLocationsUpdated();
            }
            catch (DataValidationException e)
            {
                Dialogs.ShowErrorDialog(e.Message);
            }
        }
    }
}
