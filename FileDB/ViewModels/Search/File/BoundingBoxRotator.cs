using FileDB.Model;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.File;

public interface IBoundingBoxRotator
{
    void RotateFileBoundingBoxes(int fileId, RotationDirection direction);
    PersonBoundingBox RotateBoundingBox(PersonBoundingBox bbox, RotationDirection direction);
}

public class BoundingBoxRotator : IBoundingBoxRotator
{
    private readonly IDatabaseAccessProvider dbAccessProvider;

    public BoundingBoxRotator(IDatabaseAccessProvider dbAccessProvider)
    {
        this.dbAccessProvider = dbAccessProvider;
    }

    public void RotateFileBoundingBoxes(int fileId, RotationDirection direction)
    {
        var bboxes = dbAccessProvider.DbAccess.GetFilePersonBoundingBoxes(fileId);
        
        foreach (var (personId, bbox) in bboxes)
        {
            var rotatedBbox = RotateBoundingBox(bbox, direction);
            dbAccessProvider.DbAccess.UpdateFilePersonBoundingBox(fileId, personId, rotatedBbox);
        }
    }

    public PersonBoundingBox RotateBoundingBox(PersonBoundingBox bbox, RotationDirection direction)
    {
        if (direction == RotationDirection.CounterClockwise)
        {
            // 90° visual clockwise: (x, y, w, h) → (y, 1 - x - w, h, w)
            // Note: Direction is inverted due to FileRotator's degree semantics
            return new PersonBoundingBox(
                bbox.Y,
                1 - bbox.X - bbox.Width,
                bbox.Height,
                bbox.Width);
        }
        else
        {
            // 90° visual counter-clockwise: (x, y, w, h) → (1 - y - h, x, h, w)
            // Note: Direction is inverted due to FileRotator's degree semantics
            return new PersonBoundingBox(
                1 - bbox.Y - bbox.Height,
                bbox.X,
                bbox.Height,
                bbox.Width);
        }
    }
}
