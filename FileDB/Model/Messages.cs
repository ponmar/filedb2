﻿using FileDBShared.Model;
using System;
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

public record SelectPrevFile;
public record SelectNextFile;
public record SelectFirstFile;
public record SelectLastFile;
public record SelectFileInNextDirectory;
public record SelectFileInPrevDirectory;

public record ImageLoaded(string FilePath, BitmapImage Image);
public record ImageLoadError(string FilePath, Exception Exception);

public record FilesImported(List<FilesModel> Files);

public record CloseModalDialogRequest;
