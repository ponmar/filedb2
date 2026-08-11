using FileDB.Model;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileDB.Export;

public interface ISearchResultExportDataBuilder
{
    List<ExportedFile> BuildFilesList(List<FileModel> files, CancellationToken cancellationToken = default);
    M3uExportData BuildForM3u(List<FileModel> files, string name, CancellationToken cancellationToken = default);
    RichExportData BuildRich(List<FileModel> files, string name, CancellationToken cancellationToken = default);
    JsonExportData BuildForJson(List<FileModel> files, string name, CancellationToken cancellationToken = default);
}

public class SearchResultExportDataBuilder(IDatabaseAccessProvider dbAccessProvider) : ISearchResultExportDataBuilder
{
    private const string FilesSubdir = "Files";

    public List<ExportedFile> BuildFilesList(List<FileModel> files, CancellationToken cancellationToken = default)
    {
        var exportedFiles = new List<ExportedFile>();
        int index = 1;
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var exportedFilePath = Path.Combine(FilesSubdir, $"{index}{Path.GetExtension(file.Path)}");
            exportedFiles.Add(new ExportedFile(
                file.Id,
                exportedFilePath,
                file.Path,
                FileTypeUtils.GetFileType(file.Path),
                file.Description,
                file.Datetime,
                file.Position,
                file.Orientation,
                [], [], []));
            index++;
        }
        return exportedFiles;
    }

    public M3uExportData BuildForM3u(List<FileModel> files, string name, CancellationToken cancellationToken = default)
    {
        var exportedFiles = BuildFilesList(files, cancellationToken);
        return new M3uExportData(name, exportedFiles);
    }

    public RichExportData BuildRich(List<FileModel> files, string name, CancellationToken cancellationToken = default)
    {
        var (exportedFiles, persons, locations, tags) = BuildWithMetadata(files, cancellationToken);
        return new RichExportData(
            name,
            Utils.GetVersionString(),
            DateTime.Now,
            Utils.ApplicationProjectUrl,
            exportedFiles,
            persons,
            locations,
            tags);
    }

    public JsonExportData BuildForJson(List<FileModel> files, string name, CancellationToken cancellationToken = default)
    {
        var (exportedFiles, persons, locations, tags) = BuildWithMetadata(files, cancellationToken);
        return new JsonExportData(
            name,
            Utils.GetVersionString(),
            DateTime.Now,
            Utils.ApplicationProjectUrl,
            Utils.CreateFileList(files.Select(x => x.Id)),
            exportedFiles,
            persons,
            locations,
            tags);
    }

    private (List<ExportedFile> Files, List<PersonModel> Persons, List<LocationModel> Locations, List<TagModel> Tags)
        BuildWithMetadata(List<FileModel> files, CancellationToken cancellationToken)
    {
        var exportedFiles = new List<ExportedFile>();
        var persons = new List<PersonModel>();
        var locations = new List<LocationModel>();
        var tags = new List<TagModel>();

        int index = 1;
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var filePersons = dbAccessProvider.DbAccess.GetPersonsFromFile(file.Id);
            foreach (var person in filePersons)
            {
                if (!persons.Any(x => x.Id == person.Id))
                    persons.Add(person);
            }

            var fileLocations = dbAccessProvider.DbAccess.GetLocationsFromFile(file.Id);
            foreach (var location in fileLocations)
            {
                if (!locations.Any(x => x.Id == location.Id))
                    locations.Add(location);
            }

            var fileTags = dbAccessProvider.DbAccess.GetTagsFromFile(file.Id);
            foreach (var tag in fileTags)
            {
                if (!tags.Any(x => x.Id == tag.Id))
                    tags.Add(tag);
            }

            var exportedFilePath = Path.Combine(FilesSubdir, $"{index}{Path.GetExtension(file.Path)}");
            exportedFiles.Add(new ExportedFile(
                file.Id,
                exportedFilePath,
                file.Path,
                FileTypeUtils.GetFileType(file.Path),
                file.Description,
                file.Datetime,
                file.Position,
                file.Orientation,
                filePersons.Select(x => x.Id).ToList(),
                fileLocations.Select(x => x.Id).ToList(),
                fileTags.Select(x => x.Id).ToList()));

            index++;
        }

        return (exportedFiles, persons, locations, tags);
    }
}

