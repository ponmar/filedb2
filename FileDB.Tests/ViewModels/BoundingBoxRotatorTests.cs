using FakeItEasy;
using FileDB.Model;
using FileDB.ViewModels.Search.File;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.ViewModels;

public class BoundingBoxRotatorTests
{
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();

    private BoundingBoxRotator CreateBoundingBoxRotator() => new(dbAccessProvider);

    // --- RotateBoundingBox 90° Clockwise ---

    [Fact]
    public void RotateBoundingBox_Clockwise_TransformsCoordinatesCorrectly()
    {
        var rotator = CreateBoundingBoxRotator();
        var bbox = new PersonBoundingBox(0.3, 0.2, 0.4, 0.5);

        var result = rotator.RotateBoundingBox(bbox, RotationDirection.Clockwise);

        // Expected (counter-clockwise formula): x' = 1 - y - h = 1 - 0.2 - 0.5 = 0.3, y' = x = 0.3, w' = h = 0.5, h' = w = 0.4
        Assert.Equal(0.3, result.X, 10);
        Assert.Equal(0.3, result.Y, 10);
        Assert.Equal(0.5, result.Width, 10);
        Assert.Equal(0.4, result.Height, 10);
    }

    [Fact]
    public void RotateBoundingBox_Clockwise_WithZeroPosition()
    {
        var rotator = CreateBoundingBoxRotator();
        var bbox = new PersonBoundingBox(0, 0, 1, 1);

        var result = rotator.RotateBoundingBox(bbox, RotationDirection.Clockwise);

        // (0, 0, 1, 1) clockwise => (0, 0, 1, 1) (full coverage)
        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(1, result.Width);
        Assert.Equal(1, result.Height);
    }

    [Fact]
    public void RotateBoundingBox_Clockwise_WithSmallBbox()
    {
        var rotator = CreateBoundingBoxRotator();
        var bbox = new PersonBoundingBox(0.1, 0.2, 0.2, 0.3);

        var result = rotator.RotateBoundingBox(bbox, RotationDirection.Clockwise);

        // Expected (counter-clockwise formula): x' = 1 - 0.2 - 0.3 = 0.5, y' = 0.1, w' = 0.3, h' = 0.2
        Assert.Equal(0.5, result.X);
        Assert.Equal(0.1, result.Y);
        Assert.Equal(0.3, result.Width);
        Assert.Equal(0.2, result.Height);
    }

    // --- RotateBoundingBox 90° Counter-Clockwise ---

    [Fact]
    public void RotateBoundingBox_CounterClockwise_TransformsCoordinatesCorrectly()
    {
        var rotator = CreateBoundingBoxRotator();
        var bbox = new PersonBoundingBox(0.3, 0.2, 0.4, 0.5);

        var result = rotator.RotateBoundingBox(bbox, RotationDirection.CounterClockwise);

        // Expected (clockwise formula): x' = y = 0.2, y' = 1 - x - w = 1 - 0.3 - 0.4 = 0.3, w' = h = 0.5, h' = w = 0.4
        Assert.Equal(0.2, result.X, 10);
        Assert.Equal(0.3, result.Y, 10);
        Assert.Equal(0.5, result.Width, 10);
        Assert.Equal(0.4, result.Height, 10);
    }

    [Fact]
    public void RotateBoundingBox_CounterClockwise_WithZeroPosition()
    {
        var rotator = CreateBoundingBoxRotator();
        var bbox = new PersonBoundingBox(0, 0, 1, 1);

        var result = rotator.RotateBoundingBox(bbox, RotationDirection.CounterClockwise);

        // (0, 0, 1, 1) counter-clockwise => (0, 0, 1, 1) (full coverage)
        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(1, result.Width);
        Assert.Equal(1, result.Height);
    }

    [Fact]
    public void RotateBoundingBox_CounterClockwise_WithSmallBbox()
    {
        var rotator = CreateBoundingBoxRotator();
        var bbox = new PersonBoundingBox(0.1, 0.2, 0.2, 0.3);

        var result = rotator.RotateBoundingBox(bbox, RotationDirection.CounterClockwise);

        // Expected (clockwise formula): x' = 0.2, y' = 1 - 0.1 - 0.2 = 0.7, w' = 0.3, h' = 0.2
        Assert.Equal(0.2, result.X);
        Assert.Equal(0.7, result.Y);
        Assert.Equal(0.3, result.Width);
        Assert.Equal(0.2, result.Height);
    }

    // --- Cumulative Rotations ---

    [Fact]
    public void RotateBoundingBox_TwiceClockwise_EqualsRotate180()
    {
        var rotator = CreateBoundingBoxRotator();
        var bbox = new PersonBoundingBox(0.3, 0.2, 0.4, 0.5);

        // First rotation
        var after90cw = rotator.RotateBoundingBox(bbox, RotationDirection.Clockwise);

        // Second rotation
        var after180 = rotator.RotateBoundingBox(after90cw, RotationDirection.Clockwise);

        // For 180° rotation: (x, y, w, h) => (1 - x - w, 1 - y - h, w, h)
        var expected_x = 1 - 0.3 - 0.4; // 0.3
        var expected_y = 1 - 0.2 - 0.5; // 0.3
        Assert.Equal(expected_x, after180.X, 5);
        Assert.Equal(expected_y, after180.Y, 5);
        Assert.Equal(0.4, after180.Width);
        Assert.Equal(0.5, after180.Height);
    }

