﻿using System;
using System.Collections.Generic;
using System.Linq;
using FileDBInterface.Model;
using FileDBInterface.DatabaseAccess;
using FileDB.Model;
using FileDB.Extensions;
using FileDB.Lang;
using FileDB.ViewModels.Search;
using FileDBInterface.Utils;

namespace FileDB;

public class FileTextOverlayCreator
{
    public static string GetPersonsText(IDatabaseAccess dbAccess, FileModel file, string separator)
    {
        var persons = dbAccess.GetPersonsFromFile(file.Id);
        return GetPersonsText(file, persons, separator);
    }

    public static string GetPersonsText(FileModel file, IEnumerable<PersonModel> persons, string separator)
    {
        return GetPersonsText(file.Datetime, persons, separator);
    }

    public static string GetPersonsText(string? fileDateTime, IEnumerable<PersonModel> persons, string separator)
    {
        var personStrings = GetPersonsTexts(fileDateTime, persons);
        return string.Join(separator, personStrings);
    }

    public static IEnumerable<string> GetPersonsTexts(string? fileDateTime, IEnumerable<PersonModel> persons)
    {
        var personStrings = persons.Select(p => GetPersonText(p, fileDateTime)).ToList();
        personStrings.Sort();
        return personStrings;
    }

    private static string GetPersonText(PersonModel person, string? fileDateTime)
    {
        return $"{person.Firstname} {person.Lastname}{Utils.GetPersonAgeInFileString(fileDateTime, person.DateOfBirth)}";
    }

    public static IEnumerable<Person> GetPersons(IDatabaseAccess dbAccess, FileModel file)
    {
        var persons = dbAccess.GetPersonsFromFile(file.Id);
        return persons.Select(x => new Person(x, GetPersonText(x, file.Datetime))).OrderBy(x => x.Label);
    }

    public static string GetLocationsText(IDatabaseAccess dbAccess, FileModel file, string separator)
    {
        var locations = dbAccess.GetLocationsFromFile(file.Id);
        return GetLocationsText(locations, separator);
    }

    public static string GetLocationsText(IEnumerable<LocationModel> locations, string separator)
    {
        var locationStrings = GetLocationsTexts(locations);
        return string.Join(separator, locationStrings);
    }

    public static IEnumerable<string> GetLocationsTexts(IEnumerable<LocationModel> locations)
    {
        var locationStrings = locations.Select(l => l.Name).ToList();
        locationStrings.Sort();
        return locationStrings;
    }

    public static IEnumerable<Location> GetLocations(IConfigProvider configProvider, IDatabaseAccess dbAccess, FileModel file)
    {
        var locations = dbAccess.GetLocationsFromFile(file.Id);
        return GetLocations(configProvider, locations);
    }

    public static IEnumerable<Location> GetLocations(IConfigProvider configProvider, IEnumerable<LocationModel> locations)
    {
        return locations.OrderBy(x => x.Name).Select(x => new Location(x, x.Name, GetPositionUri(configProvider, x)));
    }

    public static string GetTagsText(IDatabaseAccess dbAccess, FileModel file, string separator)
    {
        var tags = dbAccess.GetTagsFromFile(file.Id);
        return GetTagsText(tags, separator);
    }

    public static string GetTagsText(IEnumerable<TagModel> tags, string separator)
    {
        var tagStrings = GetTagsTexts(tags);
        return string.Join(separator, tagStrings);
    }

    public static IEnumerable<string> GetTagsTexts(IEnumerable<TagModel> tags)
    {
        var tagStrings = tags.Select(t => t.Name).ToList();
        tagStrings.Sort();
        return tagStrings;
    }

    public static IEnumerable<Tag> GetTags(IDatabaseAccess dbAccess, FileModel file)
    {
        var tags = dbAccess.GetTagsFromFile(file.Id);
        return tags.Select(x => new Tag(x, x.Name)).OrderBy(x => x.Name);
    }

    public static string? GetFileDateTimeText(FileModel file)
    {
        var datetime = DatabaseParsing.ParseFilesDatetime(file.Datetime);
        if (datetime is null)
        {
            return null;
        }

        // Note: when no time is available the string is used to avoid including time 00:00
        var resultString = file.Datetime!.Contains('T') ? datetime.Value.ToDateAndTime() : file.Datetime;

        var now = DateTime.Now;
        int yearsAgo = TimeUtils.GetAgeInYears(now, datetime.Value);
        if (yearsAgo == 0 && now.Year == datetime.Value.Year)
        {
            resultString += Strings.SearchFileDateTimeThisYear;
        }
        else if (yearsAgo <= 1)
        {
            resultString += string.Format(Strings.SearchFileDateTimeYearAgo, yearsAgo);
        }
        else if (yearsAgo > 1)
        {
            resultString += string.Format(Strings.SearchFileDateTimeYearsAgo, yearsAgo);
        }
        return resultString;
    }

    public static string? GetShortPositionText(FileModel file)
    {
        return file.Position is null ? null : Utils.CreateShortFilePositionString(file.Position);
    }

    public static string? GetPositionUri(IConfigProvider configProvider, FileModel file)
    {
        return file.Position is null ? null : Utils.CreatePositionLink(file.Position, configProvider.Config.LocationLink);
    }

    public static string? GetPositionUri(IConfigProvider configProvider, LocationModel location)
    {
        return location.Position is null ? null : Utils.CreatePositionLink(location.Position, configProvider.Config.LocationLink);
    }
}