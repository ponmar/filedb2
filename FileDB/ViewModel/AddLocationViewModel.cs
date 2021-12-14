using System.Windows.Input;
using FileDBInterface.Exceptions;
using FileDBInterface.Model;

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

        public string Position
        {
            get => position;
            set { SetProperty(ref position, value); }
        }
        private string position = string.Empty;

        public ICommand SaveCommand => saveCommand ??= new CommandHandler(Save);
        private ICommand saveCommand;

        private readonly Model.Model model = Model.Model.Instance;

        public AddLocationViewModel(int? locationId = null)
        {
            this.locationId = locationId;

            Title = locationId.HasValue ? "Edit Location" : "Add Location";

            if (locationId.HasValue)
            {
                var locationModel = model.DbAccess.GetLocationById(locationId.Value);
                Name = locationModel.Name;
                Description = locationModel.Description ?? string.Empty;
                Position = locationModel.Position ?? string.Empty;
            }
        }

        public void Save()
        {
            try
            {
                string newDescription = string.IsNullOrEmpty(description) ? null : description;
                string newPosition = string.IsNullOrEmpty(position) ? null : position;

                var location = new LocationModel()
                {
                    Id = locationId.HasValue ? locationId.Value : default,
                    Name = name,
                    Description = newDescription,
                    Position = newPosition
                };

                if (locationId.HasValue)
                {
                    model.DbAccess.UpdateLocation(location);
                }
                else
                {
                    model.DbAccess.InsertLocation(location);
                }
            }
            catch (DataValidationException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }
        }
    }
}
