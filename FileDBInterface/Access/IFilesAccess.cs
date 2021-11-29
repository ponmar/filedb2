using System;
using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDBInterface.Access
{
    public interface IFilesAccess
    {
        public IEnumerable<FilesModel> GetFiles();
        public int GetFileCount();
        public IEnumerable<FilesModel> SearchFiles(string criteria);
        public IEnumerable<FilesModel> SearchFilesBySex(Sex sex);
        public IEnumerable<FilesModel> GetFileByDate(DateTime start, DateTime end);
        public IEnumerable<FilesModel> SearchFilesByPath(string criteria);
        public IEnumerable<FilesModel> SearchFilesRandom(int numFiles);
        public IEnumerable<FilesModel> SearchFilesNearGpsPosition(double latitude, double longitude, double radius);
        public FilesModel GetFileById(int id);
        public FilesModel GetFileByPath(string path);
        public IEnumerable<FilesModel> GetFilesWithPersons(IEnumerable<int> personIds);
        public IEnumerable<FilesModel> GetFilesWithLocations(IEnumerable<int> locationIds);
        public IEnumerable<FilesModel> GetFilesWithTags(IEnumerable<int> tagIds);
        public IEnumerable<FilesModel> GetFilesWithMissingData();

        public void InsertFile(string internalPath, string description = null);
        public void UpdateFileFromMetaData(int id);
        public void UpdateFileDescription(int id, string description);
        public void DeleteFile(int id);

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
