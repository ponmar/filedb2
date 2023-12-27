using System;
using System.Globalization;
using System.IO;

namespace FileDBShared.Model;

public class DatabaseParsing
{
    private static DateTime ParsePersonDate(string dateStr)
    {
        if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ||
            DateTime.TryParseExact(dateStr, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ||
            DateTime.TryParseExact(dateStr, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        {
            return result;
        }

        throw new ArgumentException($"Invalid person date string: {dateStr}");
    }

    public static DateTime ParsePersonDateOfBirth(string dateStr)
    {
        return ParsePersonDate(dateStr);
    }

    public static DateTime ParsePersonDeceasedDate(string dateStr)
    {
        return ParsePersonDate(dateStr);
    }

    public static DateTime? ParseFilesDatetime(string? datetimeStr)
    {
        if (datetimeStr is null)
        {
            return null;
        }

        if (DateTime.TryParseExact(datetimeStr, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ||
            DateTime.TryParseExact(datetimeStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ||
            DateTime.TryParseExact(datetimeStr, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ||
            DateTime.TryParseExact(datetimeStr, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        {
            return result;
        }

        return null;
    }

    public static string DateTakenToFilesDatetime(DateTime dateTaken)
    {
        return dateTaken.ToString("yyyy-MM-ddTHH:mm:ss");
    }

    public static string? PathToFilesDatetime(string path)
    {
        var filenameDateTime = Path.GetFileNameWithoutExtension(path)!;
        if (filenameDateTime.EndsWith("-1"))
        {
            filenameDateTime = filenameDateTime[..^2];
        }
        if (DateTime.TryParseExact(filenameDateTime, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateFromFilename))
        {
            return DateTakenToFilesDatetime(dateFromFilename);
        }

        if (filenameDateTime.StartsWith("IMG-"))
        {
            var filenameParts = filenameDateTime.Split("-");
            if (filenameParts.Length == 3 && DateTime.TryParseExact(filenameParts[1], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFromFilename))
            {
                return dateFromFilename.ToString("yyyy-MM-dd");
            }
        }

        var pathParts = path.Split('/');
        foreach (var pathPart in pathParts)
        {
            var words = pathPart.Split(" ");
            if (words.Length > 0)
            {
                var firstWord = words[0];
                if (DateTime.TryParseExact(firstWord, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var _))
                {
                    return firstWord;
                }
            }
        }
        return null;
    }

    public static (double lat, double lon)? ParseFilesPosition(string? positionString)
    {
        if (positionString is not null)
        {
            var parts = positionString.Split(" ");
            if (parts.Length == 2 &&
                double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) &&
                double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude) &&
                latitude >= -90.0 && latitude <= 90.0 &&
                longitude >= -180.0 && longitude <= 180.0)
            {
                return (latitude, longitude);
            }
        }
        return null;
    }

    public static (double lat, double lon)? ParseFilesPositionFromUrl(string? url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            var searchCriteria = "@";
            var latLonStartIndex = url.IndexOf(searchCriteria);

            if (latLonStartIndex == -1)
            {
                searchCriteria = "/maps?q=loc:";
                latLonStartIndex = url.IndexOf(searchCriteria);
                if (latLonStartIndex == -1)
                {
                    return null;
                }
            }

            latLonStartIndex += searchCriteria.Length;

            var latLonToEnd = url.Substring(latLonStartIndex);
            var parts = latLonToEnd.Split(",");
            if (parts.Length >= 2 &&
                double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) &&
                double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
            {
                return (latitude, longitude);
            }
        }
        return null;
    }

    public static string ToFilesPosition(double lat, double lon)
    {
        return $"{lat.ToString(CultureInfo.InvariantCulture)} {lon.ToString(CultureInfo.InvariantCulture)}";
    }

    public static int OrientationToDegrees(int? orientation)
    {
        return orientation switch
        {
            1 => 0,
            8 => 90,
            3 => 180,
            6 => 270,
            _ => 0,
        };
    }

    public static int DegreesToOrientation(int degrees)
    {
        return degrees switch
        {
            0 => 1,
            90 => 8,
            180 => 3,
            270 => 6,
            _ => 0,
        };
    }
}
