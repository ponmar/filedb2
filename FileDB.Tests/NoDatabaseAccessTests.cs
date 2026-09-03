using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests;

public class NoDatabaseAccessTests
{
    private readonly NoDatabaseAccess sut = new();

    // --- Properties ---

    [Fact]
    public void Database_DefaultIsEmpty()
    {
        Assert.Equal(string.Empty, sut.Database);
    }

    [Fact]
    public void Database_CanBeSet()
    {
        sut.Database = "mydb";
        Assert.Equal("mydb", sut.Database);
    }

    [Fact]
    public void NeedsMigration_IsFalse()
    {
        Assert.False(sut.NeedsMigration);
    }

    [Fact]
    public void IsTooNew_IsFalse()
    {
        Assert.False(sut.IsTooNew);
    }

    // --- Migration ---

    [Fact]
    public void Migrate_ReturnsEmptyList()
    {
        Assert.Empty(sut.Migrate());
    }

    // --- File queries ---

    [Fact]
    public void GetFiles_ReturnsEmpty()
    {
        Assert.Empty(sut.GetFiles());
    }

    [Fact]
    public void GetDirectories_ReturnsEmpty()
    {
        Assert.Empty(sut.GetDirectories());
    }

    [Fact]
    public void GetFileCount_ReturnsZero()
    {
        Assert.Equal(0, sut.GetFileCount());
    }

