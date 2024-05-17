using System;
using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDBInterface.FileFormats;

public enum FileType
{
    Picture,
    Movie,
    Document,
    Audio,
    Unknown,
}

public record ExportedFile(
    int Id,
    string ExportedPath,
    string OriginalPath,
    FileType FileType,
    string? Description,
    string? Datetime,
    string? Position,
    int? Orientation,
    List<int> PersonIds,
    List<int> LocationIds,
    List<int> TagIds);

public record SearchResultExport(
    string Name,
    string FileDBVersion,
    DateTime ExportDateTime,
    string FileList,
    List<ExportedFile> Files,
    List<PersonModel> Persons,
    List<LocationModel> Locations,
    List<TagModel> Tags,
    string ApplicationProjectUrl);

public record DatabaseExport(
    string FileDBVersion,
    DateTime ExportDateTime,
    List<PersonModel> Persons,
    List<LocationModel> Locations,
    List<TagModel> Tags,
    List<FileModel> Files);