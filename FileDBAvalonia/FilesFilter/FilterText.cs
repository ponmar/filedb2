using FileDBAvalonia.Comparers;
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

    private static readonly FileModelByIdComparer fileModelComparer = new();

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        var files = dbAccess.SearchFiles(searchPattern, caseSensitive).ToList();
        
        var stringComparison = caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

        IEnumerable<FileModel> filesWithPersons = [];
        if (includePersons)
        {
            var machingPersons = dbAccess.GetPersons().Where(x => $"{x.Firstname} {x.Lastname}".Contains(searchPattern, stringComparison)).Select(x => x.Id);
            filesWithPersons = dbAccess.SearchFilesWithPersons(machingPersons);
        }

        IEnumerable<FileModel> filesWithLocations = [];
        if (includeLocations)
        {
            var machingLocations = dbAccess.GetLocations().Where(x => x.Name.Contains(searchPattern, stringComparison)).Select(x => x.Id);
            filesWithLocations = dbAccess.SearchFilesWithLocations(machingLocations);
        }

        IEnumerable<FileModel> filesWithTags = [];
        if (includeTags)
        {
            var machingTags = dbAccess.GetTags().Where(x => x.Name.Contains(searchPattern, stringComparison)).Select(x => x.Id);
            filesWithTags = dbAccess.SearchFilesWithTags(machingTags);
        }

        return files
            .Union(filesWithPersons, fileModelComparer)
            .Union(filesWithLocations, fileModelComparer)
            .Union(filesWithTags, fileModelComparer);
    }
}
