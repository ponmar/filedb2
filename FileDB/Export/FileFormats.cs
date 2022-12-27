using FileDBShared.Model;
using System;
using System.Collections.Generic;

namespace FileDB.Export;

public record ExportedFile(
    int Id,
    string ExportedPath,
    string OriginalPath,
    string? Description,
    string? Datetime,
    string? Position,
    int? Orientation,
    List<int> PersonIds,
    List<int> LocationIds,
    List<int> TagIds);

public record SearchResultFileFormat(
    string Header,
    string About,
    string FileList,
    List<ExportedFile> Files,
    List<PersonModel> Persons,
    List<LocationModel> Locations,
    List<TagModel> Tags,
    string ApplicationDownloadUrl);

public record ExportedDatabaseFileFormat(
    string FileDBVersion,
    DateTime ExportDateTime,
    List<PersonModel> Persons,
    List<LocationModel> Locations,
    List<TagModel> Tags,
    List<FilesModel> Files);