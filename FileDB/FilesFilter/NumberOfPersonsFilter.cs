﻿using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.FilesFilter;

public class NumberOfPersonsFilter(string minText, string maxText) : IFilesFilter
{
    public bool CanRun() => 
        int.TryParse(minText, out var min) &&
        int.TryParse(maxText, out var max) &&
        min >= 0 && min <= max;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        var min = int.Parse(minText);
        var max = int.Parse(maxText);

        // TODO: create optimized db query 
        return dbAccess.GetFiles().Where(x =>
        {
            var numPersons = dbAccess.GetPersonsFromFile(x.Id).Count();
            return numPersons >= min && numPersons <= max;
        });
    }
}
