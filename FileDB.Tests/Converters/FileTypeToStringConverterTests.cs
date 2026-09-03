using System.Globalization;
using FileDB.Converters;
using FileDB.Extensions;
using FileDB.Lang;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.Converters;

public class FileTypeToStringConverterTests
{
    [Theory]
    [InlineData(FileType.Picture, nameof(Strings.FileTypePicture))]
    [InlineData(FileType.Movie, nameof(Strings.FileTypeMovie))]
    [InlineData(FileType.Document, nameof(Strings.FileTypeDocument))]
    [InlineData(FileType.Audio, nameof(Strings.FileTypeAudio))]
    [InlineData(FileType.Unknown, nameof(Strings.FileTypeUnknown))]
    public void Convert_ReturnsFriendlyString(FileType fileType, string _)
    {
        var converter = new FileTypeToStringConverter();

        var result = converter.Convert(fileType, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(fileType.ToFriendlyString(), result);
    }

    [Fact]
    public void Convert_Null_ReturnsNull()
    {
        var converter = new FileTypeToStringConverter();

        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Null(result);
    }
}
