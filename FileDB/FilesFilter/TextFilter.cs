﻿using FileDB.Comparers;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Extensions;
using FileDBInterface.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.FilesFilter;

public class TextFilter(string searchPattern, bool caseSensitive, bool includePersons, bool includeLocations, bool includeTags, IPersonsRepository personsRepo, ILocationsRepository locationsRepo, ITagsRepository tagsRepo) : IFilesFilter
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
            var machingPersons = personsRepo.Persons.Where(x => $"{x.Firstname} {x.Lastname}".Contains(searchPattern, stringComparison) || (x.Description is not null && x.Description.Contains(searchPattern, stringComparison))).Select(x => x.Id);
            filesWithPersons = dbAccess.SearchFilesWithPersons(machingPersons);
        }

        IEnumerable<FileModel> filesWithLocations = [];
        if (includeLocations)
        {
            var machingLocations = locationsRepo.Locations.Where(x => x.Name.Contains(searchPattern, stringComparison) || (x.Description is not null && x.Description.Contains(searchPattern, stringComparison))).Select(x => x.Id);
            filesWithLocations = dbAccess.SearchFilesWithLocations(machingLocations);
        }

        IEnumerable<FileModel> filesWithTags = [];
        if (includeTags)
        {
            var machingTags = tagsRepo.Tags.Where(x => x.Name.Contains(searchPattern, stringComparison)).Select(x => x.Id);
            filesWithTags = dbAccess.SearchFilesWithTags(machingTags);
        }

        return files
            .Union(filesWithPersons, fileModelComparer)
            .Union(filesWithLocations, fileModelComparer)
            .Union(filesWithTags, fileModelComparer);
    }
}
