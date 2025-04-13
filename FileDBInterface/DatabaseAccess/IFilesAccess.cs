using System;
using System.Collections.Generic;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;

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
    IEnumerable<FileModel> GetFiles();
    IEnumerable<string> GetDirectories();
    int GetFileCount();
    FileModel? GetFileById(int id);
    FileModel? GetFileByPath(string path);
    void InsertFile(string internalPath, string? description, IFilesystemAccess filesystemAccess, bool findMetadata);
    void DeleteFile(int id);
    void UpdateFileFromMetaData(int id, IFilesystemAccess filesystemAccess);
    void UpdateFileDescription(int id, string? description);
    void UpdateFileDatetime(int id, string? datetime);
    void UpdateFileOrientation(int id, int? orientation);

    IEnumerable<FileModel> SearchFilesFromIds(IEnumerable<int> fileIds);
    IEnumerable<FileModel> SearchFilesExceptIds(IEnumerable<int> fileIds);
    IEnumerable<FileModel> SearchFiles(string criteria, bool caseSensitive);
    IEnumerable<FileModel> SearchFilesBySex(Sex sex);
    IEnumerable<FileModel> SearchFilesByTime(TimeOnly start, TimeOnly end);
    IEnumerable<FileModel> SearchFilesByDate(DateTime date);
    IEnumerable<FileModel> SearchFilesByDate(DateTime start, DateTime end);
    IEnumerable<FileModel> SearchFilesBySeason(Season season);
    IEnumerable<FileModel> SearchFilesByAnnualDate(int startMonth, int startDay, int endMonth, int endDay);
    IEnumerable<FileModel> SearchFilesByAnnualDate(int month, int day);
    IEnumerable<FileModel> SearchFilesWithoutDate();
    IEnumerable<FileModel> SearchFilesByPath(string criteria);
    IEnumerable<FileModel> SearchFilesByExtension(string extension);
    IEnumerable<FileModel> SearchFilesRandom(int numFiles);
    IEnumerable<FileModel> SearchFilesNearGpsPosition(double latitude, double longitude, double radius);
    IEnumerable<FileModel> SearchFilesWithPersons(IEnumerable<int> personIds, bool allowOtherPersons = true);
    IEnumerable<FileModel> SearchFilesWithoutPersons(IEnumerable<int> personIds);
    IEnumerable<FileModel> SearchFilesWithLocations(IEnumerable<int> locationIds);
    IEnumerable<FileModel> SearchFilesWithoutLocations(IEnumerable<int> locationIds);
    IEnumerable<FileModel> SearchFilesWithTags(IEnumerable<int> tagIds);
    IEnumerable<FileModel> SearchFilesWithoutTags(IEnumerable<int> tagIds);
    IEnumerable<FileModel> SearchFilesWithMissingData();

    void InsertFilePerson(int fileId, int personId);
    void DeleteFilePerson(int fileId, int personId);

    void InsertFileLocation(int fileId, int locationId);
    void DeleteFileLocation(int fileId, int locationId);

    void InsertFileTag(int fileId, int tagId);
    void DeleteFileTag(int fileId, int tagId);

    IEnumerable<PersonModel> GetPersonsFromFile(int fileId);
    IEnumerable<LocationModel> GetLocationsFromFile(int fileId);
    IEnumerable<TagModel> GetTagsFromFile(int fileId);
}
