using FileDBShared.Model;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace FileDB.Model;

public record ConfigUpdated();
public record NotificationsUpdated;
public record DateChanged;

public record PersonsUpdated;
public record LocationsUpdated;
public record TagsUpdated;

public record FullscreenBrowsingRequested(bool Fullscreen);

public record ShowImage(BitmapImage Image, double RotateDegrees);
public record CloseImage;

public record FilesImported(List<FilesModel> Files);

public record CloseModalDialogRequested;
