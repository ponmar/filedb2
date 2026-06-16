using System.Globalization;
using FileDB.Configuration;
using FileDB.Converters;
using FileDB.Extensions;
using FileDB.Lang;
using Xunit;

namespace FileDBTests.Converters;

public class SortMethodToStringConverterTests
{
    [Theory]
    [InlineData(SortMethod.Date, nameof(Strings.SortMethodDate))]
    [InlineData(SortMethod.DateDesc, nameof(Strings.SortMethodDateDesc))]
    [InlineData(SortMethod.Path, nameof(Strings.SortMethodPath))]
    [InlineData(SortMethod.PathDesc, nameof(Strings.SortMethodPathDesc))]
    [InlineData(SortMethod.Random, nameof(Strings.SortMethodRandom))]
    [InlineData(SortMethod.NumPersons, nameof(Strings.SortMethodNumPersons))]
    [InlineData(SortMethod.NumPersonsDesc, nameof(Strings.SortMethodNumPersonsDesc))]
    [InlineData(SortMethod.DirectoryDate, nameof(Strings.SortMethodDirectoryDate))]
    [InlineData(SortMethod.DirectoryDateDesc, nameof(Strings.SortMethodDirectoryDateDesc))]
    public void Convert_ReturnsFriendlyString(SortMethod sortMethod, string _)
    {
        var converter = new SortMethodToStringConverter();

        var result = converter.Convert(sortMethod, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(sortMethod.ToFriendlyString(), result);
    }

    [Fact]
    public void Convert_Null_ReturnsNull()
    {
        var converter = new SortMethodToStringConverter();

        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Null(result);
    }
}
