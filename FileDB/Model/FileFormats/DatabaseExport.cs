using System;
using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDB.Model.FileFormats;

public record DatabaseExport(
    string FileDBVersion,
    DateTime ExportDateTime,
    List<PersonModel> Persons,
    List<LocationModel> Locations,
    List<TagModel> Tags,
    List<FileModel> Files);