using System.Windows.Input;
using FileDBInterface.Exceptions;

namespace FileDB.ViewModel
{
    public class AddLocationViewModel : ViewModelBase
    {
        private readonly int? locationId;

        public string Title
        {
            get => title;
            set { SetProperty(ref title, value); }
        }
        private string title;

        public string Name
        {
            get => name;
            set { SetProperty(ref name, value); }
        }
        private string name = string.Empty;

        public string Description
        {
            get => description;
            set { SetProperty(ref description, value); }
        }
        private string description = string.Empty;

        public string LatLon
        {
            get => latLon;
            set { SetProperty(ref latLon, value); }
        }
        private string latLon = string.Empty;

        public ICommand SaveCommand => saveCommand ??= new CommandHandler(Save);
        private ICommand saveCommand;

        public AddLocationViewModel(int? locationId = null)
        {
            this.locationId = locationId;

            Title = locationId.HasValue ? "Edit Location" : "Add Location";

            if (locationId.HasValue)
            {
                var locationModel = Utils.FileDB2Handle.GetLocationById(locationId.Value);
                Name = locationModel.name;
                Description = locationModel.description ?? string.Empty;
                LatLon = locationModel.position ?? string.Empty;
            }
        }

        public void Save()
        {
            try
            {
                string newDescription = string.IsNullOrEmpty(description) ? null : description;
                string newGeoLocation = string.IsNullOrEmpty(latLon) ? null : latLon;

                if (locationId.HasValue)
                {
                    Utils.FileDB2Handle.UpdateLocationName(locationId.Value, name);
                    Utils.FileDB2Handle.UpdateLocationDescription(locationId.Value, newDescription);
                    Utils.FileDB2Handle.UpdateLocationPosition(locationId.Value, newGeoLocation);
                }
                else
                {
                    Utils.FileDB2Handle.InsertLocation(name, newDescription, newGeoLocation);
                }
            }
            catch (FileDBDataValidationException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }
        }
    }
}
