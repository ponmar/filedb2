using System;
using System.Globalization;

namespace FileDBInterface.DbAccess
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

        public static DateTime? ParseFilesDatetime(string datetimeStr)
        {
            DateTime? result = null;

            if (datetimeStr != null)
            {
                if (DateTime.TryParseExact(datetimeStr, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var datetime1))
                {
                    result = datetime1;
                }
                else if (DateTime.TryParseExact(datetimeStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var datetime2))
                {
                    result = datetime2;
                }
                else if (DateTime.TryParseExact(datetimeStr, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var datetime3))
                {
                    result = datetime3;
                }
            }

            return result;
        }

        public static string DateTakenToFilesDatetime(DateTime dateTaken)
        {
            return dateTaken.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public static string PathToFilesDatetime(string path)
        {
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
            if (positionString != null)
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

        public static string ToFilesPosition(double lat, double lon)
        {
            return $"{lat.ToString(CultureInfo.InvariantCulture)} {lon.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
