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
        var fileContent = new DatabaseExport(Utils.GetVersionString(), DateTime.Now, persons, locations, tags, files);

        // TODO: where to check that directory is empty?
        var filename = Path.Combine(destinationDirectory, "DatabaseExport.json");
        new DatabaseJsonExporter().Export(fileContent, filename);
    }
}
