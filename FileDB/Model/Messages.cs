using FileDBInterface.Model;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace FileDB.Model;

public record ConfigLoaded;
public record NotificationsUpdated;
public record DateChanged;

public record PersonsUpdated;
public record LocationsUpdated;
public record TagsUpdated;

public record FullscreenBrowsingRequested(bool Fullscreen);

public record ShowImage(BitmapImage Image, double RotateDegrees);
public record ShowVideo(string Path);
public record CloseFile;

public record FilesImported(List<FilesModel> Files);

public record CloseModalDialogRequested;
