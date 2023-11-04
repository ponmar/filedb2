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
    private readonly IDbAccessRepository dbAccessRepository;
    private readonly IFilesystemAccessRepository filesystemAccessRepository;
    private readonly IFileSystem fileSystem;

    public SearchResultExportHandler(IDbAccessRepository dbAccessRepository, IFilesystemAccessRepository filesystemAccessRepository, IFileSystem fileSystem)
    {
        this.dbAccessRepository = dbAccessRepository;
        this.filesystemAccessRepository = filesystemAccessRepository;
        this.fileSystem = fileSystem;
    }

    public void Export(string destinationDirectory, string header, List<FileModel> files, List<SearchResultExportType> exportTypes)
    {
        var data = GetExportedData(files, header, "UnmodifiedFiles");

        if (exportTypes.Contains(SearchResultExportType.Files))
        {
            new FilesExporter().Export(data, destinationDirectory);
        }

        if (exportTypes.Contains(SearchResultExportType.FilesWithMetaData))
        {
            var filesWithDataDirPath = Path.Combine(destinationDirectory, "FilesWithData");
            var exporter = new FilesWithOverlayExporter(filesystemAccessRepository, DescriptionPlacement.Subtitle, fileSystem);
            exporter.Export(data, filesWithDataDirPath);
        }

        if (exportTypes.Contains(SearchResultExportType.Json))
        {
            var path = Path.Combine(destinationDirectory, "data.json");
            var exporter = new JsonExporter(fileSystem);
            exporter.Export(data, path);
        }

        if (exportTypes.Contains(SearchResultExportType.M3u))
        {
            var path = Path.Combine(destinationDirectory, "playlist.m3u");
            var exporter = new M3uExporter(fileSystem);
            exporter.Export(data, path);
        }

        if (exportTypes.Contains(SearchResultExportType.Html))
        {
            var path = Path.Combine(destinationDirectory, "Html");
            var exporter = new HtmlExporter(fileSystem, filesystemAccessRepository);
            exporter.Export(data, path);
        }

        if (exportTypes.Contains(SearchResultExportType.Pdf))
        {
            var path = Path.Combine(destinationDirectory, "export.pdf");
            var exporter = new PdfExporter(fileSystem, filesystemAccessRepository, PageSizes.A4.Landscape());
            exporter.Export(data, path);
        }
    }

    private SearchResultExport GetExportedData(List<FileModel> files, string header, string filesSubdir)
    {
        var exportedFiles = new List<ExportedFile>();
        var persons = new List<PersonModel>();
        var locations = new List<LocationModel>();
        var tags = new List<TagModel>();

        int index = 1;
        foreach (var file in files)
        {
            var filePersons = dbAccessRepository.DbAccess.GetPersonsFromFile(file.Id);
            foreach (var person in filePersons)
            {
                if (!persons.Any(x => x.Id == person.Id))
                {
                    persons.Add(person);
                }
            }

            var fileLocations = dbAccessRepository.DbAccess.GetLocationsFromFile(file.Id);
            foreach (var location in fileLocations)
            {
                if (!locations.Any(x => x.Id == location.Id))
                {
                    locations.Add(location);
                }
            }

            var fileTags = dbAccessRepository.DbAccess.GetTagsFromFile(file.Id);
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
