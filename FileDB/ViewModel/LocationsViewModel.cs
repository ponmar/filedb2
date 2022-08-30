using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Sorters;
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

    public partial class LocationsViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool readWriteMode = !Model.Model.Instance.Config.ReadOnly;

        public ObservableCollection<Location> Locations { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedLocationHasPosition))]
        private Location selectedLocation;

        public bool SelectedLocationHasPosition => selectedLocation != null && !string.IsNullOrEmpty(selectedLocation.Position);

        private readonly Model.Model model = Model.Model.Instance;

        public LocationsViewModel()
        {
            ReloadLocations();
            model.LocationsUpdated += Model_LocationsUpdated;
            model.ConfigLoaded += Model_ConfigLoaded;
        }

        private void Model_ConfigLoaded(object sender, System.EventArgs e)
        {
            ReadWriteMode = !model.Config.ReadOnly;
        }

        private void Model_LocationsUpdated(object sender, System.EventArgs e)
        {
            ReloadLocations();
        }

        [RelayCommand]
        private void RemoveLocation()
        {
            if (Dialogs.ShowConfirmDialog($"Remove {selectedLocation.Name}?"))
            {
                var filesWithLocation = model.DbAccess.SearchFilesWithLocations(new List<int>() { selectedLocation.GetId() }).ToList();
                if (filesWithLocation.Count == 0 || Dialogs.ShowConfirmDialog($"Location is used in {filesWithLocation.Count} files, remove anyway?"))
                {
                    model.DbAccess.DeleteLocation(selectedLocation.GetId());
                    model.NotifyLocationsUpdated();
                }
            }
        }

        [RelayCommand]
        private void EditLocation()
        {
            var window = new AddLocationWindow(selectedLocation.GetId())
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        [RelayCommand]
        private void AddLocation()
        {
            var window = new AddLocationWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        [RelayCommand]
        private void ShowLocationOnMap()
        {
            var link = Utils.CreatePositionLink(selectedLocation.Position, model.Config.LocationLink);
            if (link != null)
            {
                Utils.OpenUriInBrowser(link);
            }
        }

        [RelayCommand]
        private void LocationSelection(Location parameter)
        {
            SelectedLocation = parameter;
        }

        private void ReloadLocations()
        {
            Locations.Clear();

            var locations = model.DbAccess.GetLocations().ToList();
            locations.Sort(new LocationModelByNameSorter());
            foreach (var location in locations.Select(lm => new Location(lm.Id) { Name = lm.Name, Description = lm.Description, Position = lm.Position }))
            {
                Locations.Add(location);
            }
        }
    }
}
