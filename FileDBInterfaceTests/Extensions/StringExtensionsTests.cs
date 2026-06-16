using FileDBInterface.Extensions;
using Xunit;

namespace FileDBInterfaceTests.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void HasContent_WithNull_ReturnsFalse()
    {
        // Arrange
        string? str = null;

        // Act
        var result = str.HasContent();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasContent_WithEmptyString_ReturnsFalse()
    {
        // Arrange
        var str = string.Empty;

        // Act
        var result = str.HasContent();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasContent_WithWhitespace_ReturnsTrue()
    {
        // Arrange
        var str = "   ";

        // Act
        var result = str.HasContent();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasContent_WithValidString_ReturnsTrue()
    {
        // Arrange
        var str = "test";

        // Act
        var result = str.HasContent();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TextBeforeLast_WithSingleOccurrence_ReturnsTextBeforeSearch()
    {
        // Arrange
        var str = "path/to/file.txt";
        var search = "/";

        // Act
        var result = str.TextBeforeLast(search);

        // Assert
        Assert.Equal("path/to", result);
    }

    [Fact]
    public void TextBeforeLast_WithMultipleOccurrences_ReturnsTextBeforeLastOccurrence()
    {
        // Arrange
        var str = "a.b.c.d";
        var search = ".";

        // Act
        var result = str.TextBeforeLast(search);

        // Assert
        Assert.Equal("a.b.c", result);
    }

    [Fact]
    public void TextBeforeLast_WithSearchAtEnd_ReturnsFullStringExceptLast()
    {
        // Arrange
        var str = "test/";
        var search = "/";

        // Act
        var result = str.TextBeforeLast(search);

        // Assert
        Assert.Equal("test", result);
    }

    [Fact]
    public void TextBeforeLast_WithLongerSearchString_ReturnsTextBeforeSearch()
    {
        // Arrange
        var str = "file1.backup.txt";
        var search = ".backup";

        // Act
        var result = str.TextBeforeLast(search);

        // Assert
        Assert.Equal("file1", result);
    }
}
