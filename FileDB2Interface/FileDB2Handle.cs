using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using FileDB2Interface.Model;
using System.IO;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Directory = System.IO.Directory;
using log4net;
using System.Reflection;
using log4net.Config;

namespace FileDB2Interface
{
    public class FileDB2Handle
    {
        public FileDB2Config Config { get; }

        private static readonly ILog log = LogManager.GetLogger("FileDB2Handle");

        public FileDB2Handle(FileDB2Config config)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            Config = config;

            if (!File.Exists(config.Database))
            {
                throw new FileDB2Exception($"Database does not exist: {config.Database}");
            }

            if (!Directory.Exists(config.FilesRootDirectory))
            {
                throw new FileDB2Exception($"Root directory does not exist: {config.FilesRootDirectory}");
            }

            log.Info($"FileDB started with database {config.Database} and root directory {config.FilesRootDirectory}");
        }

        #region Files collection

        public List<string> ListAllFilesystemFiles()
        {
            var files = Directory.GetFiles(Config.FilesRootDirectory, "*.*", SearchOption.AllDirectories);
            return PathsToInternalPaths(files);
        }

        public List<string> ListAllFilesystemDirectories()
        {
            var dirs = Directory.GetDirectories(Config.FilesRootDirectory, "*.*", SearchOption.AllDirectories);
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
            return connection.Query<FilesModel>("select * from [files]", new DynamicParameters()).ToList();
        }

        public int GetFileCount()
        {
            using var connection = CreateConnection();
            return connection.ExecuteScalar<int>("select count(*) from [files]");
        }

        public List<FilesModel> SearchFiles(string criteria)
        {
            using var connection = CreateConnection();
            var sql = "select * from [files] where (path like @criteria or description like @criteria)";
            return connection.Query<FilesModel>(sql, new { criteria = "%" + criteria + "%" }).ToList();
        }

        public List<FilesModel> SearchFilesRandom(int numFiles)
        {
            using var connection = CreateConnection();
            var sql = $"select * from [files] order by random() limit {numFiles}";
            return connection.Query<FilesModel>(sql, new DynamicParameters()).ToList();
        }

