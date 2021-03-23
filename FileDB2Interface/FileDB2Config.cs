using System;
using System.Collections.Generic;
using System.Text;

namespace FileDB2Interface
{
    public class FileDB2Config
    {
        public string Database { get; set; }
        public string FilesRootDirectory { get; set; }
        public double FileToLocationMaxDistance { get; set; } = 300;
    }
}
