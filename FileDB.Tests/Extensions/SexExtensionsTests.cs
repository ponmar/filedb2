using FileDB.Extensions;
using FileDBInterface.Extensions;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.Extensions;

public class SexExtensionsTests
{
    [Fact]
    public void ToFriendlyString_AllValues_ReturnNonEmptyStrings()
    {
        foreach (var value in Enum.GetValues<Sex>())
        {
            Assert.True(value.ToFriendlyString().HasContent());
        }
    }

    [Fact]
    public void ToFriendlyString_NotKnown_ReturnsLocalizedString()
    {
        // Arrange
        var sex = Sex.NotKnown;

        // Act
        var result = sex.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_Male_ReturnsLocalizedString()
    {
        // Arrange
        var sex = Sex.Male;

        // Act
        var result = sex.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_Female_ReturnsLocalizedString()
    {
        // Arrange
        var sex = Sex.Female;

        // Act
        var result = sex.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_NotApplicable_ReturnsLocalizedString()
    {
        // Arrange
        var sex = Sex.NotApplicable;

        // Act
        var result = sex.ToFriendlyString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToFriendlyString_AllValues_ReturnsDifferentStrings()
    {
        // Arrange & Act
        var notKnown = Sex.NotKnown.ToFriendlyString();
        var male = Sex.Male.ToFriendlyString();
        var female = Sex.Female.ToFriendlyString();
        var notApplicable = Sex.NotApplicable.ToFriendlyString();

        // Assert - All values should have unique friendly strings
        Assert.NotEqual(notKnown, male);
        Assert.NotEqual(notKnown, female);
        Assert.NotEqual(notKnown, notApplicable);
        Assert.NotEqual(male, female);
        Assert.NotEqual(male, notApplicable);
        Assert.NotEqual(female, notApplicable);
    }
}
