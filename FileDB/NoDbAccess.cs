using System;
using System.Collections.Generic;
using FileDBInterface.DbAccess;
using FileDBInterface.FilesystemAccess;
using FileDBShared.Model;

namespace FileDB;

public class NoDbAccess : IDbAccess
{
    public string Database { get; set; } = string.Empty;

    public IEnumerable<FileModel> GetFiles()
    {
        return new List<FileModel>();
    }

    public int GetFileCount()
    {
        return 0;
    }

    public IEnumerable<FileModel> SearchFilesFromIds(IEnumerable<int> fileIds)
    {
        return new List<FileModel>();
    }

    public IEnumerable<FileModel> SearchFiles(string criteria)
    {
        return new List<FileModel>();
    }

    public IEnumerable<FileModel> SearchFilesBySex(Sex sex)
    {
        return new List<FileModel>();
    }

    public IEnumerable<FileModel> SearchFilesByPath(string criteria)
    {
        return new List<FileModel>();
    }

    public IEnumerable<FileModel> SearchFilesByExtension(string extension)
    {
        return new List<FileModel>();
    }

    public IEnumerable<FileModel> SearchFilesRandom(int numFiles)
    {
        return new List<FileModel>();
    }

    public IEnumerable<LocationModel> SearchLocationsNearGpsPosition(double latitude, double longitude, double radius)
    {
        return new List<LocationModel>();
    }

    public IEnumerable<FileModel> SearchFilesNearGpsPosition(double latitude, double longitude, double radius)
    {
        return new List<FileModel>();
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
        return new List<FileModel>();
    }

    public IEnumerable<FileModel> SearchFilesWithPersons(IEnumerable<int> personIds)
    {
        return new List<FileModel>();
    }

    public IEnumerable<FileModel> SearchFilesWithLocations(IEnumerable<int> locationIds)
    {
        return new List<FileModel>();
    }

    public IEnumerable<FileModel> SearchFilesWithTags(IEnumerable<int> tagIds)
    {
        return new List<FileModel>();
    }

    public IEnumerable<FileModel> SearchFilesWithMissingData()
    {
        return new List<FileModel>();
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
        return new List<PersonModel>();
    }

    public IEnumerable<PersonModel> GetPersonsFromFile(int fileId)
    {
        return new List<PersonModel>();
    }

    public int GetPersonCount()
    {
        return 0;
    }

    public IEnumerable<PersonModel> SearchPersons(string criteria)
    {
        return new List<PersonModel>();
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
        return new List<LocationModel>();
    }

    public IEnumerable<LocationModel> GetLocationsFromFile(int fileId)
    {
        return new List<LocationModel>();
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
        return new List<TagModel>();
    }

    public IEnumerable<TagModel> GetTagsFromFile(int fileId)
    {
        return new List<TagModel>();
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
