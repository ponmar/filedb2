using System.ComponentModel;
using FileDB.Extensions;
using Xunit;

namespace FileDB.Tests.Extensions;

public class EnumExtensionsTests
{
    [Fact]
    public void GetAttribute_WithDescriptionAttribute_ReturnsAttribute()
    {
        // Arrange
        var enumValue = TestEnum.ValueWithDescription;

        // Act
        var attribute = enumValue.GetAttribute<DescriptionAttribute>();

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("Test Description", attribute.Description);
    }

    [Fact]
    public void GetAttribute_WithDifferentDescriptions_ReturnsCorrectAttribute()
    {
        // Arrange
        var enumValue1 = TestEnum.ValueWithDescription;
        var enumValue2 = TestEnum.AnotherValueWithDescription;

        // Act
        var attribute1 = enumValue1.GetAttribute<DescriptionAttribute>();
        var attribute2 = enumValue2.GetAttribute<DescriptionAttribute>();

        // Assert
        Assert.Equal("Test Description", attribute1.Description);
        Assert.Equal("Another Description", attribute2.Description);
    }

    [Fact]
    public void GetAttribute_WithCustomAttribute_ReturnsAttribute()
    {
        // Arrange
        var enumValue = TestEnum.ValueWithCustomAttribute;

        // Act
        var attribute = enumValue.GetAttribute<CustomTestAttribute>();

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("Custom Value", attribute.Value);
    }

    [Fact]
    public void GetAttribute_WithMultipleAttributesOfDifferentTypes_ReturnsRequestedType()
    {
        // Arrange
        var enumValue = TestEnum.ValueWithMultipleAttributes;

        // Act
        var descriptionAttribute = enumValue.GetAttribute<DescriptionAttribute>();
        var customAttribute = enumValue.GetAttribute<CustomTestAttribute>();

        // Assert
        Assert.NotNull(descriptionAttribute);
        Assert.Equal("Multiple Attributes", descriptionAttribute.Description);
        Assert.NotNull(customAttribute);
        Assert.Equal("Multi", customAttribute.Value);
    }

    private enum TestEnum
    {
        [Description("Test Description")]
        ValueWithDescription,

        [Description("Another Description")]
        AnotherValueWithDescription,

        [CustomTest("Custom Value")]
        ValueWithCustomAttribute,

        [Description("Multiple Attributes")]
        [CustomTest("Multi")]
        ValueWithMultipleAttributes
    }

    [AttributeUsage(AttributeTargets.Field)]
    private class CustomTestAttribute : Attribute
    {
        public string Value { get; }

        public CustomTestAttribute(string value)
        {
            Value = value;
        }
    }
}
