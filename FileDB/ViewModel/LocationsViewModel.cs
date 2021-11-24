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
        private bool readWriteMode = !Utils.Config.ReadOnly;

        public ObservableCollection<Location> Locations { get; } = new();

        private Location selectedLocation;

        public LocationsViewModel()
        {
            ReloadLocations();
        }

        public void RemoveLocation()
        {
            if (selectedLocation != null)
            {
                var filesWithLocation = Utils.DatabaseWrapper.GetFilesWithLocations(new List<int>() { selectedLocation.GetId() }).ToList();
                if (filesWithLocation.Count == 0 || Utils.ShowConfirmDialog($"Location is used in {filesWithLocation.Count} files, remove anyway?"))
                {
                    Utils.DatabaseWrapper.DeleteLocation(selectedLocation.GetId());
                    ReloadLocations();
                }
            }
        }

        public void EditLocation()
        {
            if (selectedLocation != null)
            {
                var window = new AddLocationWindow(selectedLocation.GetId())
                {
                    Owner = Application.Current.MainWindow
                };
                window.ShowDialog();
                ReloadLocations();
            }
        }

        public void AddLocation()
        {
            var window = new AddLocationWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            ReloadLocations();
        }

        public void LocationSelectionChanged(object parameter)
        {
            selectedLocation = (Location)parameter;
        }

        private void ReloadLocations()
        {
            Locations.Clear();

            var locations = Utils.DatabaseWrapper.GetLocations().Select(lm => new Location(lm.id) { Name = lm.name, Description = lm.description, Position = lm.position });
            foreach (var location in locations)
            {
                Locations.Add(location);
            }
        }
    }
}
