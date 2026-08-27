using System;
using System.Collections.Generic;
using FileDBInterface.DatabaseAccess.SQLite;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;

namespace FileDBInterface.DatabaseAccess;

/// <summary>
/// Wraps an <see cref="IDatabaseAccess"/> and blocks all write operations when the
/// application is running in read-only mode.
/// </summary>
public class ReadOnlyDatabaseAccess : IDatabaseAccess
{
    public IDatabaseAccess Inner { get; }

    public ReadOnlyDatabaseAccess(IDatabaseAccess inner)
    {
        Inner = inner;
    }

    private static T ThrowReadOnly<T>() =>
        throw new InvalidOperationException("Database is read-only.");

    private static void ThrowReadOnly() =>
        throw new InvalidOperationException("Database is read-only.");

    // ── Migration ──────────────────────────────────────────────────────────

    public bool NeedsMigration => Inner.NeedsMigration;

    public bool IsTooNew => Inner.IsTooNew;

    public List<DatabaseMigrationResult> Migrate() => ThrowReadOnly<List<DatabaseMigrationResult>>();

    // ── Files – reads ──────────────────────────────────────────────────────

    public IEnumerable<FileModel> GetFiles() => Inner.GetFiles();
    public IEnumerable<string> GetDirectories() => Inner.GetDirectories();
    public int GetFileCount() => Inner.GetFileCount();
    public FileModel? GetFileById(int id) => Inner.GetFileById(id);
    public FileModel? GetFileByPath(string path) => Inner.GetFileByPath(path);
    public PersonBoundingBox? GetFilePersonBoundingBox(int fileId, int personId) => Inner.GetFilePersonBoundingBox(fileId, personId);
    public IEnumerable<(int PersonId, PersonBoundingBox BoundingBox)> GetFilePersonBoundingBoxes(int fileId) => Inner.GetFilePersonBoundingBoxes(fileId);

    public IEnumerable<FileModel> SearchFilesFromIds(IEnumerable<int> fileIds) => Inner.SearchFilesFromIds(fileIds);
    public IEnumerable<FileModel> SearchFilesExceptIds(IEnumerable<int> fileIds) => Inner.SearchFilesExceptIds(fileIds);
    public IEnumerable<FileModel> SearchFiles(string criteria, bool caseSensitive) => Inner.SearchFiles(criteria, caseSensitive);
    public IEnumerable<FileModel> SearchFilesBySex(Sex sex) => Inner.SearchFilesBySex(sex);
    public IEnumerable<FileModel> SearchFilesByTime(TimeOnly start, TimeOnly end) => Inner.SearchFilesByTime(start, end);
    public IEnumerable<FileModel> SearchFilesByDate(DateTime date) => Inner.SearchFilesByDate(date);
    public IEnumerable<FileModel> SearchFilesByDate(DateTime start, DateTime end) => Inner.SearchFilesByDate(start, end);
    public IEnumerable<FileModel> SearchFilesBySeason(Season season) => Inner.SearchFilesBySeason(season);
    public IEnumerable<FileModel> SearchFilesByAnnualDate(int startMonth, int startDay, int endMonth, int endDay) => Inner.SearchFilesByAnnualDate(startMonth, startDay, endMonth, endDay);
    public IEnumerable<FileModel> SearchFilesByAnnualDate(int month, int day) => Inner.SearchFilesByAnnualDate(month, day);
    public IEnumerable<FileModel> SearchFilesWithoutDate() => Inner.SearchFilesWithoutDate();
    public IEnumerable<FileModel> SearchFilesByPath(string criteria) => Inner.SearchFilesByPath(criteria);
    public IEnumerable<FileModel> SearchFilesByExtension(string extension) => Inner.SearchFilesByExtension(extension);
    public IEnumerable<FileModel> SearchFilesRandom(int numFiles) => Inner.SearchFilesRandom(numFiles);
    public IEnumerable<FileModel> SearchFilesNearGpsPosition(double latitude, double longitude, double radius) => Inner.SearchFilesNearGpsPosition(latitude, longitude, radius);
    public IEnumerable<FileModel> SearchFilesByNumPersons(Range numPersonsRange) => Inner.SearchFilesByNumPersons(numPersonsRange);
    public IEnumerable<FileModel> SearchFilesWithPersons(IEnumerable<int> personIds) => Inner.SearchFilesWithPersons(personIds);
    public IEnumerable<FileModel> SearchFilesWithPersonGroup(IEnumerable<int> personIds) => Inner.SearchFilesWithPersonGroup(personIds);
    public IEnumerable<FileModel> SearchFilesWithPersonGroupOnly(IEnumerable<int> personIds) => Inner.SearchFilesWithPersonGroupOnly(personIds);
    public IEnumerable<FileModel> SearchFilesWithoutPerson(int personId) => Inner.SearchFilesWithoutPerson(personId);
    public IEnumerable<FileModel> SearchFilesWithLocations(IEnumerable<int> locationIds) => Inner.SearchFilesWithLocations(locationIds);
    public IEnumerable<FileModel> SearchFilesWithoutLocation(int locationId) => Inner.SearchFilesWithoutLocation(locationId);
    public IEnumerable<FileModel> SearchFilesWithTags(IEnumerable<int> tagIds) => Inner.SearchFilesWithTags(tagIds);
    public IEnumerable<FileModel> SearchFilesWithTagGroup(IEnumerable<int> tagIds) => Inner.SearchFilesWithTagGroup(tagIds);
    public IEnumerable<FileModel> SearchFilesWithTagGroupOnly(IEnumerable<int> tagIds) => Inner.SearchFilesWithTagGroupOnly(tagIds);
    public IEnumerable<FileModel> SearchFilesWithoutTag(int tagId) => Inner.SearchFilesWithoutTag(tagId);
    public IEnumerable<FileModel> SearchFilesWithMissingData() => Inner.SearchFilesWithMissingData();

