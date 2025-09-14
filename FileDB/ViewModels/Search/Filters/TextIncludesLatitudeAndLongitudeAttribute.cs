using System.ComponentModel.DataAnnotations;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public sealed class TextIncludesLatitudeAndLongitudeAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var positionText = value as string ?? string.Empty;
        var gpsPos = ParsePositionFromTextOrUrl(positionText);

        var lat = gpsPos?.lat;
        var lon = gpsPos?.lon;

        return lat is not null && lon is not null
            ? ValidationResult.Success!
            : new("Invalid latitude and longitude text");
    }

    public static (double lat, double lon)? ParsePositionFromTextOrUrl(string positionText)
    {
        var gpsPos = DatabaseParsing.ParseFilesPositionFromUrl(positionText);
        gpsPos ??= DatabaseParsing.ParseFilesPosition(positionText);
        return gpsPos is null ? null : gpsPos;
    }
}