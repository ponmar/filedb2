using FileDB.Extensions;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.Extensions;

public class SeasonExtensionsTests
{
    [Fact]
    public void ToFriendlyString_Spring_ReturnsLocalizedString()
    {
        // Arrange
        var season = Season.Spring;

        // Act
        var result = season.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_Summer_ReturnsLocalizedString()
    {
        // Arrange
        var season = Season.Summer;

        // Act
        var result = season.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_Autumn_ReturnsLocalizedString()
    {
        // Arrange
        var season = Season.Autumn;

        // Act
        var result = season.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_Winter_ReturnsLocalizedString()
    {
        // Arrange
        var season = Season.Winter;

        // Act
        var result = season.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_AllSeasons_ReturnsDifferentStrings()
    {
        // Arrange & Act
        var spring = Season.Spring.ToFriendlyString();
        var summer = Season.Summer.ToFriendlyString();
        var autumn = Season.Autumn.ToFriendlyString();
        var winter = Season.Winter.ToFriendlyString();

        // Assert - All seasons should have unique friendly strings
        Assert.NotEqual(spring, summer);
        Assert.NotEqual(spring, autumn);
        Assert.NotEqual(spring, winter);
        Assert.NotEqual(summer, autumn);
        Assert.NotEqual(summer, winter);
        Assert.NotEqual(autumn, winter);
    }
}
