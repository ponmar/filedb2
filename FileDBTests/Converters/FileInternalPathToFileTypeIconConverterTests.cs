using System.Globalization;
using FileDB.Converters;
using FileDB.Extensions;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.Converters;

public class FileInternalPathToFileTypeIconConverterTests
{
    [Fact]
    public void Convert_WithPicturePath_ReturnsPictureIcon()
    {
        var converter = new FileInternalPathToFileTypeIconConverter();

        var result = converter.Convert("photo.jpg", typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(FileType.Picture.ToIcon(), result);
    }

    [Fact]
    public void Convert_WithNull_ThrowsArgumentNullException()
    {
        var converter = new FileInternalPathToFileTypeIconConverter();

        Assert.Throws<ArgumentNullException>(() => converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture));
    }
}
