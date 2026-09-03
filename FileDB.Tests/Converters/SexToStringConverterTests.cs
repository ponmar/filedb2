using System.Globalization;
using FileDB.Converters;
using FileDB.Extensions;
using FileDB.Lang;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.Converters;

public class SexToStringConverterTests
{
    [Theory]
    [InlineData(Sex.NotKnown, nameof(Strings.SexNotKnown))]
    [InlineData(Sex.Male, nameof(Strings.SexMale))]
    [InlineData(Sex.Female, nameof(Strings.SexFemale))]
    [InlineData(Sex.NotApplicable, nameof(Strings.SexNotApplicable))]
    public void Convert_ReturnsFriendlyString(Sex sex, string _)
    {
        var converter = new SexToStringConverter();

        var result = converter.Convert(sex, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(sex.ToFriendlyString(), result);
    }

    [Fact]
    public void Convert_Null_ReturnsNull()
    {
        var converter = new SexToStringConverter();

        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Null(result);
    }
}
