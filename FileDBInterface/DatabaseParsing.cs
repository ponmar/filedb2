using System;
using System.Globalization;
using MetadataExtractor;

namespace FileDBInterface
{
    public class DatabaseParsing
    {
        public static DateTime ParsePersonsDateOfBirth(string dateOfBirthStr)
        {
            return DateTime.ParseExact(dateOfBirthStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static string ToPersonsDateOfBirth(DateTime dateOfBirth)
        {
            return dateOfBirth.ToString("yyyy-MM-dd");
        }

        public static DateTime ParsePersonsDeceased(string deceasedStr)
        {
            return DateTime.ParseExact(deceasedStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static string ToPersonsDeceased(DateTime deceased)
        {
            return deceased.ToString("yyyy-MM-dd");
        }

        // TODO: ParseFilesDatetime()?

        public static string DateTakenToFilesDatetime(DateTime dateTaken)
        {
            return dateTaken.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public static string PathToFilesDatetime(string path)
        {
            // TODO: use regexp
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

        public static (double lat, double lon)? ParseFilesPosition(string positionString)
        {
            var positionParts = positionString.Split(" ");
            if (positionParts.Length != 2)
            {
                return null;
            }

            if (!double.TryParse(positionParts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude))
            {
                return null;
            }

            if (!double.TryParse(positionParts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude))
            {
                return null;
            }

            return (latitude, longitude);
        }

        public static string ToFilesPosition(double lat, double lon)
        {
            return $"{lat.ToString(CultureInfo.InvariantCulture)} {lon.ToString(CultureInfo.InvariantCulture)}";
        }

        public static string ToFilesPosition(GeoLocation geoLocation)
        {
            return geoLocation != null ? ToFilesPosition(geoLocation.Latitude, geoLocation.Longitude) : null;
        }
    }
}
