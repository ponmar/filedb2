using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDB2Browser.Config
{
    public class FileDB2BrowserConfig
    {
        public string Database { get; set; } = "filedb2.db";
        public string FilesRootDirectory { get; set; } = "files";
        public List<string> BlacklistedFilePathPatterns { get; set; } = new List<string>() { "Thumbs.db", "filedb.db", "unsorted", "TN_" };
        public List<string> WhitelistedFilePathPatterns { get; set; } = new List<string>() { ".jpg", ".png", ".bmp", ".gif", ".avi", ".mpg", ".mp4", ".mkv", ".mov", ".pdf" };
        public bool IncludeHiddenDirectories { get; set; } = false;
        public TimeSpan SlideshowDelay { get; set; } = TimeSpan.FromSeconds(3);
    }
}
