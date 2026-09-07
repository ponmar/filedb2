using FileDB.Model;
using Xunit;

namespace FileDB.Tests.Extensions;

public class FilterTypeExtensionsTests
{
    [Fact]
    public void ToFriendlyString_AllFilterTypes_ReturnNonEmptyUniqueStrings()
    {
        var friendlyStrings = Enum.GetValues<FilterType>()
            .Select(filterType => filterType.ToFriendlyString())
            .ToList();

        Assert.All(friendlyStrings, value => Assert.NotEmpty(value));
        Assert.Equal(friendlyStrings.Count, friendlyStrings.Distinct().Count());
    }

    [Fact]
    public void ToFriendlyString_UnknownValue_Throws()
    {
        Assert.Throws<NotImplementedException>(() => ((FilterType)int.MaxValue).ToFriendlyString());
    }
}
