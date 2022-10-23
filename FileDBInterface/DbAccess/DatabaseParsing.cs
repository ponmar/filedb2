﻿using System;
using System.Globalization;
using System.IO;

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

        public static DateTime? ParseFilesDatetime(string? datetimeStr)
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

        public static string? PathToFilesDatetime(string path)
        {
            var filename = Path.GetFileNameWithoutExtension(path)!;
            if (DateTime.TryParseExact(filename, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateFromFilename))
            {
                return DateTakenToFilesDatetime(dateFromFilename);
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
}
