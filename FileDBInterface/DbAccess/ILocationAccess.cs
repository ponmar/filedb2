using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDBInterface.DbAccess
{
    public interface ILocationAccess
    {
        public IEnumerable<LocationModel> GetLocations();
        public LocationModel GetLocationById(int id);
        public bool HasLocationId(int id);
        public int GetLocationCount();
        public void InsertLocation(LocationModel location);
        public void UpdateLocation(LocationModel location);
        public void DeleteLocation(int id);
    }
}
