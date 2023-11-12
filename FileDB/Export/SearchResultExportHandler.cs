using FileDB.Export.SearchResult;
using FileDB.Model;
using FileDBShared.FileFormats;
using FileDBShared.Model;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace FileDB.Export;

public enum SearchResultExportType
{
    Files,
    Html,
    M3u,
    FilesWithMetaData,
    Json,
    Pdf
}

public class SearchResultExportHandler
{
    private readonly IDbAccessProvider dbAccessProvider;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IFileSystem fileSystem;

    public SearchResultExportHandler(IDbAccessProvider dbAccessProvider, IFilesystemAccessProvider filesystemAccessProvider, IFileSystem fileSystem)
    {
        this.dbAccessProvider = dbAccessProvider;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.fileSystem = fileSystem;
    }

    public void Export(string destinationDirectory, string name, List<FileModel> files, List<SearchResultExportType> exportTypes)
    {
        var data = GetExportedData(files, name, "Files");

        if (exportTypes.Contains(SearchResultExportType.Files))
        {
            new FilesExporter().Export(data, destinationDirectory);
        }

        if (exportTypes.Contains(SearchResultExportType.FilesWithMetaData))
        {
            var filesWithDataDirPath = Path.Combine(destinationDirectory, "FilesWithData");
            var exporter = new FilesWithOverlayExporter(filesystemAccessProvider, DescriptionPlacement.Subtitle, fileSystem);
            exporter.Export(data, filesWithDataDirPath);
        }

        if (exportTypes.Contains(SearchResultExportType.Json))
        {
            var path = Path.Combine(destinationDirectory, "Export.json");
            var exporter = new JsonExporter(fileSystem);
            exporter.Export(data, path);
        }

        if (exportTypes.Contains(SearchResultExportType.M3u))
        {
            var path = Path.Combine(destinationDirectory, "Export.m3u");
            var exporter = new M3uExporter(fileSystem);
            exporter.Export(data, path);
        }

        if (exportTypes.Contains(SearchResultExportType.Html))
        {
            var path = Path.Combine(destinationDirectory, "Html");
            var exporter = new HtmlExporter(fileSystem, filesystemAccessProvider);
            exporter.Export(data, path);
        }

        if (exportTypes.Contains(SearchResultExportType.Pdf))
        {
            var path = Path.Combine(destinationDirectory, "Export.pdf");
            var exporter = new PdfExporter(fileSystem, filesystemAccessProvider, PageSizes.A4.Landscape());
            exporter.Export(data, path);
        }
    }

    private SearchResultExport GetExportedData(List<FileModel> files, string name, string filesSubdir)
    {
        var exportedFiles = new List<ExportedFile>();
        var persons = new List<PersonModel>();
        var locations = new List<LocationModel>();
        var tags = new List<TagModel>();

        int index = 1;
        foreach (var file in files)
        {
            var filePersons = dbAccessProvider.DbAccess.GetPersonsFromFile(file.Id);
            foreach (var person in filePersons)
            {
                if (!persons.Any(x => x.Id == person.Id))
                {
                    persons.Add(person);
                }
            }

            var fileLocations = dbAccessProvider.DbAccess.GetLocationsFromFile(file.Id);
            foreach (var location in fileLocations)
            {
                if (!locations.Any(x => x.Id == location.Id))
                {
                    locations.Add(location);
                }
            }

            var fileTags = dbAccessProvider.DbAccess.GetTagsFromFile(file.Id);
            foreach (var tag in fileTags)
            {
                if (!tags.Any(x => x.Id == tag.Id))
                {
                    tags.Add(tag);
                }
            }

            var exportedFilePath = Path.Combine(filesSubdir, $"{index}{Path.GetExtension(file.Path)}");

            exportedFiles.Add(new ExportedFile(
                file.Id,
                exportedFilePath,
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

        return new SearchResultExport(
            name,
            Utils.GetVersionString(),
            DateTime.Now,
            Utils.CreateFileList(files.Select(x => x.Id)),
            exportedFiles,
            persons,
            locations,
            tags,
            Utils.ApplicationProjectUrl);
    }
}
