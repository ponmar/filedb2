using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using FileDBInterface.Model;
using System.IO;
using log4net;
using System.Reflection;
using log4net.Config;
using FileDBInterface.Exceptions;
using FileDBInterface.Validators;
using FileDBInterface.FilesystemAccess;

namespace FileDBInterface.DbAccess.SQLite
{
    public class SqLiteDbAccess : IDbAccess
    {
        private static readonly ILog log = LogManager.GetLogger(nameof(SqLiteDbAccess));

        public string Database
        {
            get => database;
            set
            {
                if (database != value)
                {
                    database = value;
                    if (!File.Exists(database))
                    {
                        throw new DatabaseWrapperException($"Database does not exist: {database}");
                    }
                }
            }
        }
        private string database;

        public SqLiteDbAccess(string database)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            Database = database;
        }

        #region Files

        public IEnumerable<FilesModel> GetFiles()
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.Query<FilesModel>("select * from [files]");
        }

        public int GetFileCount()
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.ExecuteScalar<int>("select count(*) from [files]");
        }

        public IEnumerable<FilesModel> SearchFilesFromIds(IEnumerable<int> fileIds)
        {
            // TODO: dapper can not handle too many fileIds? Need to split into several queries?
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "select * from [files] where Id in @ids";
            return connection.Query<FilesModel>(sql, new { ids = fileIds });
        }

        public IEnumerable<FilesModel> SearchFiles(string criteria)
        {
            // Note: 'like' is case insensitive
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "select * from [files] where (Path like @criteria or Description like @criteria)";
            return connection.Query<FilesModel>(sql, new { criteria = $"%{criteria}%" });
        }

        public IEnumerable<FilesModel> SearchFilesBySex(Sex sex)
        {
            var personIds = SearchPersonsBySex(sex).Select(p => p.Id);
            return SearchFilesWithPersons(personIds);
        }

        public IEnumerable<FilesModel> SearchFilesByPath(string criteria)
        {
            // Note: 'like' is case insensitive
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "select * from [files] where Path like @criteria";
            return connection.Query<FilesModel>(sql, new { criteria = criteria + "%" });
        }

        public IEnumerable<FilesModel> SearchFilesByExtension(string extension)
        {
            // Note: 'like' is case insensitive
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "select * from [files] where Path like @criteria";
            return connection.Query<FilesModel>(sql, new { criteria = "%" + extension });
        }

        public IEnumerable<FilesModel> SearchFilesRandom(int numFiles)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = $"select * from [files] order by random() limit {numFiles}";
            return connection.Query<FilesModel>(sql);
        }

        public IEnumerable<LocationModel> SearchLocationsNearGpsPosition(double latitude, double longitude, double radius)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "select * from [locations] where Position is not null";

            foreach (var locationWithPosition in connection.Query<LocationModel>(sql))
            {
                var gpsPos = DatabaseParsing.ParseFilesPosition(locationWithPosition.Position);
                if (gpsPos != null)
                {
                    if (DatabaseUtils.CalculateDistance(latitude, longitude, gpsPos.Value.lat, gpsPos.Value.lon) < radius)
                    {
                        yield return locationWithPosition;
                    }
                }
            }
        }

        public IEnumerable<FilesModel> SearchFilesNearGpsPosition(double latitude, double longitude, double radius)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "select * from [files] where Position is not null";

            foreach (var fileWithPosition in connection.Query<FilesModel>(sql))
            {
                var gpsPos = DatabaseParsing.ParseFilesPosition(fileWithPosition.Position);
                if (gpsPos != null)
                {
                    if (DatabaseUtils.CalculateDistance(latitude, longitude, gpsPos.Value.lat, gpsPos.Value.lon) < radius)
                    {
                        yield return fileWithPosition;
                    }
                }
            }
        }

        public FilesModel GetFileById(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.QueryFirst<FilesModel>("select * from [files] where Id = @id", new { id });
        }

        public FilesModel? GetFileByPath(string path)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.QueryFirstOrDefault<FilesModel>("select * from [files] where Path = @path", new { path });
        }

        public IEnumerable<FilesModel> SearchFilesByDate(DateTime start, DateTime end)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            foreach (var fileWithDate in connection.Query<FilesModel>($"select * from [files] where Datetime is not null"))
            {
                if (DateTime.TryParse(fileWithDate.Datetime, out var fileDatetime) &&
                    fileDatetime >= start && fileDatetime <= end)
                {
                    yield return fileWithDate;
                }
            }
        }

        public IEnumerable<FilesModel> SearchFilesWithPersons(IEnumerable<int> personIds)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.Query<FilesModel>($"select * from [files] inner join filepersons on files.Id = filepersons.FileId where filepersons.PersonId in ({string.Join(',', personIds)})");
        }

        public IEnumerable<FilesModel> SearchFilesWithLocations(IEnumerable<int> locationIds)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.Query<FilesModel>($"select * from [files] inner join filelocations on files.Id = filelocations.FileId where filelocations.LocationId in ({string.Join(',', locationIds)})");
        }

        public IEnumerable<FilesModel> SearchFilesWithTags(IEnumerable<int> tagIds)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.Query<FilesModel>($"select * from [files] inner join filetags on files.Id = filetags.FileId where filetags.TagId in ({string.Join(',', tagIds)})");
        }

        public IEnumerable<FilesModel> SearchFilesWithMissingData()
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var files = connection.Query<FilesModel>($"select * from [files] where Description is null");
            foreach (var file in files)
            {
                if (!FileHasPersons(file.Id) &&
                    !FileHasLocation(file.Id) &&
                    !FileHasTags(file.Id))
                {
                    yield return file;
                }
            }
        }

        public void InsertFile(string internalPath, string? description, IFilesystemAccess filesystemAccess, bool findMetadata)
        {
            if (!FilesModelValidator.ValidateDescription(description))
            {
                throw new DataValidationException("Description invalid");
            }

            var absolutePath = filesystemAccess.ToAbsolutePath(internalPath);
            if (!File.Exists(absolutePath))
            {
                throw new DataValidationException($"No such file: {absolutePath}");
            }

            var fileMetadata = findMetadata ? filesystemAccess.ParseFileMetadata(absolutePath) : new(null, null, null);

            try
            {
                using var connection = DatabaseUtils.CreateConnection(database);
                var files = new FilesModel() { Path = internalPath, Description = description, Datetime = fileMetadata.Datetime, Position = fileMetadata.Position, Orientation = fileMetadata.Orientation };
                var sql = "insert into [files] (Path, Description, Datetime, Position, Orientation) values (@Path, @Description, @Datetime, @Position, @Orientation)";
                connection.Execute(sql, files);
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdateFileFromMetaData(int id, IFilesystemAccess filesystemAccess)
        {
            var file = GetFileById(id);
            var fileMetadata = filesystemAccess.ParseFileMetadata(filesystemAccess.ToAbsolutePath(file.Path));

            try
            {
                using var connection = DatabaseUtils.CreateConnection(database);
                var sql = "update [files] set Datetime = @datetime, Position = @position, Orientation = @orientation where Id = @id";
                connection.Execute(sql, new { datetime = fileMetadata.Datetime, position = fileMetadata.Position, orientation = fileMetadata.Orientation, id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdateFileDescription(int id, string? description)
        {
            if (!FilesModelValidator.ValidateDescription(description))
            {
                throw new DataValidationException("Invalid description");
            }

            try
            {
                using var connection = DatabaseUtils.CreateConnection(database);
                var sql = "update [files] set Description = @description where Id = @id";
                connection.Execute(sql, new { description, id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdateFileDatetime(int id, string? datetime)
        {
            if (!FilesModelValidator.ValidateDatetime(datetime))
            {
                throw new DataValidationException("Invalid datetime");
            }

            try
            {
                using var connection = DatabaseUtils.CreateConnection(database);
                var sql = "update [files] set Datetime = @datetime where Id = @id";
                connection.Execute(sql, new { datetime, id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdateFileOrientation(int id, int? orientation)
        {
            if (!FilesModelValidator.ValidateOrientation(orientation))
            {
                throw new DataValidationException("Invalid orientation");
            }

            try
            {
                using var connection = DatabaseUtils.CreateConnection(database);
                var sql = "update [files] set Orientation = @orientation where Id = @id";
                connection.Execute(sql, new { orientation, id });
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void DeleteFile(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "delete from [files] where Id = @id";
            connection.Execute(sql, new { id });
        }

        public void InsertFilePerson(int fileId, int personId)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "insert into [filepersons] (FileId, PersonId) values (@fileId, @personId)";
            connection.Execute(sql, new { fileId, personId });
        }

        public void DeleteFilePerson(int fileId, int personId)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "delete from [filepersons] where FileId = @fileId and PersonId = @personId";
            connection.Execute(sql, new { fileId, personId });
        }

        public void InsertFileLocation(int fileId, int locationId)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "insert into [filelocations] (Fileid, LocationId) values (@fileId, @locationId)";
            connection.Execute(sql, new { fileId, locationId });
        }

        public void DeleteFileLocation(int fileId, int locationId)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "delete from [filelocations] where FileId = @fileId and LocationId = @locationId";
            connection.Execute(sql, new { fileId, locationId });
        }

        public void InsertFileTag(int fileId, int tagId)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "insert into [filetags] (FileId, TagId) values (@fileId, @tagId)";
            connection.Execute(sql, new { fileId, tagId });
        }

        public void DeleteFileTag(int fileId, int tagId)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "delete from [filetags] where FileId = @fileId and TagId = @tagId";
            connection.Execute(sql, new { fileId, tagId });
        }

        #endregion

        #region Persons

        public IEnumerable<PersonModel> GetPersons()
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.Query<PersonModel>("select * from [persons]");
        }

        private bool FileHasPersons(int fileId)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.ExecuteScalar<bool>("select count(1) from [filepersons] where FileId=@fileId", new { fileId });
        }

        public IEnumerable<PersonModel> GetPersonsFromFile(int fileId)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.Query<PersonModel>("select * from [persons] where Id in (select PersonId from [filepersons] where FileId = @fileid)", new { fileid = fileId });
        }

        public int GetPersonCount()
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.ExecuteScalar<int>("select count(*) from [persons]");
        }

        public IEnumerable<PersonModel> SearchPersonsBySex(Sex sex)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "select * from [persons] where Sex = @sex";
            return connection.Query<PersonModel>(sql, new { sex });
        }

        public PersonModel GetPersonById(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.QueryFirst<PersonModel>("select * from [persons] where Id = @ID", new { id });
        }

        public bool HasPersonId(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.ExecuteScalar<bool>("select count(1) from [persons] where Id=@id", new { id });
        }

        public void InsertPerson(PersonModel person)
        {
            var validator = new PersonModelValidator();
            var result = validator.Validate(person);
            if (!result.IsValid)
            {
                throw new DataValidationException(result);
            }

            try
            {
                using var connection = DatabaseUtils.CreateConnection(database);
                var sql = "insert into [persons] (Firstname, Lastname, Description, DateOfBirth, Deceased, ProfileFileId, Sex) values (@Firstname, @Lastname, @Description, @DateOfBirth, @Deceased, @ProfileFileId, @Sex)";
                connection.Execute(sql, person);
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdatePerson(PersonModel person)
        {
            var validator = new PersonModelValidator();
            var result = validator.Validate(person);
            if (!result.IsValid)
            {
                throw new DataValidationException(result);
            }

            try
            {
                using var connection = DatabaseUtils.CreateConnection(database);
                var sql = "update [persons] set Firstname = @Firstname, Lastname = @Lastname, Description = @Description, DateOfBirth = @DateOfBirth, Deceased = @Deceased, ProfileFileId = @ProfileFileId, Sex = @Sex where Id = @Id";
                connection.Execute(sql, person);
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void DeletePerson(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "delete from [persons] where Id = @id";
            connection.Execute(sql, new { id });
        }

        #endregion

        #region Locations

        public IEnumerable<LocationModel> GetLocations()
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.Query<LocationModel>("select * from [locations]");
        }

        private bool FileHasLocation(int fileId)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.ExecuteScalar<bool>("select count(1) from [filelocations] where FileId=@fileId", new { fileId });
        }

        public IEnumerable<LocationModel> GetLocationsFromFile(int fileId)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.Query<LocationModel>("select * from [locations] where Id in (select LocationId from [filelocations] where FileId = @fileId)", new { fileid = fileId });
        }

        public int GetLocationCount()
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.ExecuteScalar<int>("select count(*) from [locations]");
        }

        public LocationModel GetLocationById(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.QueryFirst<LocationModel>("select * from [locations] where Id = @id", new { id });
        }

        public bool HasLocationId(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.ExecuteScalar<bool>("select count(1) from [locations] where Id=@id", new { id });
        }

        public void InsertLocation(LocationModel location)
        {
            var validator = new LocationModelValidator();
            var result = validator.Validate(location);
            if (!result.IsValid)
            {
                throw new DataValidationException(result);
            }

            try
            {
                using var connection = DatabaseUtils.CreateConnection(database);
                var sql = "insert into [locations] (Name, Description, Position) values (@Name, @Description, @Position)";
                connection.Execute(sql, location);
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdateLocation(LocationModel location)
        {

            var validator = new LocationModelValidator();
            var result = validator.Validate(location);
            if (!result.IsValid)
            {
                throw new DataValidationException(result);
            }

            try
            {
                using var connection = DatabaseUtils.CreateConnection(database);
                var sql = "update [locations] set Name = @Name, Description = @Description, Position = @Position where Id = @Id";
                connection.Execute(sql, location);
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void DeleteLocation(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "delete from [locations] where Id = @id";
            connection.Execute(sql, new { id });
        }

        #endregion

        #region Tags

        public IEnumerable<TagModel> GetTags()
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.Query<TagModel>("select * from [tags]");
        }

        private bool FileHasTags(int fileId)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.ExecuteScalar<bool>("select count(1) from [filetags] where FileId=@fileId", new { fileId });
        }

        public IEnumerable<TagModel> GetTagsFromFile(int fileId)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.Query<TagModel>("select * from [tags] where Id in (select TagId from [filetags] where FileId = @fileid)", new { fileid = fileId });
        }

        public int GetTagCount()
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.ExecuteScalar<int>("select count(*) from [tags]");
        }

        public TagModel GetTagById(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.QueryFirst<TagModel>("select * from [tags] where Id = @id", new { id });
        }

        public bool HasTagId(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            return connection.ExecuteScalar<bool>("select count(1) from [tags] where Id=@id", new { id });
        }

        public void InsertTag(TagModel tag)
        {
            var validator = new TagModelValidator();
            var result = validator.Validate(tag);
            if (!result.IsValid)
            {
                throw new DataValidationException(result);
            }

            try
            {
                using var connection = DatabaseUtils.CreateConnection(database);
                var sql = "insert into [tags] (Name) values (@Name)";
                connection.Execute(sql, tag);
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void UpdateTag(TagModel tag)
        {
            var validator = new TagModelValidator();
            var result = validator.Validate(tag);
            if (!result.IsValid)
            {
                throw new DataValidationException(result);
            }

            try
            {
                using var connection = DatabaseUtils.CreateConnection(database);
                var sql = "update [tags] set Name = @Name where Id = @Id";
                connection.Execute(sql, tag);
            }
            catch (SQLiteException e)
            {
                throw new DatabaseWrapperException("SQL error", e);
            }
        }

        public void DeleteTag(int id)
        {
            using var connection = DatabaseUtils.CreateConnection(database);
            var sql = "delete from [tags] where Id = @id";
            connection.Execute(sql, new { id });
        }

        #endregion
    }
}