        public FilesModel GetFileById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = CreateConnection();
            return connection.QueryFirst<FilesModel>("select * from [files] where id = @ID", parameters);
        }

        public void InsertFile(string internalPath, string description = null)
        {
            internalPath = FixInternalPath(internalPath);
            var path = InternalPathToPath(internalPath);
            if (!File.Exists(path))
            {
                throw new FileDB2Exception($"No such file: {path}");
            }
            // TODO: throw exception if path is blacklisted
            // TODO: throw exception if path is not whitelisted

            ParseFilesystemFileExif(path, out var dateTaken, out var location);
            // TODO: serialize dateTaken
            // TODO: try to parse date from path if dateTaken not available?
            string datetime = null;
            string position = location != null ? $"{location.Latitude} {location.Longitude}" : null;

            try
            {
                using var connection = CreateConnection();
                var files = new FilesModel() { path = internalPath, description = description, datetime = datetime, position = position };
                var sql = "insert into [files] (path, description, datetime, position) values (@path, @description, @datetime, @position)";
                connection.Execute(sql, files);
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void UpdateFileDescription(int id, string description)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = "update [files] set description = @description where id = @id";
                connection.Execute(sql, new { description = description, id = id });
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        // TODO: UpdateFileDatetime(...)

        public void DeleteFile(int id)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from [files] where id = @id";
            connection.Execute(sql, new { id = id });
        }

        public void InsertFilePerson(int fileId, int personId)
        {
            using var connection = CreateConnection();
            var sql = "insert into [filepersons] (fileid, personid) values (@fileId, @locationId)";
            connection.Execute(sql, new { fileId = fileId, personId = personId });
        }

        public void DeleteFilePerson(int fileId, int personId)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from [filepersons] where fileid = @fileId and personid = @personId";
            connection.Execute(sql, new { fileId = fileId, personId = personId });
        }

        public void InsertFileLocation(int fileId, int locationId)
        {
            using var connection = CreateConnection();
            var sql = "insert into [filelocations] (fileid, locationid) values (@fileId, @locationId)";
            connection.Execute(sql, new { fileId = fileId, locationId = locationId });
        }

        public void DeleteFileLocation(int fileId, int locationId)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from [filelocations] where fileid = @fileId and locationid = @locationId";
            connection.Execute(sql, new { fileId = fileId, locationId = locationId });
        }

        public void InsertFileTag(int fileId, int tagId)
        {
            using var connection = CreateConnection();
            var sql = "insert into [filetags] (fileid, tagid) values (@fileId, @tagId)";
            connection.Execute(sql, new { fileId = fileId, tagId = tagId });
        }

        public void DeleteFileTag(int fileId, int tagId)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from [filetags] where fileid = @fileId and tagid = @tagId";
            connection.Execute(sql, new { fileId = fileId, tagId = tagId });
        }

        #endregion

        #region Persons

        public List<PersonModel> GetPersons()
        {
            using var connection = CreateConnection();
            return connection.Query<PersonModel>("select * from [persons]", new DynamicParameters()).ToList();
        }

        public List<PersonModel> GetPersonsFromFile(int fileId)
        {
            using var connection = CreateConnection();
            return connection.Query<PersonModel>("select * from [persons] where id in (select personid from [filepersons] where fileid = @fileid)", new { fileid = fileId }).ToList();
        }

        public int GetPersonCount()
        {
            using var connection = CreateConnection();
            return connection.ExecuteScalar<int>("select count(*) from [persons]");
        }

        public List<PersonModel> SearchPersons(string criteria)
        {
            using var connection = CreateConnection();
            var sql = "select * from [persons] where (firstname like @criteria or lastname like @criteria or description like @criteria)";
            return connection.Query<PersonModel>(sql, new { criteria = "%" + criteria + "%" }).ToList();
        }

        public PersonModel GetPersonById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = CreateConnection();
            return connection.QueryFirst<PersonModel>("select * from [persons] where id = @ID", parameters);
        }

        public void InsertPerson(string firstname, string lastname, string description = null, string dateOfBirth = null, int? profileFileId = null)
        {
            // TODO: validate dateOfBirth
            // TODO: validate profileFileId (it shall exist if not null)

            try
            {
                using var connection = CreateConnection();
                var person = new PersonModel() { firstname = firstname, lastname = lastname, description = description, dateofbirth = dateOfBirth, profilefileid = profileFileId };
                var sql = "insert into [persons] (firstname, lastname, description, dateofbirth, profilefileid) values (@firstname, @lastname, @description, @dateofbirth, @profilefileid)";
                connection.Execute(sql, person);
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void UpdatePersonFirstname(int id, string firstname)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = "update [persons] set firstname = @firstname where id = @id";
                connection.Execute(sql, new { firstname = firstname, id = id });
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void UpdatePersonLastname(int id, string lastname)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = "update [persons] set lastname = @lastname where id = @id";
                connection.Execute(sql, new { lastname = lastname, id = id });
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void UpdatePersonDescription(int id, string description)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = "update [persons] set description = @description where id = @id";
                connection.Execute(sql, new { description = description, id = id });
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void UpdatePersonDateOfBirth(int id, DateTime dateOfBirth)
        {
            try
            {
                var dateOfBirthStr = dateOfBirth.ToString("yyyy-MM-dd");
                using var connection = CreateConnection();
                var sql = "update [persons] set dateofbirth = @dateOfBirthStr where id = @id";
                connection.Execute(sql, new { dateOfBirthStr = dateOfBirthStr, id = id });
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void UpdatePersonProfileFileId(int id, int? profileFileId)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = "update [persons] set profilefileid = @profileFileId where id = @id";
                connection.Execute(sql, new { profileFileId = profileFileId, id = id });
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void DeletePerson(int id)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from [persons] where id = @id";
            connection.Execute(sql, new { id = id });
        }

        #endregion

        #region Locations

        public List<LocationModel> GetLocations()
        {
            using var connection = CreateConnection();
            var output = connection.Query<LocationModel>("select * from [locations]", new DynamicParameters());
            return output.ToList();
        }

        public List<LocationModel> GetLocationsFromFile(int fileId)
        {
            using var connection = CreateConnection();
            return connection.Query<LocationModel>("select * from [locations] where id in (select locationid from [filelocations] where fileid = @fileid)", new { fileid = fileId }).ToList();
        }

        public int GetLocationCount()
        {
            using var connection = CreateConnection();
            return connection.ExecuteScalar<int>("select count(*) from [locations]");
        }

        public List<LocationModel> SearchLocations(string criteria)
        {
            using var connection = CreateConnection();
            var sql = "select * from [locations] where (name like @criteria or description like @criteria)";
            return connection.Query<LocationModel>(sql, new { criteria = "%" + criteria + "%" }).ToList();
        }

        public LocationModel GetLocationById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = CreateConnection();
            return connection.QueryFirst<LocationModel>("select * from [locations] where id = @ID", parameters);
        }

        public void InsertLocation(string name, string description = null, GeoLocation geoLocation = null)
        {
            try
            {
                var position = GeoLocationToString(geoLocation);
                using var connection = CreateConnection();
                var location = new LocationModel() { name = name, description = description, position = position };
                var sql = "insert into [locations] (name, description, position) values (@name, @description, @position)";
                connection.Execute(sql, location);
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void UpdateLocationName(int id, string name)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = "update [locations] set name = @name where id = @id";
                connection.Execute(sql, new { name = name, id = id });
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void UpdateLocationDescription(int id, string description)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = "update [locations] set description = @name where id = @id";
                connection.Execute(sql, new { description = description, id = id });
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void UpdateLocationPosition(int id, GeoLocation geoLocation = null)
        {
            try
            {
                var position = GeoLocationToString(geoLocation);
                using var connection = CreateConnection();
                var sql = "update [locations] set position = @position where id = @id";
                connection.Execute(sql, new { position = position, id = id });
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void DeleteLocation(int id)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from [locations] where id = @id";
            connection.Execute(sql, new { id = id });
        }

        #endregion

        #region Tags

        public List<TagModel> GetTags()
        {
            using var connection = CreateConnection();
            var output = connection.Query<TagModel>("select * from [tags]", new DynamicParameters());
            return output.ToList();
        }

        public List<TagModel> GetTagsFromFile(int fileId)
        {
            using var connection = CreateConnection();
            return connection.Query<TagModel>("select * from [tags] where id in (select tagid from [filetags] where fileid = @fileid)", new { fileid = fileId }).ToList();
        }

        public int GetTagCount()
        {
            using var connection = CreateConnection();
            return connection.ExecuteScalar<int>("select count(*) from [tags]");
        }

        public List<TagModel> SearchTags(string criteria)
        {
            using var connection = CreateConnection();
            var sql = "select * from [tags] where (name like @criteria)";
            return connection.Query<TagModel>(sql, new { criteria = "%" + criteria + "%" }).ToList();
        }

        public TagModel GetTagById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = CreateConnection();
            return connection.QueryFirst<TagModel>("select * from [tags] where id = @ID", parameters);
        }

        public void InsertTag(string name)
        {
            try
            {
                using var connection = CreateConnection();
                var tag = new TagModel() { name = name };
                var sql = "insert into [tags] (name) values (@name)";
                connection.Execute(sql, tag);
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void UpdateTagName(int id, string name)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = "update [tags] set name = @name where id = @id";
                connection.Execute(sql, new { name = name, id = id });
            }
            catch (SQLiteException e)
            {
                throw new FileDB2Exception("SQL error", e);
            }
        }

        public void DeleteTag(int id)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from [tags] where id = @id";
            connection.Execute(sql, new { id = id });
        }

        #endregion

        #region Helpers

        private IDbConnection CreateConnection()
        {
            var connectionString = $"Data Source={Config.Database};foreign keys = true";
            return new SQLiteConnection(connectionString);
        }

        public string InternalPathToPath(string internalPath)
        {
            var path = Path.Join(Config.FilesRootDirectory, internalPath);
            return FixPath(path);
        }

        private string PathToInternalPath(string path)
        {
            if (path.StartsWith(Config.FilesRootDirectory))
            {
                path = path.Substring(Config.FilesRootDirectory.Length);
            }
            return FixInternalPath(path);
        }

        private string FixInternalPath(string path)
        {
            path = path.Replace('\\', '/');
            while (path.StartsWith('/'))
            {
                path = path.Substring(1);
            }
            return path;
        }

        private string FixPath(string path)
        {
            path = path.Replace('\\', Path.DirectorySeparatorChar);
            path = path.Replace('/', Path.DirectorySeparatorChar);
            return path;
        }

        private List<string> PathsToInternalPaths(string[] paths)
        {
            return paths.Select(p => PathToInternalPath(p)).ToList();
        }

        private string GeoLocationToString(GeoLocation geoLocation)
        {
            return geoLocation != null ? $"{geoLocation.Latitude} {geoLocation.Longitude}".Replace(',', '.') : null;
        }

        #endregion
    }
}
