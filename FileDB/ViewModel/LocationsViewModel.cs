using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FileDB.View;

namespace FileDB.ViewModel
{
    public class Location
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Position { get; set; }

        private readonly int id;

        public Location(int id)
        {
            this.id = id;
        }

        public int GetId()
        {
            return id;
        }
    }

    public class LocationsViewModel : ViewModelBase
    {
        public ICommand AddLocationCommand => addLocationCommand ??= new CommandHandler(AddLocation);
        private ICommand addLocationCommand;

        public ICommand EditLocationCommand => editLocationCommand ??= new CommandHandler(EditLocation);
        private ICommand editLocationCommand;

        public ICommand RemoveLocationCommand => removeLocationCommand ??= new CommandHandler(RemoveLocation);
        private ICommand removeLocationCommand;

        public ICommand LocationSelectionCommand => locationSelectionCommand ??= new CommandHandler(LocationSelectionChanged);
        private ICommand locationSelectionCommand;

        public bool ReadWriteMode
        {
            get => readWriteMode;
            set => SetProperty(ref readWriteMode, value);
        }
        private bool readWriteMode = !Model.Model.Instance.Config.ReadOnly;

        public ObservableCollection<Location> Locations { get; } = new();

        public Location SelectedLocation
        {
            get => selectedLocation;
            set => SetProperty(ref selectedLocation, value);
        }
        private Location selectedLocation;

        private readonly Model.Model model = Model.Model.Instance;

        public LocationsViewModel()
        {
            ReloadLocations();
            model.LocationsUpdated += Model_LocationsUpdated;
        }

        private void Model_LocationsUpdated(object sender, System.EventArgs e)
        {
            ReloadLocations();
        }

        public void RemoveLocation()
        {
            var filesWithLocation = model.DbAccess.SearchFilesWithLocations(new List<int>() { selectedLocation.GetId() }).ToList();
            if (filesWithLocation.Count == 0 || Utils.ShowConfirmDialog($"Location is used in {filesWithLocation.Count} files, remove anyway?"))
            {
                model.DbAccess.DeleteLocation(selectedLocation.GetId());
                model.NotifyLocationsUpdated();
            }
        }

        public void EditLocation()
        {
            var window = new AddLocationWindow(selectedLocation.GetId())
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        public void AddLocation()
        {
            var window = new AddLocationWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        public void LocationSelectionChanged(object parameter)
        {
            SelectedLocation = (Location)parameter;
        }

        private void ReloadLocations()
        {
            Locations.Clear();

            var locations = model.DbAccess.GetLocations().Select(lm => new Location(lm.Id) { Name = lm.Name, Description = lm.Description, Position = lm.Position });
            foreach (var location in locations)
            {
                Locations.Add(location);
            }
        }
    }
}
