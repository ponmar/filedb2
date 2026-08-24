using System;
using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDB.Model.FileFormats;

public record ExportedPersonBoundingBox(int PersonId, double X, double Y, double Width, double Height);

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
    List<int> TagIds,
    List<ExportedPersonBoundingBox> BoundingBoxes);

public record M3uExportData(
    string Name,
    List<ExportedFile> Files);

public record RichExportData(
    string Name,
    string FileDBVersion,
    DateTime ExportDateTime,
    string ApplicationProjectUrl,
    List<ExportedFile> Files,
    List<PersonModel> Persons,
    List<LocationModel> Locations,
    List<TagModel> Tags);

public record JsonExportData(
    string Name,
    string FileDBVersion,
    DateTime ExportDateTime,
    string ApplicationProjectUrl,
    string FileList,
    List<ExportedFile> Files,
    List<PersonModel> Persons,
    List<LocationModel> Locations,
    List<TagModel> Tags);
