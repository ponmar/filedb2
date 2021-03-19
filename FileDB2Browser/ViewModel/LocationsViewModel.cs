using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileDB2Interface;
using FileDB2Interface.Model;

namespace FileDB2Browser.ViewModel
{
    public class Location
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Position { get; set; }
    }

    public class LocationsViewModel
    {
        public ObservableCollection<Location> Locations { get; }

        public LocationsViewModel(FileDB2Handle fileDB2Handle)
        {
            var locations = fileDB2Handle.GetLocations().Select(lm => new Location() { Name = lm.name, Description = lm.description, Position = lm.position });
            Locations = new ObservableCollection<Location>(locations);
        }
    }
}
