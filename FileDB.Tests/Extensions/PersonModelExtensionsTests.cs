using FileDB.Extensions;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.Extensions;

public class PersonModelExtensionsTests
{
    [Fact]
    public void MatchesTextFilter_WithEmptyFilter_ReturnsTrue()
    {
        // Arrange
        var person = new PersonModel { Id = 1, ShortName = "John", FullName = "John Doe", Sex = Sex.Male };
        var filter = "";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithNullFilter_ReturnsTrue()
    {
        // Arrange
        var person = new PersonModel { Id = 1, ShortName = "Jane", FullName = "Jane Smith", Sex = Sex.Female };
        string? filter = null;

        // Act
        var result = person.MatchesTextFilter(filter!);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithMatchingShortName_ReturnsTrue()
    {
        // Arrange
        var person = new PersonModel { Id = 1, ShortName = "Bob", FullName = "Robert Johnson", Sex = Sex.Male };
        var filter = "Bob";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithPartialShortNameMatch_ReturnsTrue()
    {
        // Arrange
        var person = new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Williams", Sex = Sex.Female };
        var filter = "Ali";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithCaseInsensitiveShortNameMatch_ReturnsTrue()
    {
        // Arrange
        var person = new PersonModel { Id = 1, ShortName = "Charlie", FullName = "Charles Brown", Sex = Sex.Male };
        var filter = "charlie";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithUpperCaseFilter_ReturnsTrue()
    {
        // Arrange
        var person = new PersonModel { Id = 1, ShortName = "David", FullName = "David Miller", Sex = Sex.Male };
        var filter = "DAVID";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithMatchingFullName_ReturnsTrue()
    {
        // Arrange
        var person = new PersonModel { Id = 1, ShortName = "Eve", FullName = "Evelyn Anderson", Sex = Sex.Female };
        var filter = "Evelyn Anderson";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithPartialFullNameMatch_ReturnsTrue()
    {
        // Arrange
        var person = new PersonModel { Id = 1, ShortName = "Frank", FullName = "Franklin Roosevelt", Sex = Sex.Male };
        var filter = "Roosevelt";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithCaseInsensitiveFullNameMatch_ReturnsTrue()
    {
        // Arrange
        var person = new PersonModel { Id = 1, ShortName = "Grace", FullName = "Grace Hopper", Sex = Sex.Female };
        var filter = "hopper";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithMatchingDescription_ReturnsTrue()
    {
        // Arrange
        var person = new PersonModel 
        { 
            Id = 1, 
            ShortName = "Henry", 
            FullName = "Henry Ford", 
            Description = "American industrialist",
            Sex = Sex.Male 
        };
        var filter = "industrialist";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithPartialDescriptionMatch_ReturnsTrue()
    {
        // Arrange
        var person = new PersonModel 
        { 
            Id = 1, 
            ShortName = "Isaac", 
            FullName = "Isaac Newton", 
            Description = "English mathematician and physicist",
            Sex = Sex.Male 
        };
        var filter = "mathematician";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithCaseInsensitiveDescriptionMatch_ReturnsTrue()
    {
        // Arrange
        var person = new PersonModel 
        { 
            Id = 1, 
            ShortName = "Julia", 
            FullName = "Julia Roberts", 
            Description = "American actress",
            Sex = Sex.Female 
        };
        var filter = "ACTRESS";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithNullDescription_OnlyChecksNames()
    {
        // Arrange
        var person = new PersonModel 
        { 
            Id = 1, 
            ShortName = "Kate", 
            FullName = "Katherine Johnson", 
            Description = null,
            Sex = Sex.Female 
        };
        var filter = "Kate";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithNullDescriptionAndNoNameMatch_ReturnsFalse()
    {
        // Arrange
        var person = new PersonModel 
        { 
            Id = 1, 
            ShortName = "Leo", 
            FullName = "Leonardo da Vinci", 
            Description = null,
            Sex = Sex.Male 
        };
        var filter = "artist";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchesTextFilter_WithNoMatch_ReturnsFalse()
    {
        // Arrange
        var person = new PersonModel 
        { 
            Id = 1, 
            ShortName = "Marie", 
            FullName = "Marie Curie", 
            Description = "Polish physicist",
            Sex = Sex.Female 
        };
        var filter = "Einstein";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchesTextFilter_WithWhitespaceFilter_ReturnsFalse()
    {
        // Arrange
        var person = new PersonModel { Id = 1, ShortName = "Neil", FullName = "Neil Armstrong", Sex = Sex.Male };
        var filter = "   ";

        // Act
        var result = person.MatchesTextFilter(filter);

        // Assert
        Assert.False(result);
    }
}
