using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Linq;
using Dapper;
using FileDB2Interface.Model;
using System.IO;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Directory = System.IO.Directory;

namespace FileDB2Interface
{
    public class FileDB2Handle
    {
        private readonly FileDB2Config config;

        public FileDB2Handle(FileDB2Config config)
        {
            this.config = config;

            if (!File.Exists(config.Database))
            {
                throw new FileDB2Exception($"Configuration file does not exist: {config.Database}");
            }

            if (!Directory.Exists(config.FilesRootDirectory))
            {
                throw new FileDB2Exception($"Root directory does not exist: {config.FilesRootDirectory}");
            }
        }

        #region Files collection

        public string[] ListAllFilesystemFiles()
        {
            var files = Directory.GetFiles(config.FilesRootDirectory, "*.*", SearchOption.AllDirectories);
            return PathsToInternalPaths(files);
        }

        public string[] ListAllFilesystemDirectories()
        {
            var dirs = Directory.GetDirectories(config.FilesRootDirectory, "*.*", SearchOption.AllDirectories);
            return PathsToInternalPaths(dirs);
        }

        public bool ParseFilesystemFileExif(string path, out DateTime? dateTaken, out GeoLocation location)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(path);

                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                dateTaken = subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);

                var gps = directories.OfType<GpsDirectory>().FirstOrDefault();
                location = gps?.GetGeoLocation();

