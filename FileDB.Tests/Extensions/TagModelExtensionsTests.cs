using FileDB.Extensions;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.Extensions;

public class TagModelExtensionsTests
{
    [Fact]
    public void MatchesTextFilter_WithEmptyFilter_ReturnsTrue()
    {
        // Arrange
        var tag = new TagModel { Id = 1, Name = "Vacation" };
        var filter = "";

        // Act
        var result = tag.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithNullFilter_ReturnsTrue()
    {
        // Arrange
        var tag = new TagModel { Id = 1, Name = "Family" };
        string? filter = null;

        // Act
        var result = tag.MatchesTextFilter(filter!);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithMatchingName_ReturnsTrue()
    {
        // Arrange
        var tag = new TagModel { Id = 1, Name = "Birthday" };
        var filter = "Birthday";

        // Act
        var result = tag.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithPartialNameMatch_ReturnsTrue()
    {
        // Arrange
        var tag = new TagModel { Id = 1, Name = "Christmas" };
        var filter = "Christ";

        // Act
        var result = tag.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithCaseInsensitiveMatch_ReturnsTrue()
    {
        // Arrange
        var tag = new TagModel { Id = 1, Name = "Wedding" };
        var filter = "wedding";

        // Act
        var result = tag.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithUpperCaseFilter_ReturnsTrue()
    {
        // Arrange
        var tag = new TagModel { Id = 1, Name = "Graduation" };
        var filter = "GRADUATION";

        // Act
        var result = tag.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithMixedCaseFilter_ReturnsTrue()
    {
        // Arrange
        var tag = new TagModel { Id = 1, Name = "NewYear" };
        var filter = "nEwYeAr";

        // Act
        var result = tag.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithPartialMatchAtStart_ReturnsTrue()
    {
        // Arrange
        var tag = new TagModel { Id = 1, Name = "Anniversary" };
        var filter = "Anniv";

        // Act
        var result = tag.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithPartialMatchInMiddle_ReturnsTrue()
    {
        // Arrange
        var tag = new TagModel { Id = 1, Name = "Thanksgiving" };
        var filter = "giving";

        // Act
        var result = tag.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithPartialMatchAtEnd_ReturnsTrue()
    {
        // Arrange
        var tag = new TagModel { Id = 1, Name = "Easter" };
        var filter = "ter";

        // Act
        var result = tag.MatchesTextFilter(filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTextFilter_WithNoMatch_ReturnsFalse()
    {
        // Arrange
        var tag = new TagModel { Id = 1, Name = "Halloween" };
        var filter = "Christmas";

        // Act
        var result = tag.MatchesTextFilter(filter);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchesTextFilter_WithWhitespaceFilter_ReturnsFalse()
    {
        // Arrange
        var tag = new TagModel { Id = 1, Name = "Party" };
        var filter = "   ";

        // Act
        var result = tag.MatchesTextFilter(filter);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchesTextFilter_WithCompletelyDifferentText_ReturnsFalse()
    {
        // Arrange
        var tag = new TagModel { Id = 1, Name = "Concert" };
        var filter = "xyz";

        // Act
        var result = tag.MatchesTextFilter(filter);

        // Assert
        Assert.False(result);
    }
}
