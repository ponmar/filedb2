using Avalonia.Media.Imaging;
using FileDBShared.Model;
using System;
using System.Collections.Generic;

namespace FileDBAvalonia.Model;

public record ConfigUpdated();
public record NotificationsUpdated;
public record DateChanged;
public record Quit;

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
public record SelectSearchResultFile(FileModel File);
public record CloseSearchResultFile();
public record RemoveFileFromSearchResult(FileModel File);

public record SelectPrevFile;
public record SelectNextFile;
public record SelectFirstFile;
public record SelectLastFile;
public record SelectFileInNextDirectory;
public record SelectFileInPrevDirectory;

public record ImageLoaded(string FilePath, Bitmap Image);
public record ImageLoadError(string FilePath, Exception Exception);

public record FilesImported(List<FileModel> Files);

public record CloseModalDialogRequest;