                return true;
            }
            catch (IOException)
            {
            }
            catch (ImageProcessingException)
            {
            }

            dateTaken = null;
            location = null;
            return false;
        }

        #endregion

        #region Tools

        public List<FilesModel> GetFilesMissingInFilesystem()
        {
            var missingFiles = new List<FilesModel>();
            foreach (var file in GetFiles())
            {
                if (!File.Exists(InternalPathToPath(file.path)))
                {
                    missingFiles.Add(file);
                }
            }
            return missingFiles;
        }

        #endregion

        #region Files

        public List<FilesModel> GetFiles()
        {
            using var connection = CreateConnection();
            return connection.Query<FilesModel>("select * from files", new DynamicParameters()).ToList();
        }

        public List<FilesModel> SearchFiles(string criteria)
        {
            using var connection = CreateConnection();
            var sql = "select * from files where (path like @criteria or description like @criteria)";
            return connection.Query<FilesModel>(sql, new { criteria = "%" + criteria + "%" }).ToList();
        }

        public FilesModel GetFileById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = CreateConnection();
            return connection.QueryFirst<FilesModel>("select * from files where id = @ID", parameters);
        }

        public void DeleteFile(int id)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from files where id = @id";
            connection.Execute(sql, new { id = id });
        }

        public void InsertFilePerson(int fileId, int personId)
        {
            using var connection = CreateConnection();
            var sql = "insert into filepersons (fileid, personid) values (@fileId, @locationId)";
            connection.Execute(sql, new { fileId = fileId, personId = personId });
        }

        public void DeleteFilePerson(int fileId, int personId)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from filepersons where fileid = @fileId and personid = @personId";
            connection.Execute(sql, new { fileId = fileId, personId = personId });
        }

        public void InsertFileLocation(int fileId, int locationId)
        {
            using var connection = CreateConnection();
            var sql = "insert into filelocations (fileid, locationid) values (@fileId, @locationId)";
            connection.Execute(sql, new { fileId = fileId, locationId = locationId });
        }

        public void DeleteFileLocation(int fileId, int locationId)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from filelocations where fileid = @fileId and locationid = @locationId";
            connection.Execute(sql, new { fileId = fileId, locationId = locationId });
        }

        public void InsertFileTag(int fileId, int tagId)
        {
            using var connection = CreateConnection();
            var sql = "insert into filetags (fileid, tagid) values (@fileId, @tagId)";
            connection.Execute(sql, new { fileId = fileId, tagId = tagId });
        }

        public void DeleteFileTag(int fileId, int tagId)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from filetags where fileid = @fileId and tagid = @tagId";
            connection.Execute(sql, new { fileId = fileId, tagId = tagId });
        }

        #endregion

        #region Persons

        public List<PersonModel> GetPersons()
        {
            using var connection = CreateConnection();
            return connection.Query<PersonModel>("select * from persons", new DynamicParameters()).ToList();
        }

        public List<PersonModel> SearchPersons(string criteria)
        {
            using var connection = CreateConnection();
            var sql = "select * from persons where (firstname like @criteria or lastname like @criteria or description like @criteria)";
            return connection.Query<PersonModel>(sql, new { criteria = "%" + criteria + "%" }).ToList();
        }

        public PersonModel GetPersonById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = CreateConnection();
            return connection.QueryFirst<PersonModel>("select * from persons where id = @ID", parameters);
        }

        public void InsertPerson(string firstname, string lastname, string description = null, string dateOfBirth = null, int? profileFileId = null)
        {
            try
            {
                using var connection = CreateConnection();
                var person = new PersonModel() { firstname = firstname, lastname = lastname, description = description, dateofbirth = dateOfBirth, profilefileid = profileFileId };
                var sql = "insert into persons (firstname, lastname, description, dateofbirth, profilefileid) values (@firstname, @lastname, @description, @dateofbirth, @profilefileid)";
                connection.Execute(sql, person);
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void DeletePerson(int id)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from persons where id = @id";
            connection.Execute(sql, new { id = id });
        }

        #endregion

        #region Locations

        public List<LocationModel> GetLocations()
        {
            using var connection = CreateConnection();
            var output = connection.Query<LocationModel>("select * from locations", new DynamicParameters());
            return output.ToList();
        }

        public List<LocationModel> SearchLocations(string criteria)
        {
            using var connection = CreateConnection();
            var sql = "select * from locations where (name like @criteria or description like @criteria)";
            return connection.Query<LocationModel>(sql, new { criteria = "%" + criteria + "%" }).ToList();
        }

        public LocationModel GetLocationById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = CreateConnection();
            return connection.QueryFirst<LocationModel>("select * from locations where id = @ID", parameters);
        }

        public void DeleteLocation(int id)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from locations where id = @id";
            connection.Execute(sql, new { id = id });
        }

        #endregion

        #region Tags

        public List<TagModel> GetTags()
        {
            using var connection = CreateConnection();
            var output = connection.Query<TagModel>("select * from tags", new DynamicParameters());
            return output.ToList();
        }

        public List<TagModel> SearchTags(string criteria)
        {
            using var connection = CreateConnection();
            var sql = "select * from tags where (name like @criteria)";
            return connection.Query<TagModel>(sql, new { criteria = "%" + criteria + "%" }).ToList();
        }

        public TagModel GetTagById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = CreateConnection();
            return connection.QueryFirst<TagModel>("select * from tags where id = @ID", parameters);
        }

        public void DeleteTag(int id)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from tags where id = @id";
            connection.Execute(sql, new { id = id });
        }

        #endregion

        #region Helpers

        private IDbConnection CreateConnection()
        {
            var connectionString = $"Data Source={config.Database};foreign keys = true";
            return new SQLiteConnection(connectionString);
        }

        private string InternalPathToPath(string internalPath)
        {
            var path = Path.Join(config.FilesRootDirectory, internalPath);
            path = path.Replace('\\', Path.DirectorySeparatorChar);
            path = path.Replace('/', Path.DirectorySeparatorChar);
            return path;
        }

        private string PathToInternalPath(string path)
        {
            if (path.StartsWith(config.FilesRootDirectory))
            {
                path = path.Substring(config.FilesRootDirectory.Length);
            }
            path = path.Replace('\\', '/');
            while (path.StartsWith('/'))
            {
                path = path.Substring(1);
            }
            return path;
        }

        private string[] PathsToInternalPaths(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = PathToInternalPath(paths[i]);
            }
            return paths;
        }

        #endregion

        // TODO: InsertFile, InsertLocation, InsertTag, ...

        // TODO: delete
        // TODO: update
        // TODO: import files
    }
}
