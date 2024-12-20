﻿using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using FileDBInterface.Utils;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.FilesFilter;

public class PersonAgeFilter(string ageFromText, string ageToText) : IFilesFilter
{
    public bool CanRun() => 
        int.TryParse(ageFromText, out var ageFrom) &&
        int.TryParse(ageToText, out var ageTo) &&
        ageFrom >= 0 && ageFrom <= ageTo;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        var ageTo = int.Parse(ageToText);
        var ageFrom = int.Parse(ageFromText);

        var result = new List<FileModel>();
        var personsWithAge = dbAccess.GetPersons().Where(p => p.DateOfBirth is not null);

        foreach (var person in personsWithAge)
        {
            var dateOfBirth = DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth!);
            foreach (var file in dbAccess.SearchFilesWithPersons([person.Id]))
            {
                var fileDatetime = DatabaseParsing.ParseFilesDatetime(file.Datetime);
                if (fileDatetime is not null)
                {
                    int personAgeInFile = TimeUtils.GetAgeInYears(fileDatetime.Value, dateOfBirth);
                    if (personAgeInFile >= ageFrom && personAgeInFile <= ageTo)
                    {
                        result.Add(file);
                    }
                }
            }
        }

        return result;
    }
}
