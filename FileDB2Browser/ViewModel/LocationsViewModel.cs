using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FileDB2Browser.View;
using FileDB2Interface;
using FileDB2Interface.Model;

namespace FileDB2Browser.ViewModel
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
        public ICommand AddLocationCommand
        {
            get
            {
                return addLocationCommand ??= new CommandHandler(AddLocation);
            }
        }
        private ICommand addLocationCommand;

        public ICommand EditLocationCommand
        {
            get
            {
                return editLocationCommand ??= new CommandHandler(EditLocation);
            }
        }
        private ICommand editLocationCommand;

        public ICommand RemoveLocationCommand
        {
            get
            {
                return removeLocationCommand ??= new CommandHandler(RemoveLocation);
            }
        }
        private ICommand removeLocationCommand;

        public ICommand LocationSelectionCommand
        {
            get
            {
                return locationSelectionCommand ??= new CommandHandler(LocationSelectionChanged);
            }
        }
        private ICommand locationSelectionCommand;

        public ObservableCollection<Location> Locations { get; } = new ObservableCollection<Location>();

        private Location selectedLocation;

        private readonly FileDB2Handle fileDB2Handle;

        public LocationsViewModel(FileDB2Handle fileDB2Handle)
        {
            this.fileDB2Handle = fileDB2Handle;
            ReloadLocations();
        }

        public void RemoveLocation(object parameter)
        {
            if (selectedLocation != null)
            {
                // TODO: only delete if location not used in files?
                fileDB2Handle.DeleteLocation(selectedLocation.GetId());

                ReloadLocations();
            }
        }

        public void EditLocation(object parameter)
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

        public void AddLocation(object parameter)
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

            var locations = fileDB2Handle.GetLocations().Select(lm => new Location(lm.id) { Name = lm.name, Description = lm.description, Position = lm.position });
            foreach (var location in locations)
            {
                Locations.Add(location);
            }
        }
    }
}
