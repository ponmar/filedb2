using Avalonia.Media.Imaging;
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

public record SearchForFiles(string FileList);
public record SearchForPerson(PersonModel Person);
public record SearchForPersonGroup(IEnumerable<PersonModel> Persons);
public record SearchForLocation(LocationModel Location);
public record SearchForTag(TagModel Tag);
public record SearchForTags(IEnumerable<TagModel> Tags);
public record SearchForAnnualDate(int Month, int Day);

public record AddPersonSearchFilter(PersonModel Person);
public record AddPersonGroupSearchFilter(IEnumerable<PersonModel> Persons);
public record AddLocationSearchFilter(LocationModel Location);
public record AddTagSearchFilter(TagModel Tag);
public record AddTagsSearchFilter(IEnumerable<TagModel> Tags);
public record AddDateSearchFilter(int Month, int Day);
