using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDB.Model;

public record ConfigLoaded;
public record NotificationsUpdated;
public record DateChanged;

public record PersonsUpdated;
public record LocationsUpdated;
public record TagsUpdated;

public record FullscreenBrowsingRequested(bool Fullscreen);

public record FilesImported(List<FilesModel> Files);

public record CloseModalDialogRequested;
