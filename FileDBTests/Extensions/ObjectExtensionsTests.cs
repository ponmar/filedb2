using FileDB.Extensions;
using Xunit;

namespace FileDBTests.Extensions;

public class ObjectExtensionsTests
{
    [Fact]
    public void ToJson_WithSimpleObject_ReturnsJsonString()
    {
        // Arrange
        var obj = new { Name = "Test", Value = 123 };

        // Act
        var result = obj.ToJson();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("\"Name\"", result);
        Assert.Contains("\"Test\"", result);
        Assert.Contains("\"Value\"", result);
        Assert.Contains("123", result);
    }

    [Fact]
    public void ToJson_WithComplexObject_ReturnsJsonString()
    {
        // Arrange
        var obj = new TestObject
        {
            Name = "Parent",
            Value = 456,
            Child = new ChildObject { ChildName = "Child", ChildValue = 789 }
        };

        // Act
        var result = obj.ToJson();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("\"Name\"", result);
        Assert.Contains("\"Parent\"", result);
        Assert.Contains("\"Child\"", result);
        Assert.Contains("\"ChildName\"", result);
        Assert.Contains("\"Child\"", result);
    }

    [Fact]
    public void ToJson_WithArray_ReturnsJsonArray()
    {
        // Arrange
        var obj = new[] { 1, 2, 3, 4, 5 };

        // Act
        var result = obj.ToJson();

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("[", result);
        Assert.EndsWith("]", result);
        Assert.Contains("1", result);
        Assert.Contains("5", result);
    }

    [Fact]
    public void ToJson_WithNull_ReturnsNullString()
    {
        // Arrange
        object? obj = null;

        // Act
        var result = obj!.ToJson();

        // Assert
        Assert.Equal("null", result);
    }

    [Fact]
    public void ToJson_WithString_ReturnsQuotedString()
    {
        // Arrange
        var obj = "test string";

        // Act
        var result = obj.ToJson();

        // Assert
        Assert.Equal("\"test string\"", result);
    }

    [Fact]
    public void ToJson_WithNumber_ReturnsNumberString()
    {
        // Arrange
        var obj = 42;

        // Act
        var result = obj.ToJson();

        // Assert
        Assert.Equal("42", result);
    }

    [Fact]
    public void ToJson_WithBoolean_ReturnsLowercaseBoolean()
    {
        // Arrange
        var obj = true;

        // Act
        var result = obj.ToJson();

        // Assert
        Assert.Equal("true", result);
    }

    [Fact]
    public void ToFormattedJson_WithSimpleObject_ReturnsIndentedJson()
    {
        // Arrange
        var obj = new { Name = "Test", Value = 123 };

        // Act
        var result = obj.ToFormattedJson();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("\n", result); // Should contain newlines for formatting
        Assert.Contains("  ", result); // Should contain indentation
        Assert.Contains("\"Name\"", result);
        Assert.Contains("\"Test\"", result);
    }

    [Fact]
    public void ToFormattedJson_WithComplexObject_ReturnsNestedIndentedJson()
    {
        // Arrange
        var obj = new TestObject
        {
            Name = "Parent",
            Value = 456,
            Child = new ChildObject { ChildName = "Child", ChildValue = 789 }
        };

        // Act
        var result = obj.ToFormattedJson();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("\n", result); // Should contain newlines
        Assert.Contains("  ", result); // Should contain indentation
        var lines = result.Split('\n');
        Assert.True(lines.Length > 5); // Should be multiple lines
    }

    [Fact]
    public void ToFormattedJson_WithArray_ReturnsIndentedArray()
    {
        // Arrange
        var obj = new[] { new { Id = 1 }, new { Id = 2 } };

        // Act
        var result = obj.ToFormattedJson();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("\n", result);
        Assert.StartsWith("[", result.TrimStart());
        Assert.EndsWith("]", result.TrimEnd());
    }

    [Fact]
    public void ToFormattedJson_ProducesMoreLinesThanToJson()
    {
        // Arrange
        var obj = new { Name = "Test", Value = 123, Child = new { Name = "Child" } };

        // Act
        var compactJson = obj.ToJson();
        var formattedJson = obj.ToFormattedJson();

        // Assert
        var compactLines = compactJson.Split('\n').Length;
        var formattedLines = formattedJson.Split('\n').Length;
        Assert.True(formattedLines > compactLines);
    }

    private class TestObject
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public ChildObject? Child { get; set; }
    }

    private class ChildObject
    {
        public string ChildName { get; set; } = string.Empty;
        public int ChildValue { get; set; }
    }
}