    public IEnumerable<PersonModel> GetPersonsFromFile(int fileId) => Inner.GetPersonsFromFile(fileId);
    public IReadOnlyDictionary<int, int> GetPersonCountsFromFiles(IEnumerable<int> fileIds) => Inner.GetPersonCountsFromFiles(fileIds);
    public IEnumerable<LocationModel> GetLocationsFromFile(int fileId) => Inner.GetLocationsFromFile(fileId);
    public IEnumerable<TagModel> GetTagsFromFile(int fileId) => Inner.GetTagsFromFile(fileId);

    // ── Files – writes ─────────────────────────────────────────────────────

    public int InsertFile(string internalPath, string? description, IFilesystemAccess filesystemAccess, bool findMetadata) => ThrowReadOnly<int>();
    public void DeleteFile(int id) => ThrowReadOnly();
    public void UpdateFileFromMetaData(int id, IFilesystemAccess filesystemAccess) => ThrowReadOnly();
    public void UpdateFileDescription(int id, string? description) => ThrowReadOnly();
    public void UpdateFileDatetime(int id, string? datetime) => ThrowReadOnly();
    public void UpdateFileOrientation(int id, int? orientation) => ThrowReadOnly();

    public void InsertFilePerson(int fileId, int personId) => ThrowReadOnly();
    public void DeleteFilePerson(int fileId, int personId) => ThrowReadOnly();
    public void UpdateFilePersonBoundingBox(int fileId, int personId, PersonBoundingBox? boundingBox) => ThrowReadOnly();

    public void InsertFileLocation(int fileId, int locationId) => ThrowReadOnly();
    public void DeleteFileLocation(int fileId, int locationId) => ThrowReadOnly();

    public void InsertFileTag(int fileId, int tagId) => ThrowReadOnly();
    public void DeleteFileTag(int fileId, int tagId) => ThrowReadOnly();

    // ── Persons – reads ────────────────────────────────────────────────────

    public IEnumerable<PersonModel> GetPersons() => Inner.GetPersons();
    public int GetPersonCount() => Inner.GetPersonCount();
    public PersonModel GetPersonById(int id) => Inner.GetPersonById(id);
    public bool HasPersonId(int id) => Inner.HasPersonId(id);

    // ── Persons – writes ───────────────────────────────────────────────────

    public int InsertPerson(PersonModel person) => ThrowReadOnly<int>();
    public void UpdatePerson(PersonModel person) => ThrowReadOnly();
    public void DeletePerson(int id) => ThrowReadOnly();

    // ── Locations – reads ──────────────────────────────────────────────────

    public IEnumerable<LocationModel> GetLocations() => Inner.GetLocations();
    public int GetLocationCount() => Inner.GetLocationCount();
    public LocationModel GetLocationById(int id) => Inner.GetLocationById(id);
    public bool HasLocationId(int id) => Inner.HasLocationId(id);
    public IEnumerable<LocationModel> SearchLocationsNearGpsPosition(double latitude, double longitude, double radius) => Inner.SearchLocationsNearGpsPosition(latitude, longitude, radius);

    // ── Locations – writes ─────────────────────────────────────────────────

    public int InsertLocation(LocationModel location) => ThrowReadOnly<int>();
    public void UpdateLocation(LocationModel location) => ThrowReadOnly();
    public void DeleteLocation(int id) => ThrowReadOnly();

    // ── Tags – reads ───────────────────────────────────────────────────────

    public IEnumerable<TagModel> GetTags() => Inner.GetTags();
    public int GetTagCount() => Inner.GetTagCount();
    public TagModel GetTagById(int id) => Inner.GetTagById(id);
    public bool HasTagId(int id) => Inner.HasTagId(id);

    // ── Tags – writes ──────────────────────────────────────────────────────

    public int InsertTag(TagModel tag) => ThrowReadOnly<int>();
    public void UpdateTag(TagModel tag) => ThrowReadOnly();
    public void DeleteTag(int id) => ThrowReadOnly();
}
