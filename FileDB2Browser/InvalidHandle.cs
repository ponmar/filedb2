﻿using System;
using System.Collections.Generic;
using FileDB2Interface;
using FileDB2Interface.Model;
using MetadataExtractor;

namespace FileDB2Browser
{
    public class InvalidHandle : IFileDB2Handle
    {
        public IEnumerable<string> ListNewFilesystemFiles(List<string> blacklistedFilePathPatterns, List<string> whitelistedFilePathPatterns, bool includeHiddenDirectories)
        {
            return new List<string>();
        }

        public IEnumerable<string> ListAllFilesystemDirectories()
        {
            return new List<string>();
        }

        public bool ParseFilesystemFileExif(string path, out DateTime? dateTaken, out GeoLocation location)
        {
            dateTaken = null;
            location = null;
            return false;
        }

        public IEnumerable<FilesModel> GetFilesMissingInFilesystem()
        {
            return new List<FilesModel>();
        }

        public IEnumerable<FilesModel> GetFiles()
        {
            return new List<FilesModel>();
        }

        public int GetFileCount()
        {
            return 0;
        }

        public IEnumerable<FilesModel> SearchFiles(string criteria)
        {
            return new List<FilesModel>();
        }

        public IEnumerable<FilesModel> SearchFilesBySex(Sex sex)
        {
            return new List<FilesModel>();
        }

        public IEnumerable<FilesModel> SearchFilesByPath(string criteria)
        {
            return new List<FilesModel>();
        }

        public IEnumerable<FilesModel> SearchFilesRandom(int numFiles)
        {
            return new List<FilesModel>();
        }

        public FilesModel GetFileById(int id)
        {
            return null;
        }

        public bool HasFileId(int id)
        {
            return false;
        }

        public FilesModel GetFileByPath(string path)
        {
            return null;
        }

        public IEnumerable<FilesModel> GetFileByDate(DateTime start, DateTime end)
        {
            return new List<FilesModel>();
        }

        public IEnumerable<FilesModel> GetFilesWithPersons(IEnumerable<int> personIds)
        {
            return new List<FilesModel>();
        }

        public IEnumerable<FilesModel> GetFilesWithLocations(IEnumerable<int> locationIds)
        {
            return new List<FilesModel>();
        }

        public IEnumerable<FilesModel> GetFilesWithTags(IEnumerable<int> tagIds)
        {
            return new List<FilesModel>();
        }

        public IEnumerable<FilesModel> GetFilesWithMissingData()
        {
            return new List<FilesModel>();
        }

        public bool HasFilePath(string path)
        {
            return false;
        }

        public void InsertFile(string internalPath, string description = null)
        {
        }

        public void UpdateFileDescription(int id, string description)
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
            return null;
        }

        public bool HasPersonId(int id)
        {
            return false;
        }

        public void InsertPerson(string firstname, string lastname, string description = null, string dateOfBirth = null, int? profileFileId = null, Sex sex = Sex.NotApplicable)
        {
        }

        public void UpdatePerson(int id, string firstname, string lastname, string description = null, string dateOfBirth = null, int? profileFileId = null, Sex sex = Sex.NotApplicable)
        {
        }

        public void UpdatePersonFirstname(int id, string firstname)
        {
        }

        public void UpdatePersonLastname(int id, string lastname)
        {
        }

        public void UpdatePersonDescription(int id, string description)
        {
        }

        public void UpdatePersonDateOfBirth(int id, string dateOfBirthStr)
        {
        }

        public void UpdatePersonDateOfBirth(int id, DateTime dateOfBirth)
        {
        }

        public void UpdatePersonProfileFileId(int id, int? profileFileId)
        {
        }

        public void UpdatePersonSex(int id, Sex sex)
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

        public IEnumerable<LocationModel> SearchLocations(string criteria)
        {
            return new List<LocationModel>();
        }

        public LocationModel GetLocationById(int id)
        {
            return null;
        }

        public bool HasLocationId(int id)
        {
            return false;
        }

        public void InsertLocation(string name, string description = null, string geoLocation = null)
        {
        }

        public void InsertLocation(string name, string description = null, GeoLocation geoLocation = null)
        {
        }

        public void UpdateLocationName(int id, string name)
        {
        }

        public void UpdateLocationDescription(int id, string description)
        {
        }

        public void UpdateLocationPosition(int id, string geoLocation)
        {
        }

        public void UpdateLocationPosition(int id, GeoLocation geoLocation)
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

        public IEnumerable<TagModel> SearchTags(string criteria)
        {
            return new List<TagModel>();
        }

        public TagModel GetTagById(int id)
        {
            return null;
        }

        public bool HasTagId(int id)
        {
            return false;
        }

        public void InsertTag(string name)
        {
        }

        public void UpdateTagName(int id, string name)
        {
        }

        public void DeleteTag(int id)
        {
        }

        public string InternalPathToPath(string internalPath)
        {
            return string.Empty;
        }

        public void ValidateFileDescription(string description)
        {
        }

        public void ValidatePersonFirstname(string firstname)
        {
        }

        public void ValidatePersonLastname(string lastname)
        {
        }

        public void ValidatePersonDescription(string description)
        {
        }

        public DateTime ParseDateOfBirth(string dateOfBirthStr)
        {
            return DateTime.Now;
        }

        public void ValidatePersonDateOfBirth(string dateOfBirthStr)
        {
        }

        public void ValidatePersonProfileFileId(int? profileFileId)
        {
        }

        public void ValidateTagName(string name)
        {
        }

        public void ValidateLocationName(string name)
        {
        }

        public void ValidateLocationDescription(string description)
        {
        }

        public void ValidateLocationGeoLocation(string geoLocationStr, out GeoLocation geoLocation)
        {
            geoLocation = null;
        }
    }
}
