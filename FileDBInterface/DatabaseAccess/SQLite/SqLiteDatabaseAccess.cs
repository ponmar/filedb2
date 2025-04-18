using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using FileDBInterface.Model;
using System.IO;
using FileDBInterface.Exceptions;
using FileDBInterface.Extensions;
using FileDBInterface.Validators;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Utils;
using Microsoft.Extensions.Logging;

namespace FileDBInterface.DatabaseAccess.SQLite;

public class SqLiteDatabaseAccess : IDatabaseAccess
{
    public bool NeedsMigration => migrator.NeedsMigration;

    private readonly string database;
    private readonly ILogger logger;
    private readonly DatabaseMigrator migrator;

    public SqLiteDatabaseAccess(string database, ILoggerFactory loggerFactory)
    {
        this.database = database;
        this.logger = loggerFactory.CreateLogger<SqLiteDatabaseAccess>();

        migrator = new DatabaseMigrator(database);
    }

    public List<DatabaseMigrationResult> Migrate()
    {
        return migrator.Migrate();
    }

    #region Files

    public IEnumerable<FileModel> GetFiles()
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<FileModel>("select * from [files]");
    }

    public IEnumerable<string> GetDirectories()
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var files = connection.Query<string>("select Path from [files] where instr(Path, '/') > 0");
        return files.Select(x => x.TextBeforeLast("/")).Distinct().Order();
    }

    public int GetFileCount()
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.ExecuteScalar<int>("select count(*) from [files]");
    }

    public IEnumerable<FileModel> SearchFilesFromIds(IEnumerable<int> fileIds)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "select * from [files] where Id in @ids";
        return connection.Query<FileModel>(sql, new { ids = fileIds });
    }

    public IEnumerable<FileModel> SearchFilesExceptIds(IEnumerable<int> fileIds)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "select * from [files] where Id not in @ids";
        return connection.Query<FileModel>(sql, new { ids = fileIds });
    }

    public IEnumerable<FileModel> SearchFiles(string criteria, bool caseSensitive)
    {
        // Note: 'like' is case insensitive
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "select * from [files] where (Path like @criteria or Description like @criteria)";
        var result = connection.Query<FileModel>(sql, new { criteria = $"%{criteria}%" });
        if (caseSensitive)
        {
            return result.Where(x => x.Path.Contains(criteria) || x.Description is not null && x.Description.Contains(criteria));
        }
        return result;
    }

    public IEnumerable<FileModel> SearchFilesBySex(Sex sex)
    {
        var personIds = SearchPersonsBySex(sex).Select(p => p.Id);
        return SearchFilesWithPersons(personIds);
    }

    public IEnumerable<FileModel> SearchFilesByPath(string criteria)
    {
        // Note: 'like' is case insensitive
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "select * from [files] where Path like @criteria";
        return connection.Query<FileModel>(sql, new { criteria = criteria + "%" });
    }

    public IEnumerable<FileModel> SearchFilesByExtension(string extension)
    {
        // Note: 'like' is case insensitive
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "select * from [files] where Path like @criteria";
        return connection.Query<FileModel>(sql, new { criteria = "%" + extension });
    }

    public IEnumerable<FileModel> SearchFilesRandom(int numFiles)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = $"select * from [files] order by random() limit {numFiles}";
        return connection.Query<FileModel>(sql);
    }

    public IEnumerable<LocationModel> SearchLocationsNearGpsPosition(double latitude, double longitude, double radius)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "select * from [locations] where Position is not null";

        foreach (var locationWithPosition in connection.Query<LocationModel>(sql))
        {
            var gpsPos = DatabaseParsing.ParseFilesPosition(locationWithPosition.Position);
            if (gpsPos is not null)
            {
                if (LatLonUtils.CalculateDistance(latitude, longitude, gpsPos.Value.lat, gpsPos.Value.lon) < radius)
                {
                    yield return locationWithPosition;
                }
            }
        }
    }

    public IEnumerable<FileModel> SearchFilesNearGpsPosition(double latitude, double longitude, double radius)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "select * from [files] where Position is not null";

        foreach (var fileWithPosition in connection.Query<FileModel>(sql))
        {
            var gpsPos = DatabaseParsing.ParseFilesPosition(fileWithPosition.Position);
            if (gpsPos is not null)
            {
                if (LatLonUtils.CalculateDistance(latitude, longitude, gpsPos.Value.lat, gpsPos.Value.lon) < radius)
                {
                    yield return fileWithPosition;
                }
            }
        }
    }

    public FileModel GetFileById(int id)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.QueryFirst<FileModel>("select * from [files] where Id = @id", new { id });
    }

    public FileModel? GetFileByPath(string path)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.QueryFirstOrDefault<FileModel>("select * from [files] where Path = @path", new { path });
    }

    public IEnumerable<FileModel> SearchFilesByTime(TimeOnly start, TimeOnly end)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        foreach (var fileWithDate in connection.Query<FileModel>($"select * from [files] where Datetime like '%T%'"))
        {
            var fileTime = DatabaseParsing.ParseFileTime(fileWithDate.Datetime);
            if (fileTime is not null && fileTime >= start && fileTime <= end)
            {
                yield return fileWithDate;
            }
        }
    }

    public IEnumerable<FileModel> SearchFilesByDate(DateTime date)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        foreach (var fileWithDate in connection.Query<FileModel>($"select * from [files] where Datetime is not null"))
        {
            var fileDatetime = DatabaseParsing.ParseFilesDatetime(fileWithDate.Datetime);
            if (fileDatetime is not null &&
                fileDatetime.Value.Date == date.Date)
            {
                yield return fileWithDate;
            }
        }
    }

    public IEnumerable<FileModel> SearchFilesByDate(DateTime start, DateTime end)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        foreach (var fileWithDate in connection.Query<FileModel>($"select * from [files] where Datetime is not null"))
        {
            var fileDatetime = DatabaseParsing.ParseFilesDatetime(fileWithDate.Datetime);
            if (fileDatetime is not null &&
                fileDatetime >= start && fileDatetime <= end)
            {
                yield return fileWithDate;
            }
        }
    }

    public IEnumerable<FileModel> SearchFilesBySeason(Season season)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        foreach (var fileWithDate in connection.Query<FileModel>($"select * from [files] where Datetime is not null"))
        {
            var fileDatetime = DatabaseParsing.ParseFilesDatetime(fileWithDate.Datetime);
            if (fileDatetime?.GetApproximatedSeason() == season)
            {
                yield return fileWithDate;
            }
        }
    }

    public IEnumerable<FileModel> SearchFilesByAnnualDate(int startMonth, int startDay, int endMonth, int endDay)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        foreach (var fileWithDate in connection.Query<FileModel>($"select * from [files] where Datetime is not null"))
        {
            var fileDatetime = DatabaseParsing.ParseFilesDatetime(fileWithDate.Datetime);
            if (fileDatetime is not null && fileDatetime.Value.IsMonthAndDayInRange(startMonth, startDay, endMonth, endDay))
            {
                yield return fileWithDate;
            }
        }
    }

    public IEnumerable<FileModel> SearchFilesByAnnualDate(int month, int day)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        foreach (var fileWithDate in connection.Query<FileModel>($"select * from [files] where Datetime is not null"))
        {
            var fileDatetime = DatabaseParsing.ParseFilesDatetime(fileWithDate.Datetime);
            if (fileDatetime is not null &&
                fileDatetime.Value.Month == month &&
                fileDatetime.Value.Day == day)
            {
                yield return fileWithDate;
            }
        }
    }

    public IEnumerable<FileModel> SearchFilesWithoutDate()
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<FileModel>($"select * from [files] where Datetime is null");
    }

    public IEnumerable<FileModel> SearchFilesByNumPersons(Range numPersonsRange)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "select * from [files] where Id in (select FileId from [filepersons] group by FileId having count(*) >= @start and count(*) <= @end)";
        return connection.Query<FileModel>(sql, new { start = numPersonsRange.Start.Value, end = numPersonsRange.End.Value });
    }

    public IEnumerable<FileModel> SearchFilesWithPersons(IEnumerable<int> personIds)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var files = connection.Query<FileModel>($"select * from [files] inner join filepersons on files.Id = filepersons.FileId where filepersons.PersonId in ({string.Join(',', personIds)})");
        return files.DistinctBy(x => x.Id);
    }

    public IEnumerable<FileModel> SearchFilesWithPersonGroup(IEnumerable<int> personIds)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var files = connection.Query<FileModel>($"select * from [files] inner join filepersons on files.Id = filepersons.FileId where filepersons.PersonId in ({string.Join(',', personIds)})");
        return files.DistinctBy(x => x.Id).Where(x =>
        {
            var filePersons = GetPersonsFromFile(x.Id);
            return filePersons.Select(x => x.Id).Intersect(personIds).Count() == personIds.Count();
        });
    }

    public IEnumerable<FileModel> SearchFilesWithPersonGroupOnly(IEnumerable<int> personIds)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var files = connection.Query<FileModel>($"select * from [files] inner join filepersons on files.Id = filepersons.FileId where filepersons.PersonId in ({string.Join(',', personIds)})");
        return files.DistinctBy(x => x.Id).Where(x =>
        {
            var filePersons = GetPersonsFromFile(x.Id);
            return filePersons.Count() == personIds.Count() &&
                filePersons.Select(x => x.Id).Intersect(personIds).Count() == personIds.Count();
        });
    }

    public IEnumerable<FileModel> SearchFilesWithoutPersons(IEnumerable<int> personIds)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<FileModel>($"select * from [files] inner join filepersons on files.Id = filepersons.FileId where filepersons.PersonId not in ({string.Join(',', personIds)})");
    }

    public IEnumerable<FileModel> SearchFilesWithLocations(IEnumerable<int> locationIds)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<FileModel>($"select * from [files] inner join filelocations on files.Id = filelocations.FileId where filelocations.LocationId in ({string.Join(',', locationIds)})");
    }

    public IEnumerable<FileModel> SearchFilesWithoutLocations(IEnumerable<int> locationIds)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<FileModel>($"select * from [files] inner join filelocations on files.Id = filelocations.FileId where filelocations.LocationId not in ({string.Join(',', locationIds)})");
    }

    public IEnumerable<FileModel> SearchFilesWithTags(IEnumerable<int> tagIds)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<FileModel>($"select * from [files] inner join filetags on files.Id = filetags.FileId where filetags.TagId in ({string.Join(',', tagIds)})");
    }

    public IEnumerable<FileModel> SearchFilesWithTagGroup(IEnumerable<int> tagIds)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var files = connection.Query<FileModel>($"select * from [files] inner join filetags on files.Id = filetags.FileId where filetags.TagId in ({string.Join(',', tagIds)})");
        return files.DistinctBy(x => x.Id).Where(x =>
        {
            var fileTags = GetTagsFromFile(x.Id);
            return fileTags.Select(x => x.Id).Intersect(tagIds).Count() == tagIds.Count();
        });
    }

    public IEnumerable<FileModel> SearchFilesWithTagGroupOnly(IEnumerable<int> tagIds)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var files = connection.Query<FileModel>($"select * from [files] inner join filetags on files.Id = filetags.FileId where filetags.TagId in ({string.Join(',', tagIds)})");
        return files.DistinctBy(x => x.Id).Where(x =>
        {
            var fileTags = GetTagsFromFile(x.Id);
            return fileTags.Count() == tagIds.Count() &&
                fileTags.Select(x => x.Id).Intersect(tagIds).Count() == tagIds.Count();
        });
    }

    public IEnumerable<FileModel> SearchFilesWithoutTags(IEnumerable<int> tagIds)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<FileModel>($"select * from [files] inner join filetags on files.Id = filetags.FileId where filetags.TagId not in ({string.Join(',', tagIds)})");
    }

    public IEnumerable<FileModel> SearchFilesWithMissingData()
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<FileModel>($"select * from [files] where Description is null and Id not in (select FileId from [filepersons]) and Id not in (select FileId from [filelocations]) and Id not in (select FileId from [filetags])");
    }

    public void InsertFile(string internalPath, string? description, IFilesystemAccess filesystemAccess, bool findMetadata)
    {
        if (!FileModelValidator.ValidateDescription(description))
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
            using var connection = DatabaseSetup.CreateConnection(database);
            var files = new FileModel() { Id = default, Path = internalPath, Description = description, Datetime = fileMetadata.Datetime, Position = fileMetadata.Position, Orientation = fileMetadata.Orientation };
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
            using var connection = DatabaseSetup.CreateConnection(database);
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
        if (!FileModelValidator.ValidateDescription(description))
        {
            throw new DataValidationException("Invalid description");
        }

        try
        {
            using var connection = DatabaseSetup.CreateConnection(database);
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
        if (!FileModelValidator.ValidateDatetime(datetime))
        {
            throw new DataValidationException("Invalid datetime");
        }

        try
        {
            using var connection = DatabaseSetup.CreateConnection(database);
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
        if (!FileModelValidator.ValidateOrientation(orientation))
        {
            throw new DataValidationException("Invalid orientation");
        }

        try
        {
            using var connection = DatabaseSetup.CreateConnection(database);
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
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "delete from [files] where Id = @id";
        connection.Execute(sql, new { id });
    }

    public void InsertFilePerson(int fileId, int personId)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "insert into [filepersons] (FileId, PersonId) values (@fileId, @personId)";
        connection.Execute(sql, new { fileId, personId });
    }

    public void DeleteFilePerson(int fileId, int personId)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "delete from [filepersons] where FileId = @fileId and PersonId = @personId";
        connection.Execute(sql, new { fileId, personId });
    }

    public void InsertFileLocation(int fileId, int locationId)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "insert into [filelocations] (Fileid, LocationId) values (@fileId, @locationId)";
        connection.Execute(sql, new { fileId, locationId });
    }

    public void DeleteFileLocation(int fileId, int locationId)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "delete from [filelocations] where FileId = @fileId and LocationId = @locationId";
        connection.Execute(sql, new { fileId, locationId });
    }

    public void InsertFileTag(int fileId, int tagId)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "insert into [filetags] (FileId, TagId) values (@fileId, @tagId)";
        connection.Execute(sql, new { fileId, tagId });
    }

    public void DeleteFileTag(int fileId, int tagId)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "delete from [filetags] where FileId = @fileId and TagId = @tagId";
        connection.Execute(sql, new { fileId, tagId });
    }

    #endregion

    #region Persons

    public IEnumerable<PersonModel> GetPersons()
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<PersonModel>("select * from [persons]");
    }

    public IEnumerable<PersonModel> GetPersonsFromFile(int fileId)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<PersonModel>("select * from [persons] where Id in (select PersonId from [filepersons] where FileId = @fileid)", new { fileid = fileId });
    }

    public int GetPersonCount()
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.ExecuteScalar<int>("select count(*) from [persons]");
    }

    public IEnumerable<PersonModel> SearchPersonsBySex(Sex sex)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "select * from [persons] where Sex = @sex";
        return connection.Query<PersonModel>(sql, new { sex });
    }

    public PersonModel GetPersonById(int id)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.QueryFirst<PersonModel>("select * from [persons] where Id=@id", new { id });
    }

    public bool HasPersonId(int id)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
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
            using var connection = DatabaseSetup.CreateConnection(database);
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
            using var connection = DatabaseSetup.CreateConnection(database);
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
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "delete from [persons] where Id = @id";
        connection.Execute(sql, new { id });
    }

    #endregion

    #region Locations

    public IEnumerable<LocationModel> GetLocations()
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<LocationModel>("select * from [locations]");
    }

    public IEnumerable<LocationModel> GetLocationsFromFile(int fileId)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<LocationModel>("select * from [locations] where Id in (select LocationId from [filelocations] where FileId = @fileId)", new { fileId });
    }

    public int GetLocationCount()
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.ExecuteScalar<int>("select count(*) from [locations]");
    }

    public LocationModel GetLocationById(int id)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.QueryFirst<LocationModel>("select * from [locations] where Id=@id", new { id });
    }

    public bool HasLocationId(int id)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
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
            using var connection = DatabaseSetup.CreateConnection(database);
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
            using var connection = DatabaseSetup.CreateConnection(database);
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
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "delete from [locations] where Id = @id";
        connection.Execute(sql, new { id });
    }

    #endregion

    #region Tags

    public IEnumerable<TagModel> GetTags()
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<TagModel>("select * from [tags]");
    }

    public IEnumerable<TagModel> GetTagsFromFile(int fileId)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.Query<TagModel>("select * from [tags] where Id in (select TagId from [filetags] where FileId = @fileid)", new { fileid = fileId });
    }

    public int GetTagCount()
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.ExecuteScalar<int>("select count(*) from [tags]");
    }

    public TagModel GetTagById(int id)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
        return connection.QueryFirst<TagModel>("select * from [tags] where Id=@id", new { id });
    }

    public bool HasTagId(int id)
    {
        using var connection = DatabaseSetup.CreateConnection(database);
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
            using var connection = DatabaseSetup.CreateConnection(database);
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
            using var connection = DatabaseSetup.CreateConnection(database);
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
        using var connection = DatabaseSetup.CreateConnection(database);
        var sql = "delete from [tags] where Id = @id";
        connection.Execute(sql, new { id });
    }

    #endregion
}
