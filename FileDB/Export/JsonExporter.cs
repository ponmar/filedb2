﻿using Newtonsoft.Json;
using System.IO;

namespace FileDB.Export
{
    public class JsonExporter : IExporter
    {
        public void Export(DataFileFormat data, string filename)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}