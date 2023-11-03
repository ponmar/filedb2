﻿using FileDBShared.FileFormats;
using FileDBShared.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace FileDB.Export;

public class DatabaseExporter
{
    private readonly string destinationDirectory;
    private readonly IFileSystem fileSystem;

    public DatabaseExporter(string destinationDirectory, IFileSystem fileSystem)
    {
        this.destinationDirectory = destinationDirectory;
        this.fileSystem = fileSystem;
    }

    public void Export(List<PersonModel> persons, List<LocationModel> locations, List<TagModel> tags, List<FileModel> files)
    {
        var data = new DatabaseExport(Utils.GetVersionString(), DateTime.Now, persons, locations, tags, files);
        var filename = Path.Combine(destinationDirectory, "DatabaseExport.json");
        new DatabaseJsonExporter(fileSystem).Export(data, filename);
    }
}
