using FakeItEasy;
using FileDB.Model;
using FileDBInterface.Model;
using System.Globalization;
using Xunit;

namespace FileDB.Tests;

public class FileTextOverlayCreatorTests
{
    private static PersonModel MakePerson(string fullName, string? dob = null, string? deceased = null, string? description = null) =>
        new() { Id = 1, ShortName = fullName, FullName = fullName, DateOfBirth = dob, Deceased = deceased, Description = description };

    private static LocationModel MakeLocation(string name, string? description = null, string? position = null) =>
        new() { Id = 1, Name = name, Description = description, Position = position };

    private static TagModel MakeTag(string name) => new() { Id = 1, Name = name };

    private static IConfigProvider MakeConfigProvider(string locationLink = "")
    {
        var fakeConfigProvider = A.Fake<IConfigProvider>();
        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder { LocationLink = locationLink }.Build());
        return fakeConfigProvider;
    }

    // --- GetPersonText ---

    [Fact]
    public void GetPersonText_NoDob_ReturnsFullNameOnly()
    {
        var person = MakePerson("John Smith");
        Assert.Equal("John Smith", FileTextOverlayCreator.GetPersonText(person, "2020-06-15"));
    }

    [Fact]
    public void GetPersonText_WithDobAndFileDateTime_ReturnsNameWithAge()
    {
        var person = MakePerson("John Smith", dob: "1990-01-01");
        Assert.Equal("John Smith (30)", FileTextOverlayCreator.GetPersonText(person, "2020-06-15"));
    }

    [Fact]
    public void GetPersonText_WithDobButNullFileDateTime_ReturnsNameOnly()
    {
        var person = MakePerson("John Smith", dob: "1990-01-01");
        Assert.Equal("John Smith", FileTextOverlayCreator.GetPersonText(person, null));
    }

    [Fact]
    public void GetPersonText_WithDobButInvalidFileDateTime_ReturnsNameOnly()
    {
        var person = MakePerson("John Smith", dob: "1990-01-01");
        Assert.Equal("John Smith", FileTextOverlayCreator.GetPersonText(person, "not-a-date"));
    }

    // --- GetPersonsText ---

    [Fact]
    public void GetPersonsText_EmptyList_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, FileTextOverlayCreator.GetPersonsText((string?)null, [], ", "));
    }

    [Fact]
    public void GetPersonsText_SinglePerson_ReturnsPersonText()
    {
        var person = MakePerson("Alice");
        Assert.Equal("Alice", FileTextOverlayCreator.GetPersonsText((string?)null, [person], ", "));
    }

    [Fact]
    public void GetPersonsText_MultiplePersons_ReturnsSortedAndJoined()
    {
        var persons = new[] { MakePerson("Zelda"), MakePerson("Alice"), MakePerson("Bob") };
        Assert.Equal("Alice, Bob, Zelda", FileTextOverlayCreator.GetPersonsText((string?)null, persons, ", "));
    }

    // --- GetPersonsTexts ---

    [Fact]
    public void GetPersonsTexts_MultiplePersons_ReturnsSortedAlphabetically()
    {
        var persons = new[] { MakePerson("Zelda"), MakePerson("Alice"), MakePerson("Bob") };
        var result = FileTextOverlayCreator.GetPersonsTexts(null, persons).ToList();
        Assert.Equal(["Alice", "Bob", "Zelda"], result);
    }

    // --- GetShortPersonText ---

    [Fact]
    public void GetShortPersonText_NameBelowMaxLength_FullNameReturned()
    {
        var person = MakePerson("Alice");
        Assert.Equal("Alice", FileTextOverlayCreator.GetShortPersonText(person, null, 10));
    }

    [Fact]
    public void GetShortPersonText_NameExceedsMaxLength_NameIsTruncated()
    {
        var person = MakePerson("Alexander Hamilton");
        var result = FileTextOverlayCreator.GetShortPersonText(person, null, 5);
        Assert.Equal("Alexa...", result);
    }

    [Fact]
    public void GetShortPersonText_AlivePerson_AgeIsIncluded()
    {
        var person = MakePerson("Alice", dob: "1990-01-01");
        var result = FileTextOverlayCreator.GetShortPersonText(person, new DateTime(2020, 6, 15), 50);
        Assert.Equal("Alice (30)", result);
    }

    [Fact]
    public void GetShortPersonText_DeceasedPerson_AgeIsNotIncluded()
    {
        var person = MakePerson("Alice", dob: "1990-01-01", deceased: "2010-01-01");
        var result = FileTextOverlayCreator.GetShortPersonText(person, new DateTime(2020, 6, 15), 50);
        Assert.Equal("Alice", result);
    }

    // --- GetLocationsText ---

    [Fact]
    public void GetLocationsText_EmptyList_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, FileTextOverlayCreator.GetLocationsText([], ", "));
    }

    [Fact]
    public void GetLocationsText_SingleLocation_ReturnsName()
    {
        Assert.Equal("Paris", FileTextOverlayCreator.GetLocationsText([MakeLocation("Paris")], ", "));
    }

    [Fact]
    public void GetLocationsText_MultipleLocations_ReturnsSortedAndJoined()
    {
        var locations = new[] { MakeLocation("Rome"), MakeLocation("Athens"), MakeLocation("Paris") };
        Assert.Equal("Athens, Paris, Rome", FileTextOverlayCreator.GetLocationsText(locations, ", "));
    }

    // --- GetLocationsTexts ---

    [Fact]
    public void GetLocationsTexts_MultipleLocations_ReturnsSortedAlphabetically()
    {
        var locations = new[] { MakeLocation("Rome"), MakeLocation("Athens") };
        Assert.Equal(["Athens", "Rome"], FileTextOverlayCreator.GetLocationsTexts(locations).ToList());
    }

    // --- GetLocations ---

    [Fact]
    public void GetLocations_OrderedByName()
    {
        var locations = new[] { MakeLocation("Rome"), MakeLocation("Athens") };
        var result = FileTextOverlayCreator.GetLocations(MakeConfigProvider(), locations).ToList();
        Assert.Equal("Athens", result[0].Name);
        Assert.Equal("Rome", result[1].Name);
    }

    [Fact]
    public void GetLocations_LocationWithoutPosition_UriIsNull()
    {
        var result = FileTextOverlayCreator.GetLocations(MakeConfigProvider("https://maps/LAT,LON"), [MakeLocation("Park")]).Single();
        Assert.Null(result.MapUrl);
    }

    [Fact]
    public void GetLocations_LocationWithPositionAndLinkConfig_UriIsSet()
    {
        var location = MakeLocation("Park", position: "59.3 18.1");
        var result = FileTextOverlayCreator.GetLocations(MakeConfigProvider("https://maps/LAT,LON"), [location]).Single();
        Assert.Equal("https://maps/59.3,18.1", result.MapUrl);
    }

    // --- GetTagsText ---

    [Fact]
    public void GetTagsText_EmptyList_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, FileTextOverlayCreator.GetTagsText([], ", "));
    }

    [Fact]
    public void GetTagsText_SingleTag_ReturnsName()
    {
        Assert.Equal("Nature", FileTextOverlayCreator.GetTagsText([MakeTag("Nature")], ", "));
    }

    [Fact]
    public void GetTagsText_MultipleTags_ReturnsSortedAndJoined()
    {
        var tags = new[] { MakeTag("Zoo"), MakeTag("Art"), MakeTag("Nature") };
        Assert.Equal("Art, Nature, Zoo", FileTextOverlayCreator.GetTagsText(tags, ", "));
    }

    // --- GetTagsTexts ---

    [Fact]
    public void GetTagsTexts_MultipleTags_ReturnsSortedAlphabetically()
    {
        var tags = new[] { MakeTag("Zoo"), MakeTag("Art") };
        Assert.Equal(["Art", "Zoo"], FileTextOverlayCreator.GetTagsTexts(tags).ToList());
    }

    // --- GetFileDateTimeText ---

    [Fact]
    public void GetFileDateTimeText_NullDatetime_ReturnsNull()
    {
        var file = new FileModel { Id = 1, Path = "f", Datetime = null };
        Assert.Null(FileTextOverlayCreator.GetFileDateTimeText(file));
    }

    [Fact]
    public void GetFileDateTimeText_InvalidDatetime_ReturnsNull()
    {
        var file = new FileModel { Id = 1, Path = "f", Datetime = "not-a-date" };
        Assert.Null(FileTextOverlayCreator.GetFileDateTimeText(file));
    }

    [Fact]
    public void GetFileDateTimeText_DateOnlyFormat_ContainsDateString()
    {
        var file = new FileModel { Id = 1, Path = "f", Datetime = "2000-01-01" };
        var result = FileTextOverlayCreator.GetFileDateTimeText(file);
        Assert.NotNull(result);
        Assert.StartsWith("2000-01-01", result);
    }

    [Fact]
    public void GetFileDateTimeText_DateTimeFormat_ContainsFormattedDateTime()
    {
        var file = new FileModel { Id = 1, Path = "f", Datetime = "2000-06-15T10:30:00" };
        var result = FileTextOverlayCreator.GetFileDateTimeText(file);
        Assert.NotNull(result);
        Assert.StartsWith("2000-06-15 10:30", result);
    }

    // --- GetShortPositionText ---

    [Fact]
    public void GetShortPositionText_NullPosition_ReturnsNull()
    {
        var file = new FileModel { Id = 1, Path = "f", Position = null };
        Assert.Null(FileTextOverlayCreator.GetShortPositionText(file));
    }

    [Fact]
    public void GetShortPositionText_ValidPosition_ReturnsShortString()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        try
        {
            Utils.SetInvariantCulture();

            var file = new FileModel { Id = 1, Path = "f", Position = "59.3 18.1" };
            var result = FileTextOverlayCreator.GetShortPositionText(file);
            Assert.NotNull(result);
            Assert.Contains("59.3", result);
            Assert.Contains("18.1", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.DefaultThreadCurrentCulture = originalDefaultCulture;
        }
    }

    [Fact]
    public void GetShortPositionText_ValidPosition_SwedishUICulture_ReturnsShortString()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        var originalDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        try
        {
            Utils.SetInvariantCulture();
            Utils.SetUICulture("se");

            var file = new FileModel { Id = 1, Path = "f", Position = "59.3 18.1" };
            var result = FileTextOverlayCreator.GetShortPositionText(file);
            Assert.NotNull(result);
            Assert.Contains("59.3", result);
            Assert.Contains("18.1", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
            CultureInfo.DefaultThreadCurrentCulture = originalDefaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = originalDefaultUiCulture;
        }
    }

    // --- GetPositionUri (FileModel) ---

    [Fact]
    public void GetPositionUri_File_NullPosition_ReturnsNull()
    {
        var file = new FileModel { Id = 1, Path = "f", Position = null };
        Assert.Null(FileTextOverlayCreator.GetPositionUri(MakeConfigProvider("https://maps/LAT,LON"), file));
    }

    [Fact]
    public void GetPositionUri_File_NoLinkConfig_ReturnsNull()
    {
        var file = new FileModel { Id = 1, Path = "f", Position = "59.3 18.1" };
        Assert.Null(FileTextOverlayCreator.GetPositionUri(MakeConfigProvider(), file));
    }

    [Fact]
    public void GetPositionUri_File_WithPositionAndLinkConfig_ReturnsUri()
    {
        var file = new FileModel { Id = 1, Path = "f", Position = "59.3 18.1" };
        var result = FileTextOverlayCreator.GetPositionUri(MakeConfigProvider("https://maps/LAT,LON"), file);
        Assert.Equal("https://maps/59.3,18.1", result);
    }

    // --- GetPositionUri (LocationModel) ---

    [Fact]
    public void GetPositionUri_Location_NullPosition_ReturnsNull()
    {
        Assert.Null(FileTextOverlayCreator.GetPositionUri(MakeConfigProvider("https://maps/LAT,LON"), MakeLocation("Park")));
    }

    [Fact]
    public void GetPositionUri_Location_WithPositionAndLinkConfig_ReturnsUri()
    {
        var location = MakeLocation("Park", position: "59.3 18.1");
        var result = FileTextOverlayCreator.GetPositionUri(MakeConfigProvider("https://maps/LAT,LON"), location);
        Assert.Equal("https://maps/59.3,18.1", result);
    }

    // --- GetPersonDetailsText ---

    [Fact]
    public void GetPersonDetailsText_NoDobNoDescription_ReturnsNameOnly()
    {
        var person = MakePerson("John Smith");
        Assert.Equal("John Smith", FileTextOverlayCreator.GetPersonDetailsText(person, null));
    }

    [Fact]
    public void GetPersonDetailsText_WithDob_IncludesDobOnNewLine()
    {
        var person = MakePerson("John Smith", dob: "1990-01-01");
        Assert.Equal("John Smith\n1990-01-01", FileTextOverlayCreator.GetPersonDetailsText(person, null));
    }

    [Fact]
    public void GetPersonDetailsText_WithDobAndDeceased_IncludesDobDeceasedOnNewLine()
    {
        var person = MakePerson("John Smith", dob: "1990-01-01", deceased: "2020-05-01");
        Assert.Equal("John Smith\n1990-01-01 - 2020-05-01", FileTextOverlayCreator.GetPersonDetailsText(person, null));
    }

    [Fact]
    public void GetPersonDetailsText_WithDescription_IncludesDescriptionOnNewLine()
    {
        var person = MakePerson("John Smith", description: "Photographer");
        Assert.Equal("John Smith\nPhotographer", FileTextOverlayCreator.GetPersonDetailsText(person, null));
    }

    [Fact]
    public void GetPersonDetailsText_WithDobAndDescription_IncludesBothOnNewLines()
    {
        var person = MakePerson("John Smith", dob: "1990-01-01", description: "Photographer");
        Assert.Equal("John Smith\n1990-01-01\nPhotographer", FileTextOverlayCreator.GetPersonDetailsText(person, null));
    }

    [Fact]
    public void GetPersonDetailsText_WithDobDeceasedAndDescription_IncludesAllOnNewLines()
    {
        var person = MakePerson("John Smith", dob: "1990-01-01", deceased: "2020-05-01", description: "Photographer");
        Assert.Equal("John Smith\n1990-01-01 - 2020-05-01\nPhotographer", FileTextOverlayCreator.GetPersonDetailsText(person, null));
    }

    [Fact]
    public void GetPersonDetailsText_WithDobAndDateTime_IncludesAge()
    {
        var person = MakePerson("John Smith", dob: "1990-01-01");
        var result = FileTextOverlayCreator.GetPersonDetailsText(person, new DateTime(2020, 6, 15));
        Assert.Equal("John Smith (30)\n1990-01-01", result);
    }

    // --- GetLocationDetailsText ---

    [Fact]
    public void GetLocationDetailsText_NoDescription_ReturnsNameOnly()
    {
        Assert.Equal("Paris", FileTextOverlayCreator.GetLocationDetailsText(MakeLocation("Paris")));
    }

    [Fact]
    public void GetLocationDetailsText_WithDescription_IncludesDescriptionOnNewLine()
    {
        Assert.Equal("Paris:\nCapital of France", FileTextOverlayCreator.GetLocationDetailsText(MakeLocation("Paris", description: "Capital of France")));
    }
}
