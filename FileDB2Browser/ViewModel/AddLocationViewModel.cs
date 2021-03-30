using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileDB2Interface;
using FileDB2Interface.Exceptions;
using MetadataExtractor;

namespace FileDB2Browser.ViewModel
{
    public class AddLocationViewModel : ViewModelBase
    {
        private readonly FileDB2Handle fileDB2Handle;
        private readonly int locationId;

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

        public ICommand AddLocationCommand
        {
            get
            {
                return addLocationCommand ??= new CommandHandler(AddLocation);
            }
        }
        private ICommand addLocationCommand;

        public AddLocationViewModel(FileDB2Handle fileDB2Handle, int locationId = -1)
        {
            this.fileDB2Handle = fileDB2Handle;
            this.locationId = locationId;

            Title = locationId == -1 ? "Add Location" : "Edit Location";

            if (locationId != -1)
            {
                var locationModel = fileDB2Handle.GetLocationById(locationId);
                Name = locationModel.name;
                Description = locationModel.description ?? string.Empty;
                LatLon = locationModel.position ?? string.Empty;
            }
        }

        public void AddLocation(object parameter)
        {
            try
            {
                string newDescription = string.IsNullOrEmpty(description) ? null : description;
                string newGeoLocation = string.IsNullOrEmpty(latLon) ? null : latLon;

                if (locationId == -1)
                {
                    fileDB2Handle.InsertLocation(name, newDescription, newGeoLocation);
                }
                else
                {
                    fileDB2Handle.UpdateLocationName(locationId, name);
                    fileDB2Handle.UpdateLocationDescription(locationId, newDescription);
                    fileDB2Handle.UpdateLocationPosition(locationId, newGeoLocation);
                }
            }
            catch (FileDB2DataValidationException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }
        }
    }
}
