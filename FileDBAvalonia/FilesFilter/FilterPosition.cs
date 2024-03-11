using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using System.Collections.Generic;
using System.Linq;

namespace FileDBAvalonia.FilesFilter;

public class FilterPosition : IFilesFilter
{
    private readonly double? lat;
    private readonly double? lon;
    private readonly double radius;

    public FilterPosition(string positionText, string radiusText)
    {
        var gpsPos = DatabaseParsing.ParseFilesPositionFromUrl(positionText);
        gpsPos ??= DatabaseParsing.ParseFilesPosition(positionText);

        lat = gpsPos?.lat;
        lon = gpsPos?.lon;

        double.TryParse(radiusText, out radius);
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
