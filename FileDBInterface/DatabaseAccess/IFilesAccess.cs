using System;
using System.Collections.Generic;
using FileDBInterface.FilesystemAccess;
using FileDBShared.Model;

namespace FileDBInterface.DatabaseAccess;

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter,
}

public interface IFilesAccess
{
    public IEnumerable<FileModel> GetFiles();
    public IEnumerable<string> GetDirectories();
    public int GetFileCount();
    public FileModel? GetFileById(int id);
    public FileModel? GetFileByPath(string path);
    public void InsertFile(string internalPath, string? description, IFilesystemAccess filesystemAccess, bool findMetadata);
    public void DeleteFile(int id);
    public void UpdateFileFromMetaData(int id, IFilesystemAccess filesystemAccess);
    public void UpdateFileDescription(int id, string? description);
    public void UpdateFileDatetime(int id, string? datetime);
    public void UpdateFileOrientation(int id, int? orientation);

    public IEnumerable<FileModel> SearchFilesFromIds(IEnumerable<int> fileIds);
    public IEnumerable<FileModel> SearchFilesExceptIds(IEnumerable<int> fileIds);
    public IEnumerable<FileModel> SearchFiles(string criteria, bool caseSensitive);
    public IEnumerable<FileModel> SearchFilesBySex(Sex sex);
    public IEnumerable<FileModel> SearchFilesByDate(DateTime start, DateTime end);
    public IEnumerable<FileModel> SearchFilesBySeason(Season season);
    public IEnumerable<FileModel> SearchFilesWithoutDate();
    public IEnumerable<FileModel> SearchFilesByPath(string criteria);
    public IEnumerable<FileModel> SearchFilesByExtension(string extension);
    public IEnumerable<FileModel> SearchFilesRandom(int numFiles);
    public IEnumerable<FileModel> SearchFilesNearGpsPosition(double latitude, double longitude, double radius);
    public IEnumerable<FileModel> SearchFilesWithPersons(IEnumerable<int> personIds);
    public IEnumerable<FileModel> SearchFilesWithoutPersons(IEnumerable<int> personIds);
    public IEnumerable<FileModel> SearchFilesWithLocations(IEnumerable<int> locationIds);
    public IEnumerable<FileModel> SearchFilesWithoutLocations(IEnumerable<int> locationIds);
    public IEnumerable<FileModel> SearchFilesWithTags(IEnumerable<int> tagIds);
    public IEnumerable<FileModel> SearchFilesWithoutTags(IEnumerable<int> tagIds);
    public IEnumerable<FileModel> SearchFilesWithMissingData();

    public void InsertFilePerson(int fileId, int personId);
    public void DeleteFilePerson(int fileId, int personId);

    public void InsertFileLocation(int fileId, int locationId);
    public void DeleteFileLocation(int fileId, int locationId);

    public void InsertFileTag(int fileId, int tagId);
    public void DeleteFileTag(int fileId, int tagId);

    public IEnumerable<PersonModel> GetPersonsFromFile(int fileId);
    public IEnumerable<LocationModel> GetLocationsFromFile(int fileId);
    public IEnumerable<TagModel> GetTagsFromFile(int fileId);
}