    [Fact]
    public void SearchFilesFromIds_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesFromIds([1, 2]));
    }

    [Fact]
    public void SearchFilesExceptIds_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesExceptIds([1]));
    }

    [Fact]
    public void SearchFiles_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFiles("query", caseSensitive: false));
    }

    [Fact]
    public void SearchFilesBySex_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesBySex(Sex.Male));
    }

    [Fact]
    public void SearchFilesByPath_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesByPath("some/path"));
    }

    [Fact]
    public void SearchFilesByExtension_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesByExtension(".jpg"));
    }

    [Fact]
    public void SearchFilesRandom_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesRandom(5));
    }

    [Fact]
    public void SearchFilesNearGpsPosition_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesNearGpsPosition(59.0, 18.0, 1000));
    }

    [Fact]
    public void GetFileById_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(() => sut.GetFileById(1));
    }

    [Fact]
    public void GetFileByPath_ReturnsNull()
    {
        Assert.Null(sut.GetFileByPath("a/b.jpg"));
    }

    [Fact]
    public void SearchFilesByTime_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesByTime(new TimeOnly(8, 0), new TimeOnly(18, 0)));
    }

    [Fact]
    public void SearchFilesByDate_SingleDate_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesByDate(DateTime.Today));
    }

    [Fact]
    public void SearchFilesByDate_DateRange_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesByDate(DateTime.Today.AddDays(-7), DateTime.Today));
    }

    [Fact]
    public void SearchFilesBySeason_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesBySeason(Season.Summer));
    }
    [Fact]
    public void SearchFilesByAnnualDate_MonthDay_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesByAnnualDate(6, 15));
    }

    [Fact]
    public void SearchFilesByAnnualDate_Range_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesByAnnualDate(6, 1, 8, 31));
    }

    [Fact]
    public void SearchFilesWithoutDate_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesWithoutDate());
    }

    [Fact]
    public void SearchFilesByNumPersons_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesByNumPersons(1..3));
    }

    [Fact]
    public void SearchFilesWithPersons_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesWithPersons([1, 2]));
    }

    [Fact]
    public void SearchFilesWithPersonGroup_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesWithPersonGroup([1]));
    }

    [Fact]
    public void SearchFilesWithPersonGroupOnly_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesWithPersonGroupOnly([1]));
    }

    [Fact]
    public void SearchFilesWithoutPerson_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesWithoutPerson(1));
    }

    [Fact]
    public void SearchFilesWithLocations_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesWithLocations([1]));
    }

    [Fact]
    public void SearchFilesWithoutLocation_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesWithoutLocation(1));
    }

    [Fact]
    public void SearchFilesWithTags_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesWithTags([1, 2]));
    }

    [Fact]
    public void SearchFilesWithTagGroup_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesWithTagGroup([1]));
    }

    [Fact]
    public void SearchFilesWithTagGroupOnly_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesWithTagGroupOnly([1]));
    }

    [Fact]
    public void SearchFilesWithoutTag_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesWithoutTag(1));
    }

    [Fact]
    public void SearchFilesWithMissingData_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchFilesWithMissingData());
    }

    // --- File mutations (void / return 0) ---

    [Fact]
    public void InsertFile_ReturnsZero()
    {
        Assert.Equal(0, sut.InsertFile("path/file.jpg", null, null!, findMetadata: false));
    }

    [Fact]
    public void UpdateFileFromMetaData_DoesNotThrow()
    {
        sut.UpdateFileFromMetaData(1, null!);
    }

    [Fact]
    public void UpdateFileDescription_DoesNotThrow()
    {
        sut.UpdateFileDescription(1, "desc");
    }

    [Fact]
    public void UpdateFileDatetime_DoesNotThrow()
    {
        sut.UpdateFileDatetime(1, "2000-01-01");
    }

    [Fact]
    public void UpdateFileOrientation_DoesNotThrow()
    {
        sut.UpdateFileOrientation(1, 90);
    }

    [Fact]
    public void DeleteFile_DoesNotThrow()
    {
        sut.DeleteFile(1);
    }

    // --- File-Person associations ---

    [Fact]
    public void InsertFilePerson_DoesNotThrow()
    {
        sut.InsertFilePerson(1, 2);
    }

    [Fact]
    public void DeleteFilePerson_DoesNotThrow()
    {
        sut.DeleteFilePerson(1, 2);
    }

    [Fact]
    public void UpdateFilePersonBoundingBox_DoesNotThrow()
    {
        sut.UpdateFilePersonBoundingBox(1, 2, null);
    }

    [Fact]
    public void GetFilePersonBoundingBox_ReturnsNull()
    {
        Assert.Null(sut.GetFilePersonBoundingBox(1, 2));
    }

    [Fact]
    public void GetFilePersonBoundingBoxes_ReturnsEmpty()
    {
        Assert.Empty(sut.GetFilePersonBoundingBoxes(1));
    }

    // --- File-Location associations ---

    [Fact]
    public void InsertFileLocation_DoesNotThrow()
    {
        sut.InsertFileLocation(1, 2);
    }

    [Fact]
    public void DeleteFileLocation_DoesNotThrow()
    {
        sut.DeleteFileLocation(1, 2);
    }

    // --- File-Tag associations ---

    [Fact]
    public void InsertFileTag_DoesNotThrow()
    {
        sut.InsertFileTag(1, 2);
    }

    [Fact]
    public void DeleteFileTag_DoesNotThrow()
    {
        sut.DeleteFileTag(1, 2);
    }

    // --- Person queries ---

    [Fact]
    public void GetPersons_ReturnsEmpty()
    {
        Assert.Empty(sut.GetPersons());
    }

    [Fact]
    public void GetPersonsFromFile_ReturnsEmpty()
    {
        Assert.Empty(sut.GetPersonsFromFile(1));
    }

    [Fact]
    public void GetPersonCountsFromFiles_ReturnsEmptyDictionary()
    {
        Assert.Empty(sut.GetPersonCountsFromFiles([1, 2]));
    }

    [Fact]
    public void GetPersonCount_ReturnsZero()
    {
        Assert.Equal(0, sut.GetPersonCount());
    }

    [Fact]
    public void GetPersonById_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(() => sut.GetPersonById(1));
    }

    [Fact]
    public void HasPersonId_ReturnsFalse()
    {
        Assert.False(sut.HasPersonId(1));
    }

    [Fact]
    public void InsertPerson_ReturnsZero()
    {
        Assert.Equal(0, sut.InsertPerson(new PersonModel { Id = 0, ShortName = "A", FullName = "A B" }));
    }

    [Fact]
    public void UpdatePerson_DoesNotThrow()
    {
        sut.UpdatePerson(new PersonModel { Id = 1, ShortName = "A", FullName = "A B" });
    }

    [Fact]
    public void DeletePerson_DoesNotThrow()
    {
        sut.DeletePerson(1);
    }

    // --- Location queries ---

    [Fact]
    public void GetLocations_ReturnsEmpty()
    {
        Assert.Empty(sut.GetLocations());
    }

    [Fact]
    public void GetLocationsFromFile_ReturnsEmpty()
    {
        Assert.Empty(sut.GetLocationsFromFile(1));
    }

    [Fact]
    public void SearchLocationsNearGpsPosition_ReturnsEmpty()
    {
        Assert.Empty(sut.SearchLocationsNearGpsPosition(59.0, 18.0, 500));
    }

    [Fact]
    public void GetLocationCount_ReturnsZero()
    {
        Assert.Equal(0, sut.GetLocationCount());
    }

    [Fact]
    public void GetLocationById_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(() => sut.GetLocationById(1));
    }

    [Fact]
    public void HasLocationId_ReturnsFalse()
    {
        Assert.False(sut.HasLocationId(1));
    }

    [Fact]
    public void InsertLocation_ReturnsZero()
    {
        Assert.Equal(0, sut.InsertLocation(new LocationModel { Id = 0, Name = "Place" }));
    }

    [Fact]
    public void UpdateLocation_DoesNotThrow()
    {
        sut.UpdateLocation(new LocationModel { Id = 1, Name = "Place" });
    }

    [Fact]
    public void DeleteLocation_DoesNotThrow()
    {
        sut.DeleteLocation(1);
    }

    // --- Tag queries ---

    [Fact]
    public void GetTags_ReturnsEmpty()
    {
        Assert.Empty(sut.GetTags());
    }

    [Fact]
    public void GetTagsFromFile_ReturnsEmpty()
    {
        Assert.Empty(sut.GetTagsFromFile(1));
    }

    [Fact]
    public void GetTagCount_ReturnsZero()
    {
        Assert.Equal(0, sut.GetTagCount());
    }

    [Fact]
    public void GetTagById_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(() => sut.GetTagById(1));
    }

    [Fact]
    public void HasTagId_ReturnsFalse()
    {
        Assert.False(sut.HasTagId(1));
    }

    [Fact]
    public void InsertTag_ReturnsZero()
    {
        Assert.Equal(0, sut.InsertTag(new TagModel { Id = 0, Name = "Tag" }));
    }

    [Fact]
    public void UpdateTag_DoesNotThrow()
    {
        sut.UpdateTag(new TagModel { Id = 1, Name = "Tag" });
    }

    [Fact]
    public void DeleteTag_DoesNotThrow()
    {
        sut.DeleteTag(1);
    }
}
