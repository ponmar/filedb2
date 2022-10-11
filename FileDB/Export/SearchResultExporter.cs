using FileDBInterface.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileDB.Export;

public class SearchResultExporter
{
    private readonly string destinationDirectory;
    private readonly string header;

    public SearchResultExporter(string destinationDirectory, string header)
    {
        this.destinationDirectory = destinationDirectory;
        this.header = header;
    }

    public void Export(List<FilesModel> files)
    {
        var data = GetExportedData(files);

        var jsonPath = Path.Combine(destinationDirectory, "data.json");
        var htmlPath = Path.Combine(destinationDirectory, "index.html");
        var m3uPath = Path.Combine(destinationDirectory, "playlist.m3u");

        new SearchResultFilesExporter().Export(data, destinationDirectory);
        new SearchResultFilesWithOverlayExporter().Export(data, destinationDirectory);
        new SearchResultJsonExporter().Export(data, jsonPath);
        new SearchResultM3uExporter().Export(data, m3uPath);
        new SearchResultHtmlExporter().Export(data, htmlPath);
    }

    private SearchResultFileFormat GetExportedData(List<FilesModel> files)
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
