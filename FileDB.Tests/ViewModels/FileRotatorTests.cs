using FakeItEasy;
using FileDB.Model;
using FileDB.ViewModels.Search.File;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.ViewModels;

public class FileRotatorTests
{
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IBoundingBoxRotator boundingBoxRotator = A.Fake<IBoundingBoxRotator>();

    private FileRotator CreateFileRotator() => new(dbAccessProvider, boundingBoxRotator);

    // --- Pure degree calculation (static method) ---

    [Fact]
    public void CalculateNewDegrees_Clockwise_From0_Returns270()
    {
        Assert.Equal(270, FileRotator.CalculateNewDegrees(0, RotationDirection.Clockwise));
    }

    [Fact]
    public void CalculateNewDegrees_Clockwise_From90_Returns0()
    {
        Assert.Equal(0, FileRotator.CalculateNewDegrees(90, RotationDirection.Clockwise));
    }

    [Fact]
    public void CalculateNewDegrees_Clockwise_From180_Returns90()
    {
        Assert.Equal(90, FileRotator.CalculateNewDegrees(180, RotationDirection.Clockwise));
    }

    [Fact]
    public void CalculateNewDegrees_Clockwise_From270_Returns180()
    {
        Assert.Equal(180, FileRotator.CalculateNewDegrees(270, RotationDirection.Clockwise));
    }

    [Fact]
    public void CalculateNewDegrees_CounterClockwise_From0_Returns90()
    {
        Assert.Equal(90, FileRotator.CalculateNewDegrees(0, RotationDirection.CounterClockwise));
    }

    [Fact]
    public void CalculateNewDegrees_CounterClockwise_From90_Returns180()
    {
        Assert.Equal(180, FileRotator.CalculateNewDegrees(90, RotationDirection.CounterClockwise));
    }

    [Fact]
    public void CalculateNewDegrees_CounterClockwise_From180_Returns270()
    {
        Assert.Equal(270, FileRotator.CalculateNewDegrees(180, RotationDirection.CounterClockwise));
    }

    [Fact]
    public void CalculateNewDegrees_CounterClockwise_From270_Returns0()
    {
        Assert.Equal(0, FileRotator.CalculateNewDegrees(270, RotationDirection.CounterClockwise));
    }

    // --- Rotate method (includes DB update + FileModel update) ---

    [Fact]
    public void Rotate_UpdatesFileOrientationInDatabase()
    {
        var file = new FileModel { Id = 42, Path = "file.jpg", Orientation = 1 };
        var rotator = CreateFileRotator();

        rotator.Rotate(file, 0, RotationDirection.Clockwise);

        // 0 degrees clockwise => 270 degrees => orientation 6
        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFileOrientation(42, 6)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Rotate_UpdatesFileModelOrientation()
    {
        var file = new FileModel { Id = 1, Path = "file.jpg", Orientation = 1 };
        var rotator = CreateFileRotator();

        rotator.Rotate(file, 0, RotationDirection.CounterClockwise);

        // 0 degrees counter-clockwise => 90 degrees => orientation 8
        Assert.Equal(8, file.Orientation);
    }

    [Fact]
    public void Rotate_ReturnsNewDegrees()
    {
        var file = new FileModel { Id = 1, Path = "file.jpg" };
        var rotator = CreateFileRotator();

        var result = rotator.Rotate(file, 180, RotationDirection.Clockwise);

        Assert.Equal(90, result);
    }

    [Fact]
    public void Rotate_CallsBoundingBoxRotator()
    {
        var file = new FileModel { Id = 42, Path = "file.jpg" };
        var rotator = CreateFileRotator();

        rotator.Rotate(file, 0, RotationDirection.Clockwise);

        A.CallTo(() => boundingBoxRotator.RotateFileBoundingBoxes(42, RotationDirection.Clockwise))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Rotate_CallsBoundingBoxRotatorWithCorrectFileId()
    {
        var file = new FileModel { Id = 123, Path = "file.jpg" };
        var rotator = CreateFileRotator();

        rotator.Rotate(file, 90, RotationDirection.CounterClockwise);

        A.CallTo(() => boundingBoxRotator.RotateFileBoundingBoxes(123, RotationDirection.CounterClockwise))
            .MustHaveHappenedOnceExactly();
    }
}
