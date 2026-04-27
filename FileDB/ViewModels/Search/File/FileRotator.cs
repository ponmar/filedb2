using FileDB.Model;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.File;

public interface IFileRotator
{
    int Rotate(FileModel file, int currentDegrees, RotationDirection direction);
}

public class FileRotator : IFileRotator
{
    private readonly IDatabaseAccessProvider dbAccessProvider;

    public FileRotator(IDatabaseAccessProvider dbAccessProvider)
    {
        this.dbAccessProvider = dbAccessProvider;
    }

    public int Rotate(FileModel file, int currentDegrees, RotationDirection direction)
    {
        var newDegrees = CalculateNewDegrees(currentDegrees, direction);
        var newOrientation = DatabaseParsing.DegreesToOrientation(newDegrees);
        dbAccessProvider.DbAccess.UpdateFileOrientation(file.Id, newOrientation);
        file.Orientation = newOrientation;
        return newDegrees;
    }

    public static int CalculateNewDegrees(int currentDegrees, RotationDirection direction)
    {
        if (direction == RotationDirection.CounterClockwise)
        {
            var degrees = currentDegrees + 90;
            return degrees > 270 ? 0 : degrees;
        }
        else
        {
            var degrees = currentDegrees - 90;
            return degrees < 0 ? 270 : degrees;
        }
    }
}
