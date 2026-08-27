using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using FileDBInterface.DatabaseAccess.SQLite;
using FileDBInterface.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FileDBInterfaceTests.DatabaseAccess.SQLite;

public class SqLiteDatabaseAccessTests : IDisposable
{
    private readonly string _connectionString;
    private readonly IDbConnection _anchor;
    private readonly SqLiteDatabaseAccess _db;

    public SqLiteDatabaseAccessTests()
    {
        var uniqueName = Guid.NewGuid().ToString("N");
        _connectionString = $"FullUri=file:{uniqueName}?mode=memory&cache=shared;foreign keys=true";
        _anchor = new SQLiteConnection(_connectionString);
        _anchor.Open();
        _anchor.Execute(SqLiteDatabaseCreator.DatabaseCreationSql);
        _db = new SqLiteDatabaseAccess(_connectionString, NullLoggerFactory.Instance);
    }

    public void Dispose()
    {
        _anchor.Dispose();
    }

    private IDbConnection OpenConnection() => new SQLiteConnection(_connectionString);

    private int InsertFileRow(string path = "file.jpg", string? description = null, string? datetime = null, string? position = null)
    {
        using var conn = OpenConnection();
        conn.Open();
        conn.Execute(
            "insert into [files] (Path, Description, Datetime, Position) values (@path, @description, @datetime, @position)",
            new { path, description, datetime, position });
        return (int)conn.ExecuteScalar<long>("select last_insert_rowid()");
    }

    private int InsertPersonRow(string shortName = "JD", string fullName = "John Doe", Sex sex = Sex.NotKnown)
    {
        using var conn = OpenConnection();
        conn.Open();
        conn.Execute(
            "insert into [persons] (ShortName, FullName, Sex) values (@shortName, @fullName, @sex)",
            new { shortName, fullName, sex = (int)sex });
        return (int)conn.ExecuteScalar<long>("select last_insert_rowid()");
    }

    private int InsertLocationRow(string name = "Home", string? position = null)
    {
        using var conn = OpenConnection();
        conn.Open();
        conn.Execute(
            "insert into [locations] (Name, Position) values (@name, @position)",
            new { name, position });
        return (int)conn.ExecuteScalar<long>("select last_insert_rowid()");
    }

    private int InsertTagRow(string name = "Tag1")
    {
        using var conn = OpenConnection();
        conn.Open();
        conn.Execute("insert into [tags] (Name) values (@name)", new { name });
        return (int)conn.ExecuteScalar<long>("select last_insert_rowid()");
    }

    #region Files

    [Fact]
    public void GetFiles_EmptyDb_ReturnsEmpty()
    {
        Assert.Empty(_db.GetFiles());
    }

    [Fact]
    public void GetFileCount_EmptyDb_ReturnsZero()
    {
        Assert.Equal(0, _db.GetFileCount());
    }

    [Fact]
    public void GetFileCount_AfterInsert_ReturnsOne()
    {
        InsertFileRow();
        Assert.Equal(1, _db.GetFileCount());
    }

    [Fact]
    public void GetFiles_AfterInsert_ReturnsFile()
    {
        InsertFileRow("photos/img.jpg", "a photo");
        var files = _db.GetFiles().ToList();
        Assert.Single(files);
        Assert.Equal("photos/img.jpg", files[0].Path);
        Assert.Equal("a photo", files[0].Description);
    }

    [Fact]
    public void GetFileById_ExistingFile_ReturnsCorrectFile()
    {
        var id = InsertFileRow("img.jpg");
        var file = _db.GetFileById(id);
        Assert.Equal(id, file.Id);
        Assert.Equal("img.jpg", file.Path);
    }

    [Fact]
    public void GetFileByPath_ExistingFile_ReturnsFile()
    {
        InsertFileRow("img.jpg");
        var file = _db.GetFileByPath("img.jpg");
        Assert.NotNull(file);
        Assert.Equal("img.jpg", file.Path);
    }

