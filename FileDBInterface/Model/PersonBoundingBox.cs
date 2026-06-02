namespace FileDBInterface.Model;

/// <summary>
/// Represents a bounding box for a person in a file image.
/// All coordinates are normalized to [0.0, 1.0] range relative to image dimensions.
/// </summary>
public record PersonBoundingBox(double X, double Y, double Width, double Height);
