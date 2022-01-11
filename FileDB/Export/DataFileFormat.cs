using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDB.Export
{
    public class ExportedFile
    {
        public int Id { get; set; }
        public string ExportedPath { get; set; }
        public string OriginalPath { get; set; }
        public string Description { get; set; }
        public string Datetime { get; set; }
        public string Position { get; set; }
        public List<int> PersonIds { get; set; }
        public List<int> LocationIds { get; set; }
        public List<int> TagIds { get; set; }
    }

    public class DataFileFormat
    {
        public string Header { get; set; }
        public string About { get; set; }
        public string FileList { get; set; }
        public List<ExportedFile> Files { get; set; }
        public List<PersonModel> Persons { get; set; }
        public List<LocationModel> Locations { get; set; }
        public List<TagModel> Tags { get; set; }
        public string ApplicationDownloadUrl { get; set; }
    }
}
