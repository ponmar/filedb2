using System.IO.Abstractions;
using FakeItEasy;
using FileDB.Extensions;
using Xunit;

namespace FileDB.Tests.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void FromJson_WithValidJson_ReturnsDeserializedObject()
    {
        // Arrange
        var filePath = "test.json";
        var json = "{\"Name\":\"Test\",\"Value\":123}";
        var fileSystem = A.Fake<IFileSystem>();
        A.CallTo(() => fileSystem.File.ReadAllText(filePath)).Returns(json);

        // Act
        var result = filePath.FromJson<TestObject>(fileSystem);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
        Assert.Equal(123, result.Value);
    }

    [Fact]
    public void FromJson_WithEmptyJson_ReturnsNull()
    {
        // Arrange
        var filePath = "empty.json";
        var json = "";
        var fileSystem = A.Fake<IFileSystem>();
        A.CallTo(() => fileSystem.File.ReadAllText(filePath)).Returns(json);

        // Act
        var result = filePath.FromJson<TestObject>(fileSystem);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_WithNullJson_ReturnsNull()
    {
        // Arrange
        var filePath = "null.json";
        var json = "null";
        var fileSystem = A.Fake<IFileSystem>();
        A.CallTo(() => fileSystem.File.ReadAllText(filePath)).Returns(json);

        // Act
        var result = filePath.FromJson<TestObject>(fileSystem);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_WithWhitespaceJson_ReturnsNull()
    {
        // Arrange
        var filePath = "whitespace.json";
        var json = "   ";
        var fileSystem = A.Fake<IFileSystem>();
        A.CallTo(() => fileSystem.File.ReadAllText(filePath)).Returns(json);

        // Act
        var result = filePath.FromJson<TestObject>(fileSystem);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_WithComplexObject_ReturnsDeserializedObject()
    {
        // Arrange
        var filePath = "complex.json";
        var json = "{\"Name\":\"Parent\",\"Value\":456,\"Child\":{\"Name\":\"Child\",\"Value\":789}}";
        var fileSystem = A.Fake<IFileSystem>();
        A.CallTo(() => fileSystem.File.ReadAllText(filePath)).Returns(json);

        // Act
        var result = filePath.FromJson<ComplexTestObject>(fileSystem);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Parent", result.Name);
        Assert.Equal(456, result.Value);
        Assert.NotNull(result.Child);
        Assert.Equal("Child", result.Child.Name);
        Assert.Equal(789, result.Child.Value);
    }

    [Fact]
    public void FromJson_WithArray_ReturnsDeserializedArray()
    {
        // Arrange
        var filePath = "array.json";
        var json = "[{\"Name\":\"Item1\",\"Value\":1},{\"Name\":\"Item2\",\"Value\":2}]";
        var fileSystem = A.Fake<IFileSystem>();
        A.CallTo(() => fileSystem.File.ReadAllText(filePath)).Returns(json);

        // Act
        var result = filePath.FromJson<TestObject[]>(fileSystem);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Equal("Item1", result[0].Name);
        Assert.Equal(1, result[0].Value);
        Assert.Equal("Item2", result[1].Name);
        Assert.Equal(2, result[1].Value);
    }

    private class TestObject
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class ComplexTestObject
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public TestObject? Child { get; set; }
    }
}