    [Fact]
    public void GetFileByPath_NonExistingPath_ReturnsNull()
    {
        Assert.Null(_db.GetFileByPath("nonexistent.jpg"));
    }

    [Fact]
    public void DeleteFile_ExistingFile_RemovesIt()
    {
        var id = InsertFileRow("img.jpg");
        _db.DeleteFile(id);
        Assert.Equal(0, _db.GetFileCount());
    }

    [Fact]
    public void UpdateFileDescription_UpdatesCorrectly()
    {
        var id = InsertFileRow("img.jpg");
        _db.UpdateFileDescription(id, "new description");
        Assert.Equal("new description", _db.GetFileById(id).Description);
    }

    [Fact]
    public void UpdateFileDatetime_UpdatesCorrectly()
    {
        var id = InsertFileRow("img.jpg");
        _db.UpdateFileDatetime(id, "2024-06-15");
        Assert.Equal("2024-06-15", _db.GetFileById(id).Datetime);
    }

    [Fact]
    public void UpdateFileOrientation_UpdatesCorrectly()
    {
        var id = InsertFileRow("img.jpg");
        _db.UpdateFileOrientation(id, 3);
        Assert.Equal(3, _db.GetFileById(id).Orientation);
    }

    [Fact]
    public void GetDirectories_ReturnsDistinctDirectories()
    {
        InsertFileRow("dir1/a.jpg");
        InsertFileRow("dir1/b.jpg");
        InsertFileRow("dir2/c.jpg");
        InsertFileRow("root.jpg");
        var dirs = _db.GetDirectories().ToList();
        Assert.Equal(2, dirs.Count);
        Assert.Contains("dir1", dirs);
        Assert.Contains("dir2", dirs);
    }

    #endregion

    #region Search Files

