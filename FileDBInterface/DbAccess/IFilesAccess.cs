using System;
using System.Collections.Generic;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;

namespace FileDBInterface.DbAccess
{
    public interface IFilesAccess
    {
        public IEnumerable<FilesModel> GetFiles();
        public int GetFileCount();
        public FilesModel? GetFileById(int id);
        public FilesModel? GetFileByPath(string path);
        public void InsertFile(string internalPath, string? description, IFilesystemAccess filesystemAccess);
        public void DeleteFile(int id);
        public void UpdateFileFromMetaData(int id, IFilesystemAccess filesystemAccess);
        public void UpdateFileDescription(int id, string? description);
        public void UpdateFileDatetime(int id, string? datetime);
        public void UpdateFileOrientation(int id, int? orientation);

        public IEnumerable<FilesModel> SearchFilesFromIds(IEnumerable<int> fileIds);
        public IEnumerable<FilesModel> SearchFiles(string criteria);
        public IEnumerable<FilesModel> SearchFilesBySex(Sex sex);
        public IEnumerable<FilesModel> SearchFilesByDate(DateTime start, DateTime end);
        public IEnumerable<FilesModel> SearchFilesByPath(string criteria);
        public IEnumerable<FilesModel> SearchFilesRandom(int numFiles);
        public IEnumerable<FilesModel> SearchFilesNearGpsPosition(double latitude, double longitude, double radius);
        public IEnumerable<FilesModel> SearchFilesWithPersons(IEnumerable<int> personIds);
        public IEnumerable<FilesModel> SearchFilesWithLocations(IEnumerable<int> locationIds);
        public IEnumerable<FilesModel> SearchFilesWithTags(IEnumerable<int> tagIds);
        public IEnumerable<FilesModel> SearchFilesWithMissingData();

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
}
