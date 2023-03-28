using FileDB.ViewModel;
using FileDBShared.Model;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace FileDB.Model;

public record ConfigUpdated();
public record NotificationsUpdated;
public record DateChanged;

public record PersonEdited;
public record LocationEdited;
public record TagEdited;
public record FileEdited;

public record PersonsUpdated;
public record LocationsUpdated;
public record TagsUpdated;
public record CategorizationFunctionKeyPressed(int FunctionKey);

public record FullscreenBrowsingRequested(bool Fullscreen);

public record NewSearchResult();
public record SelectSearchResultFile(FilesModel File);
public record CloseSearchResultFile();
public record RemoveFileFromSearchResult(FilesModel File);

public record PrevFile;
public record NextFile;
public record FirstFile;
public record LastFile;
public record NextDirectory;
public record PrevDirectory;

public record ShowImage(BitmapImage Image, double RotateDegrees);
public record CloseImage;

public record FilesImported(List<FilesModel> Files);

public record CloseModalDialogRequested;
