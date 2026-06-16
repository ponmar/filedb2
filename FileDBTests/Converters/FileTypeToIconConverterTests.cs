using System.Globalization;
using FileDB.Converters;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.Converters;

public class FileTypeToIconConverterTests
{
    [Theory]
    [InlineData(FileType.Picture, "\xD83D\xDDBC")]
    [InlineData(FileType.Movie, "\xD83C\xDFAC")]
    [InlineData(FileType.Document, "\xD83D\xDDCE")]
    [InlineData(FileType.Audio, "\xD83C\xDFB5")]
    [InlineData(FileType.Unknown, "\x2370")]
    public void Convert_ReturnsIcon(FileType fileType, string expected)
    {
        var converter = new FileTypeToIconConverter();

        var result = converter.Convert(fileType, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_Null_ReturnsNull()
    {
        var converter = new FileTypeToIconConverter();

        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Null(result);
    }
}
