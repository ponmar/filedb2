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
    public class LocationsViewModel
    {
        private readonly FileDB2Handle fileDB2Handle;

        public ObservableCollection<LocationModel> Locations { get; } = new ObservableCollection<LocationModel>();

        public LocationsViewModel(FileDB2Handle fileDB2Handle)
        {
            this.fileDB2Handle = fileDB2Handle;

            foreach (var location in fileDB2Handle.GetLocations())
            {
                Locations.Add(location);
            }
        }
    }
}
