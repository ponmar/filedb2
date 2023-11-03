using FileDBShared.FileFormats;
using FileDBShared.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileDB.Export;

public class DatabaseExporter
{
    private readonly string destinationDirectory;

    public DatabaseExporter(string destinationDirectory)
    {
        this.destinationDirectory = destinationDirectory;
    }

    public void Export(List<PersonModel> persons, List<LocationModel> locations, List<TagModel> tags, List<FileModel> files)
    {
        var data = new DatabaseExport(Utils.GetVersionString(), DateTime.Now, persons, locations, tags, files);
        var filename = Path.Combine(destinationDirectory, "DatabaseExport.json");
        new DatabaseJsonExporter().Export(data, filename);
    }
}
