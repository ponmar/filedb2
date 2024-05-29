using FileDBInterface.FileFormats;
using System.IO.Abstractions;

namespace FileDB.Export.SearchResult;

public class M3uExporter(IFileSystem fileSystem) : ISearchResultExporter
{
    public void Export(SearchResultExport data, string filename)
    {
        var m3uLinebreak = "\r\n";
        int duration = 10; // Note: VLC ignore the duration information when showing images in a playlist
        var content = $"#EXTM3U{m3uLinebreak}";
        content += $"#PLAYLIST:{data.Name}{m3uLinebreak}";
        foreach (var file in data.Files)
        {
            content += $"#EXTINF:{duration},{file.OriginalPath}{m3uLinebreak}";
            content += $"{file.ExportedPath}{m3uLinebreak}";
        }

        fileSystem.File.WriteAllText(filename, content);
    }
}
