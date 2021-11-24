﻿using System;
using System.Collections.Generic;
using FileDBInterface.Model;
using MetadataExtractor;

namespace FileDBInterface
{
    public interface IDatabaseWrapper
    {
        public IEnumerable<string> ListNewFilesystemFiles(IEnumerable<string> blacklistedFilePathPatterns, IEnumerable<string> whitelistedFilePathPatterns, bool includeHiddenDirectories);
        public IEnumerable<string> ListAllFilesystemDirectories();
        public IEnumerable<FilesModel> GetFilesMissingInFilesystem();
        public IEnumerable<FilesModel> GetFiles();
        public int GetFileCount();
        public IEnumerable<FilesModel> SearchFiles(string criteria);
        public IEnumerable<FilesModel> SearchFilesBySex(Sex sex);
        public IEnumerable<FilesModel> GetFileByDate(DateTime start, DateTime end);
        public IEnumerable<FilesModel> SearchFilesByPath(string criteria);
        public IEnumerable<FilesModel> SearchFilesRandom(int numFiles);
        public IEnumerable<FilesModel> SearchFilesNearGpsPosition(double latitude, double longitude, double radius);
        public FilesModel GetFileById(int id);
        public bool HasFileId(int id);
        public FilesModel GetFileByPath(string path);
        public IEnumerable<FilesModel> GetFilesWithPersons(IEnumerable<int> personIds);
        public IEnumerable<FilesModel> GetFilesWithLocations(IEnumerable<int> locationIds);
        public IEnumerable<FilesModel> GetFilesWithTags(IEnumerable<int> tagIds);
        public IEnumerable<FilesModel> GetFilesWithMissingData();
        public bool HasFilePath(string path);
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
        public IEnumerable<PersonModel> GetPersons();
        public IEnumerable<PersonModel> GetPersonsFromFile(int fileId);
        public int GetPersonCount();
        public IEnumerable<PersonModel> SearchPersons(string criteria);
        public PersonModel GetPersonById(int id);
        public bool HasPersonId(int id);
        public void InsertPerson(string firstname, string lastname, string description = null, string dateOfBirth = null, string deceased = null, int? profileFileId = null, Sex sex = Sex.NotApplicable);
        public void UpdatePerson(int id, string firstname, string lastname, string description = null, string dateOfBirth = null, string deceased = null, int? profileFileId = null, Sex sex = Sex.NotApplicable);
        public void DeletePerson(int id);
        public IEnumerable<LocationModel> GetLocations();
        public IEnumerable<LocationModel> GetLocationsFromFile(int fileId);
        public int GetLocationCount();
        public IEnumerable<LocationModel> SearchLocations(string criteria);
        public LocationModel GetLocationById(int id);
        public bool HasLocationId(int id);
        public void InsertLocation(string name, string description = null, string position = null);
        public void UpdateLocation(int id, string name, string description = null, string position = null);
        public void DeleteLocation(int id);
        public IEnumerable<TagModel> GetTags();
        public IEnumerable<TagModel> GetTagsFromFile(int fileId);
        public int GetTagCount();
        public IEnumerable<TagModel> SearchTags(string criteria);
        public TagModel GetTagById(int id);
        public bool HasTagId(int id);
        public void InsertTag(string name);
        public void UpdateTag(int id, string name);
        public void DeleteTag(int id);
        public string ToAbsolutePath(string internalPath);
    }
}
