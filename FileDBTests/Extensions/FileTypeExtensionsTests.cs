using FileDB.Extensions;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.Extensions;

public class FileTypeExtensionsTests
{
    [Fact]
    public void ToFriendlyString_Picture_ReturnsLocalizedString()
    {
        // Arrange
        var fileType = FileType.Picture;

        // Act
        var result = fileType.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_Movie_ReturnsLocalizedString()
    {
        // Arrange
        var fileType = FileType.Movie;

        // Act
        var result = fileType.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_Document_ReturnsLocalizedString()
    {
        // Arrange
        var fileType = FileType.Document;

        // Act
        var result = fileType.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_Audio_ReturnsLocalizedString()
    {
        // Arrange
        var fileType = FileType.Audio;

        // Act
        var result = fileType.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_Unknown_ReturnsLocalizedString()
    {
        // Arrange
        var fileType = FileType.Unknown;

        // Act
        var result = fileType.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToIcon_Picture_ReturnsFrameIcon()
    {
        // Arrange
        var fileType = FileType.Picture;

        // Act
        var result = fileType.ToIcon();

        // Assert
        Assert.Equal("\xD83D\xDDBC", result);
    }

    [Fact]
    public void ToIcon_Movie_ReturnsClapperBoardIcon()
    {
        // Arrange
        var fileType = FileType.Movie;

        // Act
        var result = fileType.ToIcon();

        // Assert
        Assert.Equal("\xD83C\xDFAC", result);
    }

    [Fact]
    public void ToIcon_Document_ReturnsDocumentIcon()
    {
        // Arrange
        var fileType = FileType.Document;

        // Act
        var result = fileType.ToIcon();

        // Assert
        Assert.Equal("\xD83D\xDDCE", result);
    }

    [Fact]
    public void ToIcon_Audio_ReturnsMusicalNoteIcon()
    {
        // Arrange
        var fileType = FileType.Audio;

        // Act
        var result = fileType.ToIcon();

        // Assert
        Assert.Equal("\xD83C\xDFB5", result);
    }

    [Fact]
    public void ToIcon_Unknown_ReturnsQuestionIcon()
    {
        // Arrange
        var fileType = FileType.Unknown;

        // Act
        var result = fileType.ToIcon();

        // Assert
        Assert.Equal("\x2370", result);
    }

    [Fact]
    public void GetSupportedFileExtensions_Picture_ReturnsImageExtensions()
    {
        // Arrange
        var fileType = FileType.Picture;

        // Act
        var result = fileType.GetSupportedFileExtensions();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(".jpg", result);
        Assert.Contains(".jpeg", result);
        Assert.Contains(".png", result);
        Assert.Contains(".bmp", result);
        Assert.Contains(".gif", result);
        Assert.Equal(5, result.Length);
    }

    [Fact]
    public void GetSupportedFileExtensions_Movie_ReturnsVideoExtensions()
    {
        // Arrange
        var fileType = FileType.Movie;

        // Act
        var result = fileType.GetSupportedFileExtensions();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(".mkv", result);
        Assert.Contains(".avi", result);
        Assert.Contains(".mpg", result);
        Assert.Contains(".mov", result);
        Assert.Contains(".mp4", result);
        Assert.Equal(5, result.Length);
    }

    [Fact]
    public void GetSupportedFileExtensions_Document_ReturnsDocumentExtensions()
    {
        // Arrange
        var fileType = FileType.Document;

        // Act
        var result = fileType.GetSupportedFileExtensions();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(".doc", result);
        Assert.Contains(".pdf", result);
        Assert.Contains(".txt", result);
        Assert.Contains(".md", result);
        Assert.Equal(4, result.Length);
    }

    [Fact]
    public void GetSupportedFileExtensions_Audio_ReturnsAudioExtensions()
    {
        // Arrange
        var fileType = FileType.Audio;

        // Act
        var result = fileType.GetSupportedFileExtensions();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(".mp3", result);
        Assert.Contains(".wav", result);
        Assert.Equal(2, result.Length);
    }

    [Fact]
    public void GetSupportedFileExtensions_Unknown_ReturnsEmptyArray()
    {
        // Arrange
        var fileType = FileType.Unknown;

        // Act
        var result = fileType.GetSupportedFileExtensions();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
