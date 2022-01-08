using FileDBInterface.Model;
using System;
using System.Collections.Generic;

namespace FileDB.Export
{
    public class ExportedFile
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public string Datetime { get; set; }
        public string Position { get; set; }
        public List<int> PersonIds { get; set; }
        public List<int> LocationIds { get; set; }
        public List<int> TagIds { get; set; }
    }

    public class ExportedData
    {
        public string About { get; set; }
        public DateTime CreationDate { get; set; }
        public List<ExportedFile> Files { get; set; }
        public List<PersonModel> Persons { get; set; }
        public List<LocationModel> Locations { get; set; }
        public List<TagModel> Tags { get; set; }
    }
}
