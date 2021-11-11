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
                var locationModel = Utils.FileDBHandle.GetLocationById(locationId.Value);
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
                    Utils.FileDBHandle.UpdateLocationName(locationId.Value, name);
                    Utils.FileDBHandle.UpdateLocationDescription(locationId.Value, newDescription);
                    Utils.FileDBHandle.UpdateLocationPosition(locationId.Value, newGeoLocation);
                }
                else
                {
                    Utils.FileDBHandle.InsertLocation(name, newDescription, newGeoLocation);
                }
            }
            catch (FileDBDataValidationException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }
        }
    }
}