    [Fact]
    public void RotateBoundingBox_ClockwiseThenCounterClockwise_ReturnToOriginal()
    {
        var rotator = CreateBoundingBoxRotator();
        var bbox = new PersonBoundingBox(0.3, 0.2, 0.4, 0.5);

        // Rotate clockwise then counter-clockwise
        var rotated = rotator.RotateBoundingBox(bbox, RotationDirection.Clockwise);
        var unrotated = rotator.RotateBoundingBox(rotated, RotationDirection.CounterClockwise);

        // Should return to original
        Assert.Equal(bbox.X, unrotated.X, 5);
        Assert.Equal(bbox.Y, unrotated.Y, 5);
        Assert.Equal(bbox.Width, unrotated.Width, 5);
        Assert.Equal(bbox.Height, unrotated.Height, 5);
    }

    // --- RotateFileBoundingBoxes Integration ---

    [Fact]
    public void RotateFileBoundingBoxes_WithNoBboxes_DoesNotErrorAndDoesNotUpdate()
    {
        var rotator = CreateBoundingBoxRotator();
        A.CallTo(() => dbAccessProvider.DbAccess.GetFilePersonBoundingBoxes(42))
            .Returns(new List<(int, PersonBoundingBox)>());

        rotator.RotateFileBoundingBoxes(42, RotationDirection.Clockwise);

        // Verify no update calls were made
        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFilePersonBoundingBox(A<int>._, A<int>._, A<PersonBoundingBox>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public void RotateFileBoundingBoxes_WithOneBbox_UpdatesDatabase()
    {
        var rotator = CreateBoundingBoxRotator();
        var bbox = new PersonBoundingBox(0.3, 0.2, 0.4, 0.5);
        A.CallTo(() => dbAccessProvider.DbAccess.GetFilePersonBoundingBoxes(42))
            .Returns(new List<(int, PersonBoundingBox)> { (10, bbox) });

        rotator.RotateFileBoundingBoxes(42, RotationDirection.Clockwise);

        // Verify the bbox was transformed and updated
        // Clockwise uses counter-clockwise formula: x' = 0.3, y' = 0.3, w' = 0.5, h' = 0.4
        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFilePersonBoundingBox(
                42, 10, A<PersonBoundingBox>.That.Matches(b =>
                    Math.Abs(b.X - 0.3) < 0.00001 && 
                    Math.Abs(b.Y - 0.3) < 0.00001 && 
                    Math.Abs(b.Width - 0.5) < 0.00001 && 
                    Math.Abs(b.Height - 0.4) < 0.00001)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void RotateFileBoundingBoxes_WithMultipleBboxes_UpdatesAllInDatabase()
    {
        var rotator = CreateBoundingBoxRotator();
        var bbox1 = new PersonBoundingBox(0.1, 0.2, 0.3, 0.4);
        var bbox2 = new PersonBoundingBox(0.5, 0.5, 0.2, 0.2);
        A.CallTo(() => dbAccessProvider.DbAccess.GetFilePersonBoundingBoxes(42))
            .Returns(new List<(int, PersonBoundingBox)> { (1, bbox1), (2, bbox2) });

        rotator.RotateFileBoundingBoxes(42, RotationDirection.Clockwise);

        // Verify both bboxes were transformed and updated
        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFilePersonBoundingBox(42, A<int>._, A<PersonBoundingBox>._))
            .MustHaveHappened(2, Times.Exactly);
    }

    [Fact]
    public void RotateFileBoundingBoxes_CounterClockwise_TransformsCorrectly()
    {
        var rotator = CreateBoundingBoxRotator();
        var bbox = new PersonBoundingBox(0.3, 0.2, 0.4, 0.5);
        A.CallTo(() => dbAccessProvider.DbAccess.GetFilePersonBoundingBoxes(42))
            .Returns(new List<(int, PersonBoundingBox)> { (10, bbox) });

        rotator.RotateFileBoundingBoxes(42, RotationDirection.CounterClockwise);

        // CounterClockwise uses clockwise formula: x' = 0.2, y' = 0.3, w' = 0.5, h' = 0.4
        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFilePersonBoundingBox(
                42, 10, A<PersonBoundingBox>.That.Matches(b =>
                    Math.Abs(b.X - 0.2) < 0.00001 && 
                    Math.Abs(b.Y - 0.3) < 0.00001 && 
                    Math.Abs(b.Width - 0.5) < 0.00001 && 
                    Math.Abs(b.Height - 0.4) < 0.00001)))
            .MustHaveHappenedOnceExactly();
    }
}
