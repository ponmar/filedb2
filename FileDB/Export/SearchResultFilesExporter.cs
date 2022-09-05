using System.IO;

namespace FileDB.Export
{
    public class SearchResultFilesExporter : ISearchResultExporter
    {
        public void Export(SearchResultFileFormat data, string path)
        {
            var model = Model.Model.Instance;
            foreach (var file in data.Files)
            {
                var sourceFilePath = model.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
                var destFilePath = Path.Combine(path, file.ExportedPath);
                var destDir = Path.GetDirectoryName(destFilePath)!;
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(sourceFilePath, destFilePath);
            }
        }
    }
}
