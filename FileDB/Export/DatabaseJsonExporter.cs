﻿using FileDBShared.FileFormats;
using Newtonsoft.Json;
using System.IO;

namespace FileDB.Export;

public class DatabaseJsonExporter
{
    public void Export(DatabaseExport data, string filename)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(filename, json);
    }
}
