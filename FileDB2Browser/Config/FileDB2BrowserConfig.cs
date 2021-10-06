using System;
using System.Collections.Generic;

namespace FileDB2Browser.Config
{
    public class FileDB2BrowserConfig
    {
        public string Database { get; set; }
        public string FilesRootDirectory { get; set; }
        public List<string> BlacklistedFilePathPatterns { get; set; }
        public List<string> WhitelistedFilePathPatterns { get; set; }
        public bool IncludeHiddenDirectories { get; set; }
        public TimeSpan SlideshowDelay { get; set; }
        public int SearchHistorySize { get; set; }
    }
}
