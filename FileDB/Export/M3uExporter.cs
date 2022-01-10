using System.IO;

namespace FileDB.Export
{
    public class M3uExporter : IExporter
    {
        public void Export(DataFileFormat data, string filename)
        {
            var m3uLinebreak = "\r\n";
            int duration = 10; // Note: VLC ignore the duration information when showing images in a playlist
            var content = $"#EXTM3U{m3uLinebreak}";
            content += $"#PLAYLIST:{data.Header}{m3uLinebreak}";
            foreach (var file in data.Files)
            {
                content += $"#EXTINF:{duration},{file.OriginalPath}{m3uLinebreak}";
                content += $"{file.ExportedPath}{m3uLinebreak}";
            }

            File.WriteAllText(filename, content);
        }
    }
}
