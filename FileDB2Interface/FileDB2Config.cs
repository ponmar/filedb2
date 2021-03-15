using System;
using System.Collections.Generic;
using System.Text;

namespace FileDB2Interface
{
    public class FileDB2Config
    {
        public string Database { get; set; }
        public string FilesRootDirectory { get; set; }
        public bool IncludeHiddenDirectories { get; set; } = false;
        public List<string> BlacklistedFilePathPatterns { get; set; } = new List<string>();
        public List<string> WhitelistedFilePathPatterns { get; set; } = new List<string>();
        public double FileToLocationMaxDistance { get; set; } = 300;
    }
}
