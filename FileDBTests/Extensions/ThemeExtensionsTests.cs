using FileDB.Configuration;
using FileDB.Extensions;
using Xunit;

namespace FileDBTests.Extensions;

public class ThemeExtensionsTests
{
    [Fact]
    public void ToFriendlyString_Default_ReturnsLocalizedString()
    {
        // Arrange
        var theme = Theme.Default;

        // Act
        var result = theme.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_Dark_ReturnsLocalizedString()
    {
        // Arrange
        var theme = Theme.Dark;

        // Act
        var result = theme.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_Light_ReturnsLocalizedString()
    {
        // Arrange
        var theme = Theme.Light;

        // Act
        var result = theme.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_AllThemes_ReturnsDifferentStrings()
    {
        // Arrange & Act
        var defaultTheme = Theme.Default.ToFriendlyString();
        var darkTheme = Theme.Dark.ToFriendlyString();
        var lightTheme = Theme.Light.ToFriendlyString();

        // Assert - All themes should have unique friendly strings
        Assert.NotEqual(defaultTheme, darkTheme);
        Assert.NotEqual(defaultTheme, lightTheme);
        Assert.NotEqual(darkTheme, lightTheme);
    }
}
