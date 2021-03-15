using System;
using System.Collections.Generic;
using System.Text;

namespace FileDB2Interface
{
    public class FileDB2Config
    {
        public string Database { get; set; }
        public string FilesRootDirectory { get; set; }
        public bool IncludeHiddenDirectories { get; set; }
        public string BlacklistedFilePathPatterns { get; set; }
        public string WhitelistedFilePathPatterns { get; set; }
        public double FileToLocationMaxDistance { get; set; }
    }
}
