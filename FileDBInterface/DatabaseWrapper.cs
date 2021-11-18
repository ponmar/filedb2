using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using FileDBInterface.Model;
using System.IO;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Directory = System.IO.Directory;
using log4net;
using System.Reflection;
using log4net.Config;
using System.Globalization;
using FileDBInterface.Exceptions;

namespace FileDBInterface
{
    public class DatabaseWrapper : IDatabaseWrapper
    {
        public DatabaseWrapperConfig Config { get; }

        private static readonly ILog log = LogManager.GetLogger("FileDBHandle");

        public DatabaseWrapper(DatabaseWrapperConfig config, bool allowMissingFilesRootDirectory = true)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            Config = config;

            if (!File.Exists(config.Database))
            {
                throw new DatabaseWrapperException($"Database does not exist: {config.Database}");
            }

            if (!Directory.Exists(config.FilesRootDirectory))
            {
                var message = $"Files root directory does not exist: {config.FilesRootDirectory}";
                if (allowMissingFilesRootDirectory)
                {
                    log.Warn(message);
                }
                else
                {
                    throw new DatabaseWrapperException(message);
                }
            }

            log.Info($"FileDB started with database {config.Database} and root directory {config.FilesRootDirectory}");
        }

        #region Files collection

        public IEnumerable<string> ListNewFilesystemFiles(List<string> blacklistedFilePathPatterns, List<string> whitelistedFilePathPatterns, bool includeHiddenDirectories)
        {
            foreach (var filename in Directory.GetFiles(Config.FilesRootDirectory, "*.*", SearchOption.AllDirectories))
            {
                var internalPath = PathToInternalPath(filename);
                if (!PathIsBlacklisted(internalPath, blacklistedFilePathPatterns) &&
                    PathIsWhitelisted(internalPath, whitelistedFilePathPatterns) &&
                    PathIsVisible(internalPath, includeHiddenDirectories) &&
                    !HasFilePath(internalPath))
                {
                    yield return internalPath;
                }
            }
        }

        private bool PathIsBlacklisted(string internalPath, List<string> blacklistedFilePathPatterns)
        {
            return blacklistedFilePathPatterns.FirstOrDefault(pattern => internalPath.IndexOf(pattern) != -1) != null;
        }

        private bool PathIsWhitelisted(string internalPath, List<string> whitelistedFilePathPatterns)
        {
            if (whitelistedFilePathPatterns.Count == 0)
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
            var dirs = Directory.GetDirectories(Config.FilesRootDirectory, "*.*", SearchOption.AllDirectories);
            return PathsToInternalPaths(dirs);
        }

        private void ParseFileExif(string path, out DateTime? dateTaken, out GeoLocation location)
        {
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

            dateTaken = null;
            location = null;
        }

        private string ParseDateFromPath(string path)
        {
            // TODO: use regexp
            var pathParts = path.Split('/');
            foreach (var pathPart in pathParts)
            {
                var words = pathPart.Split(" ");
                if (words.Length > 0)
                {
                    var firstWord = words[0];
                    if (DateTime.TryParseExact(firstWord, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var _))
                    {
                        return firstWord;
                    }
                }
            }
            return null;
        }

        #endregion

        #region Tools

        public IEnumerable<FilesModel> GetFilesMissingInFilesystem()
        {
            foreach (var file in GetFiles())
            {
                if (!File.Exists(InternalPathToPath(file.path)))
                {
                    yield return file;
                }
            }
        }

        #endregion

        #region Files

