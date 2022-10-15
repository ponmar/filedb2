using FileDBInterface.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileDB.Export;

public class SearchResultExporter
{
    public void Export(string destinationDirectory, string header, List<FilesModel> files, bool exportIncludesFiles, bool exportIncludesHtml, bool exportIncludesM3u, bool exportIncludesFilesWithMetaData, bool exportIncludesJson)
    {
        var data = GetExportedData(files, header);

        var jsonPath = Path.Combine(destinationDirectory, "data.json");
        var htmlPath = Path.Combine(destinationDirectory, "index.html");
        var m3uPath = Path.Combine(destinationDirectory, "playlist.m3u");

        if (exportIncludesFiles)
        {
            new SearchResultFilesExporter().Export(data, destinationDirectory);
        }

        if (exportIncludesFilesWithMetaData)
        {
            new SearchResultFilesWithOverlayExporter(DescriptionPlacement.Subtitle).Export(data, destinationDirectory);
        }

        if (exportIncludesJson)
        {
            new SearchResultJsonExporter().Export(data, jsonPath);
        }

        if (exportIncludesM3u)
        {
            new SearchResultM3uExporter().Export(data, m3uPath);
        }

        if (exportIncludesHtml)
        {
            new SearchResultHtmlExporter().Export(data, htmlPath);
        }
    }

    private SearchResultFileFormat GetExportedData(List<FilesModel> files, string header)
    {
        var model = Model.Model.Instance;

        var exportedFiles = new List<ExportedFile>();
        var persons = new List<PersonModel>();
        var locations = new List<LocationModel>();
        var tags = new List<TagModel>();

        int index = 1;
        foreach (var file in files)
        {
            var filePersons = model.DbAccess.GetPersonsFromFile(file.Id);
            foreach (var person in filePersons)
            {
                if (!persons.Any(x => x.Id == person.Id))
                {
                    persons.Add(person);
                }
            }

            var fileLocations = model.DbAccess.GetLocationsFromFile(file.Id);
            foreach (var location in fileLocations)
            {
                if (!locations.Any(x => x.Id == location.Id))
                {
                    locations.Add(location);
                }
            }

            var fileTags = model.DbAccess.GetTagsFromFile(file.Id);
            foreach (var tag in fileTags)
            {
                if (!tags.Any(x => x.Id == tag.Id))
                {
                    tags.Add(tag);
                }
            }

            exportedFiles.Add(new ExportedFile(
                file.Id,
                $"files/{index}{Path.GetExtension(file.Path)}",
                file.Path,
                file.Description,
                file.Datetime,
                file.Position,
                file.Orientation,
                filePersons.Select(x => x.Id).ToList(),
                fileLocations.Select(x => x.Id).ToList(),
                fileTags.Select(x => x.Id).ToList()));

            index++;
        }

        return new SearchResultFileFormat(
            header,
            $"Exported with {Utils.ApplicationName} version {Utils.GetVersionString()} at {DateTime.Now:yyyy-MM-dd HH:mm}",
            Utils.CreateFileList(files.Select(x => x.Id)),
            exportedFiles,
            persons,
            locations,
            tags,
            Utils.ApplicationDownloadUrl);
    }
}
