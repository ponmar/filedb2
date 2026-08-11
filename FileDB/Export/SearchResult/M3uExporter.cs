using FileDB.Model.FileFormats;
using System.IO.Abstractions;

namespace FileDB.Export.SearchResult;

public interface IM3uExporter
{
    void Export(M3uExportData data, string filename);
}

public class M3uExporter(IFileSystem fileSystem) : IM3uExporter
{
    public void Export(M3uExportData data, string filename)
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
