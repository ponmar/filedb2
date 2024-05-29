using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.FilesFilter;

public class PositionFilter : IFilesFilter
{
    private readonly double? lat;
    private readonly double? lon;
    private readonly double radius;

    public PositionFilter(string positionText, string radiusText)
    {
        var gpsPos = DatabaseParsing.ParseFilesPositionFromUrl(positionText);
        gpsPos ??= DatabaseParsing.ParseFilesPosition(positionText);

        lat = gpsPos?.lat;
        lon = gpsPos?.lon;

        _ = double.TryParse(radiusText, out radius);
    }

    public bool CanRun() => lat is not null && lon is not null && radius > 0;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        var result = dbAccess.SearchFilesNearGpsPosition(lat!.Value, lon!.Value, radius).ToList();
        var nearLocations = dbAccess.SearchLocationsNearGpsPosition(lat!.Value, lon!.Value, radius);
        result.AddRange(dbAccess.SearchFilesWithLocations(nearLocations.Select(x => x.Id)));
        return result;
    }
}
