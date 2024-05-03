using FileDBInterface.DatabaseAccess;
using FileDBInterface.Extensions;
using FileDBShared.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileDBAvalonia.FilesFilter;

public class FilterText(string searchPattern, bool caseSensitive, bool includePersons, bool includeLocations, bool includeTags) : IFilesFilter
{
    public bool CanRun() => searchPattern.HasContent();

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        var result = dbAccess.SearchFiles(searchPattern, caseSensitive).ToList();
        
        var stringComparison = caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

        if (includePersons)
        {
            var machingPersons = dbAccess.GetPersons().Where(x => $"{x.Firstname} {x.Lastname}".Contains(searchPattern, stringComparison)).Select(x => x.Id);
            var filesWithPersons = dbAccess.SearchFilesWithPersons(machingPersons);
            result.AddRange(filesWithPersons);
        }

        if (includeLocations)
        {
            var machingLocations = dbAccess.GetLocations().Where(x => x.Name.Contains(searchPattern, stringComparison)).Select(x => x.Id);
            var filesWithLocations = dbAccess.SearchFilesWithLocations(machingLocations);
            result.AddRange(filesWithLocations);
        }

        if (includeTags)
        {
            var machingTags = dbAccess.GetTags().Where(x => x.Name.Contains(searchPattern, stringComparison)).Select(x => x.Id);
            var filesWithTags = dbAccess.SearchFilesWithTags(machingTags);
            result.AddRange(filesWithTags);
        }

        return result;
    }
}
