using System.Collections.Generic;
using FileDBShared.Model;

namespace FileDBInterface.DatabaseAccess;

public interface ILocationAccess
{
    IEnumerable<LocationModel> GetLocations();
    LocationModel GetLocationById(int id);
    bool HasLocationId(int id);
    int GetLocationCount();
    IEnumerable<LocationModel> SearchLocationsNearGpsPosition(double latitude, double longitude, double radius);
    void InsertLocation(LocationModel location);
    void UpdateLocation(LocationModel location);
    void DeleteLocation(int id);
}
