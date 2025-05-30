﻿using Avalonia.Media.Imaging;
using FileDB.Configuration;
using FileDBInterface.Model;
using System;
using System.Collections.Generic;

namespace FileDB.Model;

public record ConfigEdited(bool HasChanges);
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

public record SetTheme(Theme Theme);
public record FullscreenBrowsingRequested(bool Fullscreen);

public record TransferSearchResult(IEnumerable<FileModel> Files);
public record SearchResultRepositoryUpdated();
public record FileSelectionChanged();
public record RemoveFileFromSearchResult(FileModel File);

public record SelectPrevFile;
public record SelectNextFile;
public record SelectFirstFile;
public record SelectLastFile;
public record SelectFileInNextDirectory;
public record SelectFileInPrevDirectory;

public record ImageLoaded(string FilePath, Bitmap Image);
public record ImageLoadError(string FilePath, Exception Exception);

public record CloseModalDialogRequest;