    [Fact]
    public void SearchFilesFromIds_ReturnsOnlyMatchingFiles()
    {
        var id1 = InsertFileRow("a.jpg");
        var id2 = InsertFileRow("b.jpg");
        InsertFileRow("c.jpg");
        var result = _db.SearchFilesFromIds([id1, id2]).ToList();
        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.Contains(f.Id, new[] { id1, id2 }));
    }

    [Fact]
    public void SearchFilesExceptIds_ReturnsNonMatchingFiles()
    {
        var id1 = InsertFileRow("a.jpg");
        InsertFileRow("b.jpg");
        var id3 = InsertFileRow("c.jpg");
        var result = _db.SearchFilesExceptIds([id1, id3]).ToList();
        Assert.Single(result);
        Assert.Equal("b.jpg", result[0].Path);
    }

    [Fact]
    public void SearchFilesByPath_ReturnsFilesMatchingPrefix()
    {
        InsertFileRow("photos/img.jpg");
        InsertFileRow("videos/clip.mp4");
        var result = _db.SearchFilesByPath("photos/").ToList();
        Assert.Single(result);
        Assert.Equal("photos/img.jpg", result[0].Path);
    }

    [Fact]
    public void SearchFilesByExtension_ReturnsFilesMatchingSuffix()
    {
        InsertFileRow("a.jpg");
        InsertFileRow("b.png");
        InsertFileRow("c.jpg");
        var result = _db.SearchFilesByExtension(".jpg").ToList();
        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.EndsWith(".jpg", f.Path));
    }

    [Fact]
    public void SearchFiles_CaseInsensitive_ReturnsResults()
    {
        InsertFileRow("PHOTO.jpg", "A Summer Day");
        var result = _db.SearchFiles("summer", caseSensitive: false).ToList();
        Assert.Single(result);
    }

    [Fact]
    public void SearchFiles_CaseSensitive_FiltersCorrectly()
    {
        InsertFileRow("photo.jpg", "A Summer Day");
        InsertFileRow("other.jpg", "another summer day");
        var result = _db.SearchFiles("Summer", caseSensitive: true).ToList();
        Assert.Single(result);
        Assert.Equal("photo.jpg", result[0].Path);
    }

    [Fact]
    public void SearchFilesWithoutDate_ReturnsFilesWithNullDatetime()
    {
        InsertFileRow("no_date.jpg");
        InsertFileRow("with_date.jpg", datetime: "2024-01-01");
        var result = _db.SearchFilesWithoutDate().ToList();
        Assert.Single(result);
        Assert.Equal("no_date.jpg", result[0].Path);
    }

    [Fact]
    public void SearchFilesRandom_ReturnsRequestedCount()
    {
        for (var i = 0; i < 10; i++)
            InsertFileRow($"file{i}.jpg");
        var result = _db.SearchFilesRandom(3).ToList();
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void SearchFilesWithMissingData_ReturnsFilesWithNoMetadataOrRelations()
    {
        var missingId = InsertFileRow("missing.jpg");
        var withDesc = InsertFileRow("withdesc.jpg", description: "has description");
        var personId = InsertPersonRow();
        var withPerson = InsertFileRow("withperson.jpg");
        _db.InsertFilePerson(withPerson, personId);

        var result = _db.SearchFilesWithMissingData().ToList();
        Assert.Single(result);
        Assert.Equal(missingId, result[0].Id);
    }

    [Fact]
    public void SearchFilesByDate_ReturnsFilesOnExactDate()
    {
        InsertFileRow("match.jpg", datetime: "2024-06-15");
        InsertFileRow("no_match.jpg", datetime: "2024-06-16");
        var result = _db.SearchFilesByDate(new DateTime(2024, 6, 15)).ToList();
        Assert.Single(result);
        Assert.Equal("match.jpg", result[0].Path);
    }

    [Fact]
    public void SearchFilesByDate_Range_ReturnsFilesInRange()
    {
        InsertFileRow("in_range.jpg", datetime: "2024-06-15T10:00:00");
        InsertFileRow("out_of_range.jpg", datetime: "2024-06-20T10:00:00");
        var result = _db.SearchFilesByDate(new DateTime(2024, 6, 1), new DateTime(2024, 6, 18)).ToList();
        Assert.Single(result);
        Assert.Equal("in_range.jpg", result[0].Path);
    }

    [Fact]
    public void SearchFilesByTime_ReturnsFilesInTimeRange()
    {
        InsertFileRow("morning.jpg", datetime: "2024-01-01T08:30:00");
        InsertFileRow("evening.jpg", datetime: "2024-01-01T20:00:00");
        var result = _db.SearchFilesByTime(new TimeOnly(8, 0), new TimeOnly(12, 0)).ToList();
        Assert.Single(result);
        Assert.Equal("morning.jpg", result[0].Path);
    }

    [Fact]
    public void SearchFilesByAnnualDate_MonthAndDay_ReturnsMatchingFiles()
    {
        InsertFileRow("match.jpg", datetime: "2024-06-15");
        InsertFileRow("no_match.jpg", datetime: "2024-06-16");
        var result = _db.SearchFilesByAnnualDate(6, 15).ToList();
        Assert.Single(result);
        Assert.Equal("match.jpg", result[0].Path);
    }

    [Fact]
    public void SearchFilesByAnnualDate_Range_ReturnsFilesInRange()
    {
        InsertFileRow("in_range.jpg", datetime: "2024-06-15");
        InsertFileRow("out_of_range.jpg", datetime: "2024-08-20");
        var result = _db.SearchFilesByAnnualDate(6, 1, 7, 1).ToList();
        Assert.Single(result);
        Assert.Equal("in_range.jpg", result[0].Path);
    }

    [Fact]
    public void SearchFilesNearGpsPosition_ReturnsFilesWithinRadius()
    {
        InsertFileRow("near.jpg", position: "59.0 18.0");
        InsertFileRow("far.jpg", position: "0.0 0.0");
        var result = _db.SearchFilesNearGpsPosition(59.0, 18.0, 10000).ToList();
        Assert.Single(result);
        Assert.Equal("near.jpg", result[0].Path);
    }

    #endregion

    #region Persons

    [Fact]
    public void GetPersons_EmptyDb_ReturnsEmpty()
    {
        Assert.Empty(_db.GetPersons());
    }

    [Fact]
    public void InsertPerson_GetPersonById_ReturnsCorrectPerson()
    {
        var id = _db.InsertPerson(new PersonModel { Id = 0, ShortName = "JD", FullName = "John Doe", Sex = Sex.Male });
        var byId = _db.GetPersonById(id);
        Assert.Equal("JD", byId.ShortName);
        Assert.Equal("John Doe", byId.FullName);
        Assert.Equal(Sex.Male, byId.Sex);
    }

    [Fact]
    public void GetPersonCount_ReturnsCorrectCount()
    {
        Assert.Equal(0, _db.GetPersonCount());
        InsertPersonRow("A", "Alpha");
        InsertPersonRow("B", "Beta");
        Assert.Equal(2, _db.GetPersonCount());
    }

    [Fact]
    public void HasPersonId_ExistingId_ReturnsTrue()
    {
        var id = InsertPersonRow();
        Assert.True(_db.HasPersonId(id));
    }

    [Fact]
    public void HasPersonId_NonExistingId_ReturnsFalse()
    {
        Assert.False(_db.HasPersonId(999));
    }

    [Fact]
    public void UpdatePerson_UpdatesAllFields()
    {
        var id = InsertPersonRow("Old", "Old Name");
        var updated = new PersonModel { Id = id, ShortName = "New", FullName = "New Name", Description = "desc", Sex = Sex.Female };
        _db.UpdatePerson(updated);
        var result = _db.GetPersonById(id);
        Assert.Equal("New", result.ShortName);
        Assert.Equal("New Name", result.FullName);
        Assert.Equal("desc", result.Description);
        Assert.Equal(Sex.Female, result.Sex);
    }

    [Fact]
    public void DeletePerson_RemovesPerson()
    {
        var id = InsertPersonRow();
        _db.DeletePerson(id);
        Assert.Equal(0, _db.GetPersonCount());
    }

    #endregion

    #region Locations

    [Fact]
    public void GetLocations_EmptyDb_ReturnsEmpty()
    {
        Assert.Empty(_db.GetLocations());
    }

    [Fact]
    public void InsertLocation_GetLocationById_ReturnsCorrectLocation()
    {
        var id = _db.InsertLocation(new LocationModel { Id = 0, Name = "Home", Description = "My home" });
        var byId = _db.GetLocationById(id);
        Assert.Equal("Home", byId.Name);
        Assert.Equal("My home", byId.Description);
    }

    [Fact]
    public void GetLocationCount_ReturnsCorrectCount()
    {
        Assert.Equal(0, _db.GetLocationCount());
        InsertLocationRow("A");
        InsertLocationRow("B");
        Assert.Equal(2, _db.GetLocationCount());
    }

    [Fact]
    public void HasLocationId_ExistingId_ReturnsTrue()
    {
        var id = InsertLocationRow();
        Assert.True(_db.HasLocationId(id));
    }

    [Fact]
    public void HasLocationId_NonExistingId_ReturnsFalse()
    {
        Assert.False(_db.HasLocationId(999));
    }

    [Fact]
    public void UpdateLocation_UpdatesAllFields()
    {
        var id = InsertLocationRow("Old");
        var updated = new LocationModel { Id = id, Name = "New", Description = "new desc", Position = "10.0 20.0" };
        _db.UpdateLocation(updated);
        var result = _db.GetLocationById(id);
        Assert.Equal("New", result.Name);
        Assert.Equal("new desc", result.Description);
        Assert.Equal("10.0 20.0", result.Position);
    }

    [Fact]
    public void DeleteLocation_RemovesLocation()
    {
        var id = InsertLocationRow();
        _db.DeleteLocation(id);
        Assert.Equal(0, _db.GetLocationCount());
    }

    [Fact]
    public void SearchLocationsNearGpsPosition_ReturnsLocationsWithinRadius()
    {
        InsertLocationRow("Near", "59.0 18.0");
        InsertLocationRow("Far", "0.0 0.0");
        var result = _db.SearchLocationsNearGpsPosition(59.0, 18.0, 10000).ToList();
        Assert.Single(result);
        Assert.Equal("Near", result[0].Name);
    }

    #endregion

    #region Tags

    [Fact]
    public void GetTags_EmptyDb_ReturnsEmpty()
    {
        Assert.Empty(_db.GetTags());
    }

    [Fact]
    public void InsertTag_GetTagById_ReturnsCorrectTag()
    {
        var id = _db.InsertTag(new TagModel { Id = 0, Name = "Nature" });
        var byId = _db.GetTagById(id);
        Assert.Equal("Nature", byId.Name);
    }

    [Fact]
    public void GetTagCount_ReturnsCorrectCount()
    {
        Assert.Equal(0, _db.GetTagCount());
        InsertTagRow("A");
        InsertTagRow("B");
        Assert.Equal(2, _db.GetTagCount());
    }

    [Fact]
    public void HasTagId_ExistingId_ReturnsTrue()
    {
        var id = InsertTagRow();
        Assert.True(_db.HasTagId(id));
    }

    [Fact]
    public void HasTagId_NonExistingId_ReturnsFalse()
    {
        Assert.False(_db.HasTagId(999));
    }

    [Fact]
    public void UpdateTag_UpdatesName()
    {
        var id = InsertTagRow("Old");
        _db.UpdateTag(new TagModel { Id = id, Name = "New" });
        Assert.Equal("New", _db.GetTagById(id).Name);
    }

    [Fact]
    public void DeleteTag_RemovesTag()
    {
        var id = InsertTagRow();
        _db.DeleteTag(id);
        Assert.Equal(0, _db.GetTagCount());
    }

    #endregion

    #region File-Person Relations

    [Fact]
    public void InsertFilePerson_GetPersonsFromFile_ReturnsLinkedPersons()
    {
        var fileId = InsertFileRow();
        var personId = InsertPersonRow();
        _db.InsertFilePerson(fileId, personId);
        var persons = _db.GetPersonsFromFile(fileId).ToList();
        Assert.Single(persons);
        Assert.Equal(personId, persons[0].Id);
    }

    [Fact]
    public void GetPersonsFromFile_NoRelations_ReturnsEmpty()
    {
        var fileId = InsertFileRow();
        Assert.Empty(_db.GetPersonsFromFile(fileId));
    }

    [Fact]
    public void GetPersonCountsFromFiles_ReturnsCountPerFile()
    {
        var f1 = InsertFileRow("f1.jpg");
        var f2 = InsertFileRow("f2.jpg");
        var f3 = InsertFileRow("f3.jpg");
        var p1 = InsertPersonRow();
        var p2 = InsertPersonRow();
        _db.InsertFilePerson(f1, p1);
        _db.InsertFilePerson(f2, p1);
        _db.InsertFilePerson(f2, p2);

        var result = _db.GetPersonCountsFromFiles([f1, f2, f3]);

        Assert.Equal(1, result[f1]);
        Assert.Equal(2, result[f2]);
        Assert.False(result.ContainsKey(f3));
    }

    [Fact]
    public void GetPersonCountsFromFiles_EmptyInput_ReturnsEmptyDictionary()
    {
        Assert.Empty(_db.GetPersonCountsFromFiles([]));
    }

    [Fact]
    public void DeleteFilePerson_RemovesRelation()
    {
        var fileId = InsertFileRow();
        var personId = InsertPersonRow();
        _db.InsertFilePerson(fileId, personId);
        _db.DeleteFilePerson(fileId, personId);
        Assert.Empty(_db.GetPersonsFromFile(fileId));
    }

    [Fact]
    public void DeleteFile_CascadeDeletesFilePersons()
    {
        var fileId = InsertFileRow();
        var personId = InsertPersonRow();
        _db.InsertFilePerson(fileId, personId);
        _db.DeleteFile(fileId);
        Assert.Empty(_db.GetPersonsFromFile(fileId));
        Assert.True(_db.HasPersonId(personId));
    }

    [Fact]
    public void DeleteFile_CascadeDeletesFileLocations()
    {
        var fileId = InsertFileRow();
        var locationId = InsertLocationRow();
        _db.InsertFileLocation(fileId, locationId);
        _db.DeleteFile(fileId);
        Assert.Empty(_db.GetLocationsFromFile(fileId));
        Assert.True(_db.HasLocationId(locationId));
    }

    [Fact]
    public void DeleteFile_CascadeDeletesFileTags()
    {
        var fileId = InsertFileRow();
        var tagId = InsertTagRow();
        _db.InsertFileTag(fileId, tagId);
        _db.DeleteFile(fileId);
        Assert.Empty(_db.GetTagsFromFile(fileId));
        Assert.True(_db.HasTagId(tagId));
    }

    #endregion

    #region File-Location Relations

    [Fact]
    public void InsertFileLocation_GetLocationsFromFile_ReturnsLinkedLocations()
    {
        var fileId = InsertFileRow();
        var locationId = InsertLocationRow();
        _db.InsertFileLocation(fileId, locationId);
        var locations = _db.GetLocationsFromFile(fileId).ToList();
        Assert.Single(locations);
        Assert.Equal(locationId, locations[0].Id);
    }

    [Fact]
    public void GetLocationsFromFile_NoRelations_ReturnsEmpty()
    {
        var fileId = InsertFileRow();
        Assert.Empty(_db.GetLocationsFromFile(fileId));
    }

    [Fact]
    public void DeleteFileLocation_RemovesRelation()
    {
        var fileId = InsertFileRow();
        var locationId = InsertLocationRow();
        _db.InsertFileLocation(fileId, locationId);
        _db.DeleteFileLocation(fileId, locationId);
        Assert.Empty(_db.GetLocationsFromFile(fileId));
    }

    #endregion

    #region File-Tag Relations

    [Fact]
    public void InsertFileTag_GetTagsFromFile_ReturnsLinkedTags()
    {
        var fileId = InsertFileRow();
        var tagId = InsertTagRow();
        _db.InsertFileTag(fileId, tagId);
        var tags = _db.GetTagsFromFile(fileId).ToList();
        Assert.Single(tags);
        Assert.Equal(tagId, tags[0].Id);
    }

    [Fact]
    public void GetTagsFromFile_NoRelations_ReturnsEmpty()
    {
        var fileId = InsertFileRow();
        Assert.Empty(_db.GetTagsFromFile(fileId));
    }

    [Fact]
    public void DeleteFileTag_RemovesRelation()
    {
        var fileId = InsertFileRow();
        var tagId = InsertTagRow();
        _db.InsertFileTag(fileId, tagId);
        _db.DeleteFileTag(fileId, tagId);
        Assert.Empty(_db.GetTagsFromFile(fileId));
    }

    #endregion

    #region SearchFilesWithPersons / Locations / Tags

    [Fact]
    public void SearchFilesWithPersons_ReturnsFilesWithAnyOfThePersons()
    {
        var p1 = InsertPersonRow("A", "Alice");
        var p2 = InsertPersonRow("B", "Bob");
        var f1 = InsertFileRow("f1.jpg");
        var f2 = InsertFileRow("f2.jpg");
        var f3 = InsertFileRow("f3.jpg");
        _db.InsertFilePerson(f1, p1);
        _db.InsertFilePerson(f2, p2);

        var result = _db.SearchFilesWithPersons([p1, p2]).Select(x => x.Id).ToList();
        Assert.Equal(2, result.Count);
        Assert.Contains(f1, result);
        Assert.Contains(f2, result);
        Assert.DoesNotContain(f3, result);
    }

    [Fact]
    public void SearchFilesWithPersonGroup_ReturnsFilesWithAllPersons()
    {
        var p1 = InsertPersonRow("A", "Alice");
        var p2 = InsertPersonRow("B", "Bob");
        var f1 = InsertFileRow("f1.jpg"); // has both
        var f2 = InsertFileRow("f2.jpg"); // has only p1
        _db.InsertFilePerson(f1, p1);
        _db.InsertFilePerson(f1, p2);
        _db.InsertFilePerson(f2, p1);

        var result = _db.SearchFilesWithPersonGroup([p1, p2]).Select(x => x.Id).ToList();
        Assert.Single(result);
        Assert.Equal(f1, result[0]);
    }

    [Fact]
    public void SearchFilesWithPersonGroupOnly_ReturnsFilesWithExactlyThosePersons()
    {
        var p1 = InsertPersonRow("A", "Alice");
        var p2 = InsertPersonRow("B", "Bob");
        var p3 = InsertPersonRow("C", "Carol");
        var f1 = InsertFileRow("f1.jpg"); // has exactly p1+p2
        var f2 = InsertFileRow("f2.jpg"); // has p1+p2+p3
        _db.InsertFilePerson(f1, p1);
        _db.InsertFilePerson(f1, p2);
        _db.InsertFilePerson(f2, p1);
        _db.InsertFilePerson(f2, p2);
        _db.InsertFilePerson(f2, p3);

        var result = _db.SearchFilesWithPersonGroupOnly([p1, p2]).Select(x => x.Id).ToList();
        Assert.Single(result);
        Assert.Equal(f1, result[0]);
    }

    [Fact]
    public void SearchFilesWithoutPerson_ReturnsFilesNotLinkedToPerson()
    {
        var p1 = InsertPersonRow();
        var f1 = InsertFileRow("f1.jpg");
        var f2 = InsertFileRow("f2.jpg");
        _db.InsertFilePerson(f1, p1);

        var result = _db.SearchFilesWithoutPerson(p1).Select(x => x.Id).ToList();
        Assert.Single(result);
        Assert.Equal(f2, result[0]);
    }

    [Fact]
    public void SearchFilesWithLocations_ReturnsFilesWithAnyOfTheLocations()
    {
        var l1 = InsertLocationRow("Home");
        var l2 = InsertLocationRow("Work");
        var f1 = InsertFileRow("f1.jpg");
        var f2 = InsertFileRow("f2.jpg");
        var f3 = InsertFileRow("f3.jpg");
        _db.InsertFileLocation(f1, l1);
        _db.InsertFileLocation(f2, l2);

        var result = _db.SearchFilesWithLocations([l1, l2]).Select(x => x.Id).ToList();
        Assert.Equal(2, result.Count);
        Assert.Contains(f1, result);
        Assert.Contains(f2, result);
        Assert.DoesNotContain(f3, result);
    }

    [Fact]
    public void SearchFilesWithoutLocation_ReturnsFilesNotLinkedToLocation()
    {
        var l1 = InsertLocationRow();
        var f1 = InsertFileRow("f1.jpg");
        var f2 = InsertFileRow("f2.jpg");
        _db.InsertFileLocation(f1, l1);

        var result = _db.SearchFilesWithoutLocation(l1).Select(x => x.Id).ToList();
        Assert.Single(result);
        Assert.Equal(f2, result[0]);
    }

    [Fact]
    public void SearchFilesWithTags_ReturnsFilesWithAnyOfTheTags()
    {
        var t1 = InsertTagRow("Nature");
        var t2 = InsertTagRow("Travel");
        var f1 = InsertFileRow("f1.jpg");
        var f2 = InsertFileRow("f2.jpg");
        var f3 = InsertFileRow("f3.jpg");
        _db.InsertFileTag(f1, t1);
        _db.InsertFileTag(f2, t2);

        var result = _db.SearchFilesWithTags([t1, t2]).Select(x => x.Id).ToList();
        Assert.Equal(2, result.Count);
        Assert.Contains(f1, result);
        Assert.Contains(f2, result);
        Assert.DoesNotContain(f3, result);
    }

    [Fact]
    public void SearchFilesWithTagGroup_ReturnsFilesWithAllTags()
    {
        var t1 = InsertTagRow("Nature");
        var t2 = InsertTagRow("Travel");
        var f1 = InsertFileRow("f1.jpg"); // has both
        var f2 = InsertFileRow("f2.jpg"); // has only t1
        _db.InsertFileTag(f1, t1);
        _db.InsertFileTag(f1, t2);
        _db.InsertFileTag(f2, t1);

        var result = _db.SearchFilesWithTagGroup([t1, t2]).Select(x => x.Id).ToList();
        Assert.Single(result);
        Assert.Equal(f1, result[0]);
    }

    [Fact]
    public void SearchFilesWithTagGroupOnly_ReturnsFilesWithExactlyThoseTags()
    {
        var t1 = InsertTagRow("Nature");
        var t2 = InsertTagRow("Travel");
        var t3 = InsertTagRow("Food");
        var f1 = InsertFileRow("f1.jpg"); // exactly t1+t2
        var f2 = InsertFileRow("f2.jpg"); // t1+t2+t3
        _db.InsertFileTag(f1, t1);
        _db.InsertFileTag(f1, t2);
        _db.InsertFileTag(f2, t1);
        _db.InsertFileTag(f2, t2);
        _db.InsertFileTag(f2, t3);

        var result = _db.SearchFilesWithTagGroupOnly([t1, t2]).Select(x => x.Id).ToList();
        Assert.Single(result);
        Assert.Equal(f1, result[0]);
    }

    [Fact]
    public void SearchFilesWithoutTag_ReturnsFilesNotLinkedToTag()
    {
        var t1 = InsertTagRow();
        var f1 = InsertFileRow("f1.jpg");
        var f2 = InsertFileRow("f2.jpg");
        _db.InsertFileTag(f1, t1);

        var result = _db.SearchFilesWithoutTag(t1).Select(x => x.Id).ToList();
        Assert.Single(result);
        Assert.Equal(f2, result[0]);
    }

    #endregion

    #region SearchFilesByNumPersons

    [Fact]
    public void SearchFilesByNumPersons_ZeroPersons_ReturnsFilesWithNoPerson()
    {
        var p1 = InsertPersonRow();
        var f1 = InsertFileRow("f1.jpg");
        var f2 = InsertFileRow("f2.jpg");
        _db.InsertFilePerson(f1, p1);

        var result = _db.SearchFilesByNumPersons(0..0).Select(x => x.Id).ToList();
        Assert.Single(result);
        Assert.Equal(f2, result[0]);
    }

    [Fact]
    public void SearchFilesByNumPersons_Range_ReturnsFilesWithPersonCountInRange()
    {
        var p1 = InsertPersonRow("A", "Alice");
        var p2 = InsertPersonRow("B", "Bob");
        var f1 = InsertFileRow("f1.jpg"); // 1 person
        var f2 = InsertFileRow("f2.jpg"); // 2 persons
        _db.InsertFilePerson(f1, p1);
        _db.InsertFilePerson(f2, p1);
        _db.InsertFilePerson(f2, p2);

        var result = _db.SearchFilesByNumPersons(1..2).Select(x => x.Id).ToList();
        Assert.Equal(2, result.Count);
        Assert.Contains(f1, result);
        Assert.Contains(f2, result);
    }

    #endregion

    #region SearchFilesBySex

    [Fact]
    public void SearchFilesBySex_ReturnsFilesLinkedToPersonsOfThatSex()
    {
        var male = InsertPersonRow("M", "Male Person", Sex.Male);
        var female = InsertPersonRow("F", "Female Person", Sex.Female);
        var f1 = InsertFileRow("f1.jpg");
        var f2 = InsertFileRow("f2.jpg");
        _db.InsertFilePerson(f1, male);
        _db.InsertFilePerson(f2, female);

        var result = _db.SearchFilesBySex(Sex.Male).Select(x => x.Id).ToList();
        Assert.Single(result);
        Assert.Equal(f1, result[0]);
    }

    #endregion
}
