using System;
using System.Collections.Generic;

namespace FileDB2Browser.Config
{
    public record BrowserConfig(string Database, string FilesRootDirectory, List<string> BlacklistedFilePathPatterns, List<string> WhitelistedFilePathPatterns, bool IncludeHiddenDirectories, TimeSpan SlideshowDelay, int SearchHistorySize);
}
