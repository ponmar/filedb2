using FileDB.Extensions;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.Extensions;

public class LocationModelExtensionsTests
{
    [Fact]
    public void MatchesTextFilter_WithEmptyFilter_ReturnsTrue()
    {
        // Arrange
        var location = new LocationModel { Id = 1, Name = "New York" };
        var filter = "";

        // Act
        var result = location.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithNullFilter_ReturnsTrue()
    {
        // Arrange
        var location = new LocationModel { Id = 1, Name = "Paris" };
        string? filter = null;

        // Act
        var result = location.MatchesTextFilter(filter!);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithMatchingName_ReturnsTrue()
    {
        // Arrange
        var location = new LocationModel { Id = 1, Name = "London" };
        var filter = "London";

        // Act
        var result = location.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithPartialNameMatch_ReturnsTrue()
    {
        // Arrange
        var location = new LocationModel { Id = 1, Name = "San Francisco" };
        var filter = "Fran";

        // Act
        var result = location.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithCaseInsensitiveMatch_ReturnsTrue()
    {
        // Arrange
        var location = new LocationModel { Id = 1, Name = "Tokyo" };
        var filter = "tokyo";

        // Act
        var result = location.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithUpperCaseFilter_ReturnsTrue()
    {
        // Arrange
        var location = new LocationModel { Id = 1, Name = "Berlin" };
        var filter = "BERLIN";

        // Act
        var result = location.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithMatchingDescription_ReturnsTrue()
    {
        // Arrange
        var location = new LocationModel { Id = 1, Name = "Rome", Description = "Capital of Italy" };
        var filter = "Italy";

        // Act
        var result = location.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithPartialDescriptionMatch_ReturnsTrue()
    {
        // Arrange
        var location = new LocationModel { Id = 1, Name = "Sydney", Description = "Largest city in Australia" };
        var filter = "Australia";

        // Act
        var result = location.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithCaseInsensitiveDescriptionMatch_ReturnsTrue()
    {
        // Arrange
        var location = new LocationModel { Id = 1, Name = "Madrid", Description = "Capital of Spain" };
        var filter = "spain";

        // Act
        var result = location.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithNullDescription_OnlyChecksName()
    {
        // Arrange
        var location = new LocationModel { Id = 1, Name = "Amsterdam", Description = null };
        var filter = "Amsterdam";

        // Act
        var result = location.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithNullDescriptionAndNoNameMatch_ReturnsFalse()
    {
        // Arrange
        var location = new LocationModel { Id = 1, Name = "Dublin", Description = null };
        var filter = "Ireland";

        // Act
        var result = location.MatchesTextFilter(filter);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchesTextFilter_WithNoMatch_ReturnsFalse()
    {
        // Arrange
        var location = new LocationModel { Id = 1, Name = "Vienna", Description = "Capital of Austria" };
        var filter = "Germany";

        // Act
        var result = location.MatchesTextFilter(filter);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchesTextFilter_WithWhitespaceFilter_ReturnsFalse()
    {
        // Arrange
        var location = new LocationModel { Id = 1, Name = "Prague" };
        var filter = "   ";

        // Act
        var result = location.MatchesTextFilter(filter);

        // Assert
        Assert.False(result);
    }
}
