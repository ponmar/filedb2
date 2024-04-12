using System;
using System.Collections.Generic;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.DatabaseAccess.SQLite;
using FileDBInterface.FilesystemAccess;
using FileDBShared.Model;

namespace FileDBAvalonia;

public class NoDatabaseAccess : IDatabaseAccess
{
    public string Database { get; set; } = string.Empty;

    public bool NeedsMigration => false;

    public List<DatabaseMigrationResult> Migrate()
    {
        return [];
    }

    public IEnumerable<FileModel> GetFiles()
    {
        return [];
    }

    public IEnumerable<string> GetDirectories()
    {
        return [];
    }

    public int GetFileCount()
    {
        return 0;
    }

    public IEnumerable<FileModel> SearchFilesFromIds(IEnumerable<int> fileIds)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesExceptIds(IEnumerable<int> fileIds)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFiles(string criteria, bool caseSensitive)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesBySex(Sex sex)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesByPath(string criteria)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesByExtension(string extension)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesRandom(int numFiles)
    {
        return [];
    }

    public IEnumerable<LocationModel> SearchLocationsNearGpsPosition(double latitude, double longitude, double radius)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesNearGpsPosition(double latitude, double longitude, double radius)
    {
        return [];
    }

    public FileModel GetFileById(int id)
    {
        throw new NotImplementedException();
    }

    public FileModel? GetFileByPath(string path)
    {
        return null;
    }

    public IEnumerable<FileModel> SearchFilesByDate(DateTime start, DateTime end)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesBySeason(Season season)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesByAnnualDate(int startMonth, int startDay, int endMonth, int endDay)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesByAnnualDate(int month, int day)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesWithoutDate()
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesWithPersons(IEnumerable<int> personIds)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesWithoutPersons(IEnumerable<int> personIds)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesWithLocations(IEnumerable<int> locationIds)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesWithoutLocations(IEnumerable<int> locationIds)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesWithTags(IEnumerable<int> tagIds)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesWithoutTags(IEnumerable<int> tagIds)
    {
        return [];
    }

    public IEnumerable<FileModel> SearchFilesWithMissingData()
    {
        return [];
    }

    public void InsertFile(string internalPath, string? description, IFilesystemAccess fileSystemAccess, bool findMetadata)
    {
    }

    public void UpdateFileFromMetaData(int id, IFilesystemAccess filesystemAccess)
    {
    }

    public void UpdateFileDescription(int id, string? description)
    {
    }

    public void UpdateFileDatetime(int id, string? datetime)
    {
    }

    public void UpdateFileOrientation(int id, int? orientation)
    {
    }

    public void DeleteFile(int id)
    {
    }

    public void InsertFilePerson(int fileId, int personId)
    {
    }

    public void DeleteFilePerson(int fileId, int personId)
    {
    }

    public void InsertFileLocation(int fileId, int locationId)
    {
    }

    public void DeleteFileLocation(int fileId, int locationId)
    {
    }

    public void InsertFileTag(int fileId, int tagId)
    {
    }

    public void DeleteFileTag(int fileId, int tagId)
    {
    }

    public IEnumerable<PersonModel> GetPersons()
    {
        return [];
    }

    public IEnumerable<PersonModel> GetPersonsFromFile(int fileId)
    {
        return [];
    }

    public int GetPersonCount()
    {
        return 0;
    }

    public static IEnumerable<PersonModel> SearchPersons(string criteria)
    {
        return [];
    }

    public PersonModel GetPersonById(int id)
    {
        throw new NotImplementedException();
    }

    public bool HasPersonId(int id)
    {
        return false;
    }

    public void InsertPerson(PersonModel person)
    {
    }

    public void UpdatePerson(PersonModel person)
    {
    }

    public void DeletePerson(int id)
    {
    }

    public IEnumerable<LocationModel> GetLocations()
    {
        return [];
    }

    public IEnumerable<LocationModel> GetLocationsFromFile(int fileId)
    {
        return [];
    }

    public int GetLocationCount()
    {
        return 0;
    }

    public LocationModel GetLocationById(int id)
    {
        throw new NotImplementedException();
    }

    public bool HasLocationId(int id)
    {
        return false;
    }

    public void InsertLocation(LocationModel location)
    {
    }

    public void UpdateLocation(LocationModel location)
    {
    }

    public void DeleteLocation(int id)
    {
    }

    public IEnumerable<TagModel> GetTags()
    {
        return [];
    }

    public IEnumerable<TagModel> GetTagsFromFile(int fileId)
    {
        return [];
    }

    public int GetTagCount()
    {
        return 0;
    }

    public TagModel GetTagById(int id)
    {
        throw new NotImplementedException();
    }

    public bool HasTagId(int id)
    {
        return false;
    }

    public void InsertTag(TagModel tag)
    {
    }

    public void UpdateTag(TagModel tag)
    {
    }

    public void DeleteTag(int id)
    {
    }
}
