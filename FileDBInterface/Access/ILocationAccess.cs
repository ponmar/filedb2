using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDBInterface.Access
{
    public interface ILocationAccess
    {
        public IEnumerable<LocationModel> GetLocations();
        public LocationModel GetLocationById(int id);
        public bool HasLocationId(int id);
        public int GetLocationCount();
        public void InsertLocation(string name, string description = null, string position = null);
        public void UpdateLocation(int id, string name, string description = null, string position = null);
        public void DeleteLocation(int id);
    }
}