        public IEnumerable<FilesModel> GetFiles()
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.Query<FilesModel>("select * from [files]", new DynamicParameters());
        }

        public int GetFileCount()
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.ExecuteScalar<int>("select count(*) from [files]");
        }

        public IEnumerable<FilesModel> SearchFiles(string criteria)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "select * from [files] where (path like @criteria or description like @criteria)";
            return connection.Query<FilesModel>(sql, new { criteria = "%" + criteria + "%" });
        }

        public IEnumerable<FilesModel> SearchFilesBySex(Sex sex)
        {
            var personIds = SearchPersonsBySex(sex).Select(p => p.id);
            return GetFilesWithPersons(personIds);
        }

        public IEnumerable<FilesModel> SearchFilesByPath(string criteria)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "select * from [files] where path like @criteria";
            return connection.Query<FilesModel>(sql, new { criteria = criteria + "%" });
        }

        public IEnumerable<FilesModel> SearchFilesRandom(int numFiles)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = $"select * from [files] order by random() limit {numFiles}";
            return connection.Query<FilesModel>(sql, new DynamicParameters());
        }

        public IEnumerable<FilesModel> SearchFilesNearGpsPosition(double latitude, double longitude, double radius)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "select * from [files] where position is not null";

            foreach (var fileWithPosition in connection.Query<FilesModel>(sql))
            {
                var gpsPos = DatabaseUtils.ParseGpsPosition(fileWithPosition.position);
                if (gpsPos != null)
                {
                    if (CalculateDistance(latitude, longitude, gpsPos.Value.lat, gpsPos.Value.lon) < radius)
                    {
                        yield return fileWithPosition;
                    }
                }
            }
        }

        private double CalculateDistance(double point1Lat, double point1Lon, double point2Lat, double point2Long)
        {
            var d1 = point1Lat * (Math.PI / 180.0);
            var num1 = point1Lon * (Math.PI / 180.0);
            var d2 = point2Lat * (Math.PI / 180.0);
            var num2 = point2Long * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }

        public FilesModel GetFileById(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.QueryFirst<FilesModel>("select * from [files] where id = @id", new { id = id });
        }

        public bool HasFileId(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.ExecuteScalar<bool>("select count(1) from [files] where id=@id", new { id });
        }

        public FilesModel GetFileByPath(string path)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.QueryFirst<FilesModel>("select * from [files] where path = @path", new { path = path });
        }

        public IEnumerable<FilesModel> GetFileByDate(DateTime start, DateTime end)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            foreach (var fileWithDate in connection.Query<FilesModel>($"select * from [files] where datetime is not null"))
            {
                if (DateTime.TryParse(fileWithDate.datetime, out var fileDatetime) &&
                    fileDatetime >= start && fileDatetime <= end)
                {
                    yield return fileWithDate;
                }
            }
        }

        public IEnumerable<FilesModel> GetFilesWithPersons(IEnumerable<int> personIds)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.Query<FilesModel>($"select * from [files] inner join filepersons on files.id = filepersons.fileid where filepersons.personid in ({string.Join(',', personIds)})");
        }

        public IEnumerable<FilesModel> GetFilesWithLocations(IEnumerable<int> locationIds)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.Query<FilesModel>($"select * from [files] inner join filelocations on files.id = filelocations.fileid where filelocations.locationid in ({string.Join(',', locationIds)})");
        }

        public IEnumerable<FilesModel> GetFilesWithTags(IEnumerable<int> tagIds)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.Query<FilesModel>($"select * from [files] inner join filetags on files.id = filetags.fileid where filetags.tagid in ({string.Join(',', tagIds)})");
        }

        public IEnumerable<FilesModel> GetFilesWithMissingData()
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var files = connection.Query<FilesModel>($"select * from [files] where description is null");
            foreach (var file in files)
            {
                if (!FileHasPersons(file.id) &&
                    !FileHasLocation(file.id) &&
                    !FileHasTags(file.id))
                {
                    yield return file;
                }
            }
        }

        public bool HasFilePath(string path)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.ExecuteScalar<bool>("select count(1) from [files] where path=@path", new { path = path });
        }

        public void InsertFile(string internalPath, string description = null)
        {
            FormatValidator.ValidateFileDescription(description);

            internalPath = FixInternalPath(internalPath);
            var path = InternalPathToPath(internalPath);
            if (!File.Exists(path))
            {
                throw new DataValidationException($"No such file: {path}");
            }

            GetFileMetaData(path, out var datetime, out var position);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var files = new FilesModel() { path = internalPath, description = description, datetime = datetime, position = position };
                var sql = "insert into [files] (path, description, datetime, position) values (@path, @description, @datetime, @position)";
                connection.Execute(sql, files);
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        private void GetFileMetaData(string path, out string datetime, out string position)
        {
            ParseFileExif(path, out var dateTaken, out var location);

            if (dateTaken != null)
            {
                datetime = dateTaken.Value.ToString("yyyy-MM-ddTHH:mm:ss");
            }
            else
            {
                datetime = ParseDateFromPath(path);
                // TODO: otherwise try to get year from path? The datebase supports yyyy format also
            }

            position = location != null ? $"{location.Latitude} {location.Longitude}" : null;
        }

        public void UpdateFileFromMetaData(int id)
        {
            var file = GetFileById(id);
            GetFileMetaData(InternalPathToPath(file.path), out var datetime, out var position);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var sql = "update [files] set datetime = @datetime, position = @position where id = @id";
                connection.Execute(sql, new { datetime = datetime, position = position, id = id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdateFileDescription(int id, string description)
        {
            FormatValidator.ValidateFileDescription(description);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var sql = "update [files] set description = @description where id = @id";
                connection.Execute(sql, new { description = description, id = id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void DeleteFile(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "delete from [files] where id = @id";
            connection.Execute(sql, new { id = id });
        }

        public void InsertFilePerson(int fileId, int personId)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "insert into [filepersons] (fileid, personid) values (@fileId, @personId)";
            connection.Execute(sql, new { fileId = fileId, personId = personId });
        }

        public void DeleteFilePerson(int fileId, int personId)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "delete from [filepersons] where fileid = @fileId and personid = @personId";
            connection.Execute(sql, new { fileId = fileId, personId = personId });
        }

        public void InsertFileLocation(int fileId, int locationId)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "insert into [filelocations] (fileid, locationid) values (@fileId, @locationId)";
            connection.Execute(sql, new { fileId = fileId, locationId = locationId });
        }

        public void DeleteFileLocation(int fileId, int locationId)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "delete from [filelocations] where fileid = @fileId and locationid = @locationId";
            connection.Execute(sql, new { fileId = fileId, locationId = locationId });
        }

        public void InsertFileTag(int fileId, int tagId)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "insert into [filetags] (fileid, tagid) values (@fileId, @tagId)";
            connection.Execute(sql, new { fileId = fileId, tagId = tagId });
        }

        public void DeleteFileTag(int fileId, int tagId)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "delete from [filetags] where fileid = @fileId and tagid = @tagId";
            connection.Execute(sql, new { fileId = fileId, tagId = tagId });
        }

        #endregion

        #region Persons

        public IEnumerable<PersonModel> GetPersons()
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.Query<PersonModel>("select * from [persons]", new DynamicParameters());
        }

        public bool FileHasPersons(int fileId)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.ExecuteScalar<bool>("select count(1) from [filepersons] where fileid=@fileId", new { fileId });
        }

        public IEnumerable<PersonModel> GetPersonsFromFile(int fileId)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.Query<PersonModel>("select * from [persons] where id in (select personid from [filepersons] where fileid = @fileid)", new { fileid = fileId });
        }

        public int GetPersonCount()
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.ExecuteScalar<int>("select count(*) from [persons]");
        }

        public IEnumerable<PersonModel> SearchPersons(string criteria)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "select * from [persons] where (firstname like @criteria or lastname like @criteria or description like @criteria)";
            return connection.Query<PersonModel>(sql, new { criteria = "%" + criteria + "%" });
        }

        public IEnumerable<PersonModel> SearchPersonsBySex(Sex sex)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "select * from [persons] where sex = @sex";
            return connection.Query<PersonModel>(sql, new { sex = sex });
        }

        public PersonModel GetPersonById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.QueryFirst<PersonModel>("select * from [persons] where id = @ID", parameters);
        }

        public bool HasPersonId(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.ExecuteScalar<bool>("select count(1) from [persons] where id=@id", new { id });
        }

        public void InsertPerson(string firstname, string lastname, string description = null, string dateOfBirth = null, string deceased = null, int? profileFileId = null, Sex sex = Sex.NotApplicable)
        {
            FormatValidator.ValidatePersonFirstname(firstname);
            FormatValidator.ValidatePersonLastname(lastname);
            FormatValidator.ValidatePersonDescription(description);
            FormatValidator.ValidatePersonDateOfBirth(dateOfBirth);
            FormatValidator.ValidatePersonDeceased(deceased);
            ValidatePersonProfileFileId(profileFileId);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var person = new PersonModel() { firstname = firstname, lastname = lastname, description = description, dateofbirth = dateOfBirth, deceased = deceased, profilefileid = profileFileId, sex = sex };
                var sql = "insert into [persons] (firstname, lastname, description, dateofbirth, deceased, profilefileid, sex) values (@firstname, @lastname, @description, @dateofbirth, @deceased, @profilefileid, @sex)";
                connection.Execute(sql, person);
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdatePerson(int id, string firstname, string lastname, string description = null, string dateOfBirth = null, string deceased = null, int? profileFileId = null, Sex sex = Sex.NotApplicable)
        {
            FormatValidator.ValidatePersonFirstname(firstname);
            FormatValidator.ValidatePersonLastname(lastname);
            FormatValidator.ValidatePersonDescription(description);
            FormatValidator.ValidatePersonDateOfBirth(dateOfBirth);
            FormatValidator.ValidatePersonDeceased(deceased);
            ValidatePersonProfileFileId(profileFileId);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var person = new PersonModel() { id = id, firstname = firstname, lastname = lastname, description = description, dateofbirth = dateOfBirth, deceased = deceased, profilefileid = profileFileId, sex = sex };
                var sql = "update [persons] set firstname = @firstname, lastname = @lastname, description = @description, dateofbirth = @dateofbirth, deceased = @deceased, profilefileid = @profilefileid, sex = @sex where id = @id";
                connection.Execute(sql, person);
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdatePersonFirstname(int id, string firstname)
        {
            FormatValidator.ValidatePersonFirstname(firstname);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var sql = "update [persons] set firstname = @firstname where id = @id";
                connection.Execute(sql, new { firstname = firstname, id = id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdatePersonLastname(int id, string lastname)
        {
            FormatValidator.ValidatePersonLastname(lastname);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var sql = "update [persons] set lastname = @lastname where id = @id";
                connection.Execute(sql, new { lastname = lastname, id = id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdatePersonDescription(int id, string description)
        {
            FormatValidator.ValidatePersonDescription(description);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var sql = "update [persons] set description = @description where id = @id";
                connection.Execute(sql, new { description = description, id = id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdatePersonDateOfBirth(int id, string dateOfBirthStr)
        {
            FormatValidator.ValidatePersonDateOfBirth(dateOfBirthStr);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var sql = "update [persons] set dateofbirth = @dateOfBirthStr where id = @id";
                connection.Execute(sql, new { dateOfBirthStr = dateOfBirthStr, id = id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdatePersonDateOfBirth(int id, DateTime dateOfBirth)
        {
            var dateOfBirthStr = dateOfBirth.ToString("yyyy-MM-dd");
            UpdatePersonDateOfBirth(id, dateOfBirthStr);
        }

        public void UpdatePersonDeceased(int id, string deceasedStr)
        {
            FormatValidator.ValidatePersonDeceased(deceasedStr);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var sql = "update [persons] set deceased = @deceasedStr where id = @id";
                connection.Execute(sql, new { deceased = deceasedStr, id = id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdatePersonDeceased(int id, DateTime deceased)
        {
            var deceasedStr = deceased.ToString("yyyy-MM-dd");
            UpdatePersonDeceased(id, deceasedStr);
        }

        public void UpdatePersonProfileFileId(int id, int? profileFileId)
        {
            ValidatePersonProfileFileId(profileFileId);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var sql = "update [persons] set profilefileid = @profileFileId where id = @id";
                connection.Execute(sql, new { profileFileId = profileFileId, id = id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdatePersonSex(int id, Sex sex)
        {
            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var sql = "update [persons] set sex = @sex where id = @id";
                connection.Execute(sql, new { sex = sex, id = id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void DeletePerson(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "delete from [persons] where id = @id";
            connection.Execute(sql, new { id = id });
        }

        #endregion

        #region Locations

        public IEnumerable<LocationModel> GetLocations()
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.Query<LocationModel>("select * from [locations]", new DynamicParameters());
        }

        public bool FileHasLocation(int fileId)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.ExecuteScalar<bool>("select count(1) from [filelocations] where fileid=@fileId", new { fileId });
        }

        public IEnumerable<LocationModel> GetLocationsFromFile(int fileId)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.Query<LocationModel>("select * from [locations] where id in (select locationid from [filelocations] where fileid = @fileid)", new { fileid = fileId });
        }

        public int GetLocationCount()
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.ExecuteScalar<int>("select count(*) from [locations]");
        }

        public IEnumerable<LocationModel> SearchLocations(string criteria)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "select * from [locations] where (name like @criteria or description like @criteria)";
            return connection.Query<LocationModel>(sql, new { criteria = "%" + criteria + "%" });
        }

        public LocationModel GetLocationById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.QueryFirst<LocationModel>("select * from [locations] where id = @ID", parameters);
        }

        public bool HasLocationId(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.ExecuteScalar<bool>("select count(1) from [locations] where id=@id", new { id });
        }

        public void InsertLocation(string name, string description = null, string geoLocation = null)
        {
            FormatValidator.ValidateLocationName(name);
            FormatValidator.ValidateLocationDescription(description);
            FormatValidator.ValidateLocationGeoLocation(geoLocation, out _);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var location = new LocationModel() { name = name, description = description, position = geoLocation };
                var sql = "insert into [locations] (name, description, position) values (@name, @description, @position)";
                connection.Execute(sql, location);
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void InsertLocation(string name, string description = null, GeoLocation geoLocation = null)
        {
            var geoLocationStr = GeoLocationToString(geoLocation);
            InsertLocation(name, description, geoLocationStr);
        }

        public void UpdateLocationName(int id, string name)
        {
            FormatValidator.ValidateLocationName(name);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var sql = "update [locations] set name = @name where id = @id";
                connection.Execute(sql, new { name = name, id = id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdateLocationDescription(int id, string description)
        {
            FormatValidator.ValidateLocationDescription(description);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var sql = "update [locations] set description = @description where id = @id";
                connection.Execute(sql, new { description = description, id = id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdateLocationPosition(int id, string geoLocation)
        {
            FormatValidator.ValidateLocationGeoLocation(geoLocation, out _);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var sql = "update [locations] set position = @position where id = @id";
                connection.Execute(sql, new { position = geoLocation, id = id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdateLocationPosition(int id, GeoLocation geoLocation)
        {
            var geoLocationStr = geoLocation != null ? GeoLocationToString(geoLocation) : null;
            UpdateLocationPosition(id, geoLocationStr);
        }

        public void DeleteLocation(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "delete from [locations] where id = @id";
            connection.Execute(sql, new { id = id });
        }

        #endregion

        #region Tags

        public IEnumerable<TagModel> GetTags()
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.Query<TagModel>("select * from [tags]", new DynamicParameters());
        }

        public bool FileHasTags(int fileId)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.ExecuteScalar<bool>("select count(1) from [filetags] where fileid=@fileId", new { fileId });
        }

        public IEnumerable<TagModel> GetTagsFromFile(int fileId)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.Query<TagModel>("select * from [tags] where id in (select tagid from [filetags] where fileid = @fileid)", new { fileid = fileId });
        }

        public int GetTagCount()
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.ExecuteScalar<int>("select count(*) from [tags]");
        }

        public IEnumerable<TagModel> SearchTags(string criteria)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "select * from [tags] where (name like @criteria)";
            return connection.Query<TagModel>(sql, new { criteria = "%" + criteria + "%" });
        }

        public TagModel GetTagById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.QueryFirst<TagModel>("select * from [tags] where id = @ID", parameters);
        }

        public bool HasTagId(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            return connection.ExecuteScalar<bool>("select count(1) from [tags] where id=@id", new { id });
        }

        public void InsertTag(string name)
        {
            FormatValidator.ValidateTagName(name);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var tag = new TagModel() { name = name };
                var sql = "insert into [tags] (name) values (@name)";
                connection.Execute(sql, tag);
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdateTagName(int id, string name)
        {
            FormatValidator.ValidateTagName(name);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(Config.Database);
                var sql = "update [tags] set name = @name where id = @id";
                connection.Execute(sql, new { name = name, id = id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void DeleteTag(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(Config.Database);
            var sql = "delete from [tags] where id = @id";
            connection.Execute(sql, new { id = id });
        }

        #endregion

        #region Helpers

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
            path = path.Replace('\\', '/');
            //path = path.Replace('/', Path.DirectorySeparatorChar);
            return path;
        }

        private IEnumerable<string> PathsToInternalPaths(string[] paths)
        {
            return paths.Select(p => PathToInternalPath(p));
        }

        private string GeoLocationToString(GeoLocation geoLocation)
        {
            return geoLocation != null ? $"{geoLocation.Latitude} {geoLocation.Longitude}".Replace(',', '.') : null;
        }

        #endregion

        public void ValidatePersonProfileFileId(int? profileFileId)
        {
            if (profileFileId != null && !HasFileId(profileFileId.Value))
            {
                throw new DataValidationException($"File with id {profileFileId.Value} does not exist");
            }
        }
    }
}
