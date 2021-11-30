using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;
using log4net;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace FileDBInterface.FilesystemAccess
{
    public class FilesystemAccess : IFilesystemAccess
    {
        private static readonly ILog log = LogManager.GetLogger(nameof(FilesystemAccess));

        private readonly string filesRootDirectory;

        public FilesystemAccess(string filesRootDirectory)
        {
            this.filesRootDirectory = filesRootDirectory;
            if (System.IO.Directory.Exists(filesRootDirectory))
            {
                log.Info($"Using files root directory: {filesRootDirectory}");
            }
            else
            {
                log.Warn($"Files root directory does not exist: {filesRootDirectory}");
            }
        }

        public IEnumerable<string> ListNewFilesystemFiles(IEnumerable<string> blacklistedFilePathPatterns, IEnumerable<string> whitelistedFilePathPatterns, bool includeHiddenDirectories, IFilesAccess filesDbAccess)
        {
            foreach (var filename in System.IO.Directory.GetFiles(filesRootDirectory, "*.*", SearchOption.AllDirectories))
            {
                var internalPath = ToInternalFilesPath(filename);
                if (!PathIsBlacklisted(internalPath, blacklistedFilePathPatterns) &&
                    PathIsWhitelisted(internalPath, whitelistedFilePathPatterns) &&
                    PathIsVisible(internalPath, includeHiddenDirectories) &&
                    filesDbAccess.GetFileByPath(internalPath) == null)
                {
                    yield return internalPath;
                }
            }
        }

        private bool PathIsBlacklisted(string internalPath, IEnumerable<string> blacklistedFilePathPatterns)
        {
            return blacklistedFilePathPatterns.FirstOrDefault(pattern => internalPath.IndexOf(pattern) != -1) != null;
        }

        private bool PathIsWhitelisted(string internalPath, IEnumerable<string> whitelistedFilePathPatterns)
        {
            if (whitelistedFilePathPatterns.Count() == 0)
                return true;

            var pathLower = internalPath.ToLower();
            return whitelistedFilePathPatterns.FirstOrDefault(pattern => pathLower.EndsWith(pattern)) != null;
        }

        private bool PathIsVisible(string internalPath, bool includeHiddenDirectories)
        {
            return includeHiddenDirectories || !PathIsHidden(internalPath);
        }

        private bool PathIsHidden(string internalPath)
        {
            return internalPath.StartsWith('.') || internalPath.IndexOf("/.") != -1;
        }

        public IEnumerable<string> ListAllFilesystemDirectories()
        {
            var dirs = System.IO.Directory.GetDirectories(filesRootDirectory, "*.*", SearchOption.AllDirectories);
            return dirs.Select(p => ToInternalFilesPath(p));
        }

        public IEnumerable<FilesModel> GetFilesMissingInFilesystem(IEnumerable<FilesModel> allFiles)
        {
            foreach (var file in allFiles)
            {
                if (!File.Exists(ToAbsolutePath(file.Path)))
                {
                    yield return file;
                }
            }
        }

        public string ToAbsolutePath(string internalPath)
        {
            var path = Path.Join(filesRootDirectory, internalPath);
            return path.Replace('\\', '/');
        }

        private string ToInternalFilesPath(string path)
        {
            if (path.StartsWith(filesRootDirectory))
            {
                path = path.Substring(filesRootDirectory.Length);
            }
            return FixInternalPath(path);
        }

        private string FixInternalPath(string internalPath)
        {
            internalPath = internalPath.Replace('\\', '/');
            while (internalPath.StartsWith('/'))
            {
                internalPath = internalPath.Substring(1);
            }
            return internalPath;
        }

        public FileMetadata ParseFileMetadata(string path)
        {
            var result = new FileMetadata() { AbsolutePath = path };

            ParseFileExif(path, out var dateTaken, out var location);

            if (dateTaken != null)
            {
                result.Datetime = DatabaseParsing.DateTakenToFilesDatetime(dateTaken.Value);
            }
            else
            {
                result.Datetime = DatabaseParsing.PathToFilesDatetime(path);
            }

            result.Position = location != null ? DatabaseParsing.ToFilesPosition(location.Latitude, location.Longitude) : null;
            return result;
        }

        private void ParseFileExif(string path, out DateTime? dateTaken, out GeoLocation location)
        {
            dateTaken = null;
            location = null;

            try
            {
                var directories = ImageMetadataReader.ReadMetadata(path);

                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                dateTaken = subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);

                var gps = directories.OfType<GpsDirectory>().FirstOrDefault();
                location = gps?.GetGeoLocation();
            }
            catch (IOException)
            {
            }
            catch (ImageProcessingException)
            {
            }
        }
    }
}
