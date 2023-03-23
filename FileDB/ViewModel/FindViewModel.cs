using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using FileDB.View;
using FileDB.Sorters;
using FileDBInterface.Exceptions;
using FileDBShared.Model;
using TextCopy;
using FileDBInterface.DbAccess;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using FileDBShared.Validators;

namespace FileDB.ViewModel;

public enum RotationDirection { Clockwise, CounterClockwise }

[AttributeUsage(AttributeTargets.Field)]
public class FileExtensionsAttribute : Attribute
{
    public string[] FileExtensions { get; }

    public FileExtensionsAttribute(string[] fileExtensions)
    {
        FileExtensions = fileExtensions;
    }
}

public record PersonToUpdate(int Id, string Name, string ShortName);
public record LocationToUpdate(int Id, string Name, string ShortName);
public record TagToUpdate(int Id, string Name, string ShortName);

public enum UpdateHistoryType
{
    TogglePerson,
    ToggleLocation,
    ToggleTag,
}

public class UpdateHistoryItem
{
    public required UpdateHistoryType Type { get; init; }
    public required int ItemId { get; init; }
    public required string ShortItemName { get; init; }
    public required string ItemName { get; init; }
    public required int FunctionKey { get; init; }
    public string ToggleText => $"F{FunctionKey}: Toggle '{ShortItemName}'";
}

public partial class FindViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowUpdateSection))]
    private bool maximize = false;

    partial void OnMaximizeChanged(bool value)
    {
        Events.Send(new FullscreenBrowsingRequested(value));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverlayFontSize))]
    private bool largeTextMode = false;

    public int OverlayFontSize => LargeTextMode ? configRepository.Config.OverlayTextSizeLarge : configRepository.Config.OverlayTextSize;

    public bool ShowUpdateSection => !Maximize && ReadWriteMode;

    #region Meta-data change commands and properties

    [ObservableProperty]
    private string? newFileDescription;

    [ObservableProperty]
    private string? newFileDateTime;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowUpdateSection))]
    private bool readWriteMode;

    #endregion

    #region Current file properties

    public bool FileSelected => SelectedFile != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileSelected))]
    private FilesModel? selectedFile;

    [ObservableProperty]
    private string currentFileInternalPath = string.Empty;

    [ObservableProperty]
    private string currentFileInternalDirectoryPath = string.Empty;

    [ObservableProperty]
    private string currentFilePath = string.Empty;

    [ObservableProperty]
    private string currentFileDescription = string.Empty;

    [ObservableProperty]
    private string currentFileDateTime = string.Empty;

    [ObservableProperty]
    private string currentFilePosition = string.Empty;

    [ObservableProperty]
    private Uri? currentFilePositionLink;

    [ObservableProperty]
    private string currentFilePersons = string.Empty;

    [ObservableProperty]
    private string currentFileLocations = string.Empty;

    [ObservableProperty]
    private string currentFileTags = string.Empty;

    [ObservableProperty]
    private string currentFileLoadError = string.Empty;

    private BitmapImage? currentFileImage = null;

    private int currentFileRotation = 0;

    private IEnumerable<PersonModel> currentFilePersonList = new List<PersonModel>();
    private IEnumerable<LocationModel> currentFileLocationList = new List<LocationModel>();
    private IEnumerable<TagModel> currentFileTagList = new List<TagModel>();

    #endregion

    public ObservableCollection<PersonToUpdate> Persons { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedPersonCanBeAdded))]
    [NotifyPropertyChangedFor(nameof(SelectedPersonCanBeRemoved))]
    private PersonToUpdate? selectedPersonToUpdate;
    
    public bool SelectedPersonCanBeAdded =>
        SelectedFile != null &&
        SelectedPersonToUpdate != null &&
        !currentFilePersonList.Any(x => x.Id == SelectedPersonToUpdate.Id);

    public bool SelectedPersonCanBeRemoved =>
        SelectedFile != null &&
        SelectedPersonToUpdate != null &&
        currentFilePersonList.Any(x => x.Id == SelectedPersonToUpdate.Id);

    public ObservableCollection<LocationToUpdate> Locations { get; } = new();

    public ObservableCollection<LocationToUpdate> LocationsWithPosition { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedLocationCanBeAdded))]
    [NotifyPropertyChangedFor(nameof(SelectedLocationCanBeRemoved))]
    public LocationToUpdate? selectedLocationToUpdate;

    public bool SelectedLocationCanBeAdded =>
        SelectedFile != null &&
        SelectedLocationToUpdate != null &&
        !currentFileLocationList.Any(x => x.Id == SelectedLocationToUpdate.Id);

    public bool SelectedLocationCanBeRemoved =>
        SelectedFile != null &&
        SelectedLocationToUpdate != null &&
        currentFileLocationList.Any(x => x.Id == SelectedLocationToUpdate.Id);

    public ObservableCollection<TagToUpdate> Tags { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedTagCanBeAdded))]
    [NotifyPropertyChangedFor(nameof(SelectedTagCanBeRemoved))]
    private TagToUpdate? selectedTagToUpdate;

    public bool SelectedTagCanBeAdded =>
        SelectedFile != null &&
        SelectedTagToUpdate != null &&
        !currentFileTagList.Any(x => x.Id == SelectedTagToUpdate.Id);

    public bool SelectedTagCanBeRemoved =>
        SelectedFile != null &&
        SelectedTagToUpdate != null &&
        currentFileTagList.Any(x => x.Id == SelectedTagToUpdate.Id);

    public ObservableCollection<UpdateHistoryItem> UpdateHistoryItems { get; } = new();

    public bool HasUpdateHistory => UpdateHistoryItems.Count > 0;

    private int? prevEditedFileId = null;

    private readonly IConfigRepository configRepository;
    private readonly IDbAccessRepository dbAccessRepository;
    private readonly IFilesystemAccessRepository filesystemAccessRepository;
    private readonly IDialogs dialogs;

    public FindViewModel(IConfigRepository configRepository, IDbAccessRepository dbAccessRepository, IFilesystemAccessRepository filesystemAccessRepository, IDialogs dialogs)
    {
        this.configRepository = configRepository;
        this.dbAccessRepository = dbAccessRepository;
        this.filesystemAccessRepository = filesystemAccessRepository;
        this.dialogs = dialogs;

        ReadWriteMode = !configRepository.Config.ReadOnly;

        ReloadPersons();
        ReloadLocations();
        ReloadTags();

        this.RegisterForEvent<PersonsUpdated>((x) => ReloadPersons());
        this.RegisterForEvent<LocationsUpdated>((x) => ReloadLocations());
        this.RegisterForEvent<TagsUpdated>((x) => ReloadTags());

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            ReadWriteMode = !configRepository.Config.ReadOnly;
            OnPropertyChanged(nameof(LargeTextMode));
        });

        this.RegisterForEvent<SelectSearchResultFile>((x) =>
        {
            LoadFile(x.File);
        });

        this.RegisterForEvent<CloseSearchResultFile>((x) =>
        {
            ResetFile();
        });
    }

    [RelayCommand]
    private void OpenFileLocation()
    {
        if (!string.IsNullOrEmpty(CurrentFilePath) && File.Exists(CurrentFilePath))
        {
            Utils.SelectFileInExplorer(CurrentFilePath);
        }
    }

    [RelayCommand]
    private void OpenFileWithDefaultApp()
    {
        if (!string.IsNullOrEmpty(CurrentFilePath) && File.Exists(CurrentFilePath))
        {
            Utils.OpenFileWithDefaultApp(CurrentFilePath);
        }
    }

    [RelayCommand]
    private void CopyFileId()
    {
        if (SelectedFile != null)
        {
            ClipboardService.SetText(Utils.CreateFileList(new List<FilesModel>() { SelectedFile }));
        }
    }

    [RelayCommand]
    private void RemoveFileFromCurrentSearchResult()
    {
        if (SelectedFile != null)
        {
            Events.Send(new RemoveFileFromSearchResult(SelectedFile));
        }
    }

    private void LoadFile(FilesModel selection)
    {
        SelectedFile = selection;

        currentFilePersonList = dbAccessRepository.DbAccess.GetPersonsFromFile(selection.Id);
        OnPropertyChanged(nameof(SelectedPersonCanBeAdded));
        OnPropertyChanged(nameof(SelectedPersonCanBeRemoved));

        currentFileLocationList = dbAccessRepository.DbAccess.GetLocationsFromFile(selection.Id);
        OnPropertyChanged(nameof(SelectedLocationCanBeAdded));
        OnPropertyChanged(nameof(SelectedLocationCanBeRemoved));

        currentFileTagList = dbAccessRepository.DbAccess.GetTagsFromFile(selection.Id);
        OnPropertyChanged(nameof(SelectedTagCanBeAdded));
        OnPropertyChanged(nameof(SelectedTagCanBeRemoved));

        CurrentFileInternalDirectoryPath = Path.GetDirectoryName(selection.Path)!.Replace(@"\", "/");
        CurrentFileInternalPath = selection.Path;
        CurrentFilePath = filesystemAccessRepository.FilesystemAccess.ToAbsolutePath(selection.Path);
        CurrentFileDescription = selection.Description ?? string.Empty;
        CurrentFileDateTime = GetFileDateTimeString(selection.Datetime);
        CurrentFilePosition = selection.Position != null ? Utils.CreateShortFilePositionString(selection.Position) : string.Empty;
        CurrentFilePositionLink = selection.Position != null ? Utils.CreatePositionUri(selection.Position, configRepository.Config.LocationLink) : null;
        CurrentFilePersons = GetFilePersonsString(selection);
        CurrentFileLocations = GetFileLocationsString(selection.Id);
        CurrentFileTags = GetFileTagsString(selection.Id);

        NewFileDescription = CurrentFileDescription;
        NewFileDateTime = selection.Datetime;

        var fileType = FileTypeUtils.GetFileType(selection.Path);
        if (fileType != FileType.Picture)
        {
            CurrentFileLoadError = "File type not supported.";
            currentFileImage = null;
            Events.Send<CloseImage>();
            return;
        }

        // Note: reading of orientation from Exif is done here to get correct visualization for files added to database before orientation was parsed
        currentFileRotation = DatabaseParsing.OrientationToDegrees(selection.Orientation ?? filesystemAccessRepository.FilesystemAccess.ParseFileMetadata(CurrentFilePath).Orientation);

        var uri = new Uri(CurrentFilePath, UriKind.Absolute);
        try
        {
            CurrentFileLoadError = string.Empty;
            currentFileImage = new BitmapImage(uri);
            Events.Send(new ShowImage(currentFileImage, -currentFileRotation));
        }
        catch (WebException e)
        {
            CurrentFileLoadError = $"Image loading error:\n{e.Message}";
            currentFileImage = null;
            Events.Send<CloseImage>();
        }
        catch (IOException e)
        {
            CurrentFileLoadError = $"Image loading error:\n{e.Message}";
            currentFileImage = null;
            Events.Send<CloseImage>();
        }
        catch (NotSupportedException e)
        {
            CurrentFileLoadError = $"File format not supported:\n{e.Message}";
            currentFileImage = null;
            Events.Send<CloseImage>();
        }
    }

    private void ResetFile()
    {
        SelectedFile = null;

        CurrentFileInternalPath = string.Empty;
        CurrentFileInternalDirectoryPath = string.Empty;
        CurrentFilePath = string.Empty;
        CurrentFileDescription = string.Empty;
        CurrentFileDateTime = string.Empty;
        CurrentFilePosition = string.Empty;
        CurrentFilePositionLink = null;
        CurrentFilePersons = string.Empty;
        CurrentFileLocations = string.Empty;
        CurrentFileTags = string.Empty;
        currentFileRotation = 0;

        NewFileDescription = string.Empty;
        NewFileDateTime = string.Empty;

        CurrentFileLoadError = "No match";
        currentFileImage = null;

        Events.Send<CloseImage>();
    }

    private string GetFileDateTimeString(string? datetimeString)
    {
        var datetime = DatabaseParsing.ParseFilesDatetime(datetimeString);
        if (datetime == null)
        {
            return string.Empty;
        }

        // Note: when no time is available the string is used to avoid including time 00:00
        var resultString = datetimeString!.Contains('T') ? datetime.Value.ToString("yyyy-MM-dd HH:mm") : datetimeString;

        var now = DateTime.Now;
        int yearsAgo = DatabaseUtils.GetAgeInYears(now, datetime.Value);
        if (yearsAgo == 0 && now.Year == datetime.Value.Year)
        {
            resultString = $"{resultString} (this year)";
        }
        else if (yearsAgo <= 1)
        {
            resultString = $"{resultString} ({yearsAgo} year ago)";
        }
        else if (yearsAgo > 1)
        {
            resultString = $"{resultString} ({yearsAgo} years ago)";
        }
        return resultString;
    }

    private string GetFilePersonsString(FilesModel selection)
    {
        var personStrings = currentFilePersonList.Select(p => $"{p.Firstname} {p.Lastname}{Utils.GetPersonAgeInFileString(selection.Datetime, p.DateOfBirth)}");
        return string.Join("\n", personStrings);
    }

    private string GetFileLocationsString(int fileId)
    {
        var locationStrings = currentFileLocationList.Select(l => l.Name);
        return string.Join("\n", locationStrings);
    }

    private string GetFileTagsString(int fileId)
    {
        var tagStrings = currentFileTagList.Select(t => t.Name);
        return string.Join("\n", tagStrings);
    }

    [RelayCommand]
    private void AddFilePerson()
    {
        if (SelectedFile == null)
        {
            dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (SelectedPersonToUpdate == null)
        {
            dialogs.ShowErrorDialog("No person selected");
            return;
        }

        AddFilePersonToCurrentFile(SelectedPersonToUpdate.Id);
    }

    private void AddFilePersonToCurrentFile(int personId)
    {
        var person = dbAccessRepository.DbAccess.GetPersonById(personId);

        if (SelectedFile!.Datetime != null)
        {
            var fileDatetime = DatabaseParsing.ParseFilesDatetime(SelectedFile.Datetime);
            
            if (person.DateOfBirth != null)
            {
                var dateOfBirth = DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth);
                if (fileDatetime < dateOfBirth &&
                    !dialogs.ShowConfirmDialog("Person was not born at the time for this file. Add anyway?"))
                {
                    return;
                }
            }

            if (person.Deceased != null)
            {
                var deceased = DatabaseParsing.ParsePersonDeceasedDate(person.Deceased);
                if (fileDatetime > deceased &&
                    !dialogs.ShowConfirmDialog("Person was deceased at the time for this file. Add anyway?"))
                {
                    return;
                }
            }
        }

        if (!dbAccessRepository.DbAccess.GetPersonsFromFile(SelectedFile.Id).Any(p => p.Id == personId))
        {
            dbAccessRepository.DbAccess.InsertFilePerson(SelectedFile.Id, personId);
            LoadFile(SelectedFile);
            AddUpdateHistoryItem(UpdateHistoryType.TogglePerson, personId, $"{person.Firstname} {person.Lastname}");
            prevEditedFileId = SelectedFile.Id;
        }
    }

    [RelayCommand]
    private void RemoveFilePerson()
    {
        if (SelectedFile == null)
        {
            dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (SelectedPersonToUpdate == null)
        {
            dialogs.ShowErrorDialog("No person selected");
            return;
        }

        dbAccessRepository.DbAccess.DeleteFilePerson(SelectedFile.Id, SelectedPersonToUpdate.Id);
        LoadFile(SelectedFile);
        AddUpdateHistoryItem(UpdateHistoryType.TogglePerson, SelectedPersonToUpdate.Id, SelectedPersonToUpdate.Name);
        prevEditedFileId = SelectedFile.Id;
    }

    [RelayCommand]
    private void AddFileLocation()
    {
        if (SelectedFile == null)
        {
            dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (SelectedLocationToUpdate == null)
        {
            dialogs.ShowErrorDialog("No location selected");
            return;
        }

        AddFileLocationToCurrentFile(SelectedLocationToUpdate);
    }

    private void AddFileLocationToCurrentFile(LocationToUpdate location)
    {
        var fileId = SelectedFile.Id;
        if (!dbAccessRepository.DbAccess.GetLocationsFromFile(fileId).Any(l => l.Id == location.Id))
        {
            dbAccessRepository.DbAccess.InsertFileLocation(fileId, location.Id);
            LoadFile(SelectedFile);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, location.Id, location.Name);
            prevEditedFileId = fileId;
        }
    }

    [RelayCommand]
    private void RemoveFileLocation()
    {
        if (SelectedFile == null)
        {
            dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (SelectedLocationToUpdate == null)
        {
            dialogs.ShowErrorDialog("No location selected");
            return;
        }

        var fileId = SelectedFile.Id;
        dbAccessRepository.DbAccess.DeleteFileLocation(fileId, SelectedLocationToUpdate.Id);
        LoadFile(SelectedFile);
        AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, SelectedLocationToUpdate.Id, SelectedLocationToUpdate.Name);
        prevEditedFileId = fileId;
    }

    [RelayCommand]
    private void AddFileTag()
    {
        if (SelectedFile == null)
        {
            dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (SelectedTagToUpdate == null)
        {
            dialogs.ShowErrorDialog("No tag selected");
            return;
        }

        AddFileTagToCurrentFile(SelectedTagToUpdate);
    }

    private void AddFileTagToCurrentFile(TagToUpdate tag)
    {
        var fileId = SelectedFile.Id;
        if (!dbAccessRepository.DbAccess.GetTagsFromFile(fileId).Any(t => t.Id == tag.Id))
        {
            dbAccessRepository.DbAccess.InsertFileTag(fileId, tag.Id);
            LoadFile(SelectedFile);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleTag, tag.Id, tag.Name);
            prevEditedFileId = fileId;
        }
    }

    [RelayCommand]
    private void RemoveFileTag()
    {
        if (SelectedFile == null)
        {
            dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (SelectedTagToUpdate == null)
        {
            dialogs.ShowErrorDialog("No tag selected");
            return;
        }

        var fileId = SelectedFile.Id;
        dbAccessRepository.DbAccess.DeleteFileTag(fileId, SelectedTagToUpdate.Id);
        LoadFile(SelectedFile);
        AddUpdateHistoryItem(UpdateHistoryType.ToggleTag, SelectedTagToUpdate.Id, SelectedTagToUpdate.Name);
        prevEditedFileId = fileId;
    }

    [RelayCommand]
    private void SetFileDescription()
    {
        if (SelectedFile != null)
        {
            var fileId = SelectedFile.Id;
            NewFileDescription = NewFileDescription?.Trim().ReplaceLineEndings(FilesModelValidator.DescriptionLineEnding);
            var description = string.IsNullOrEmpty(NewFileDescription) ? null : NewFileDescription;

            try
            {
                dbAccessRepository.DbAccess.UpdateFileDescription(fileId, description);
                SelectedFile.Description = description;
                LoadFile(SelectedFile);
                prevEditedFileId = fileId;
            }
            catch (DataValidationException e)
            {
                dialogs.ShowErrorDialog(e.Message);
            }
        }
    }

    [RelayCommand]
    private void SetFileDateTime()
    {
        if (SelectedFile != null)
        {
            var fileId = SelectedFile.Id;
            NewFileDateTime = NewFileDateTime?.Trim();

            var dateTime = string.IsNullOrEmpty(NewFileDateTime) ? null : NewFileDateTime;

            try
            {
                dbAccessRepository.DbAccess.UpdateFileDatetime(fileId, dateTime);
                SelectedFile.Datetime = dateTime;
                LoadFile(SelectedFile);
                prevEditedFileId = fileId;
            }
            catch (DataValidationException e)
            {
                dialogs.ShowErrorDialog(e.Message);
            }
        }
    }

    [RelayCommand]
    private void ReApplyFileMetaData()
    {
        if (SelectedFile == null)
        {
            dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (prevEditedFileId == null)
        {
            dialogs.ShowErrorDialog("Meta-data not edited previously");
            return;
        }

        var fileId = SelectedFile.Id;

        try
        {
            var prevEditedFile = dbAccessRepository.DbAccess.GetFileById(prevEditedFileId.Value)!;
            
            var prevPersons = dbAccessRepository.DbAccess.GetPersonsFromFile(prevEditedFileId.Value);
            var prevLocations = dbAccessRepository.DbAccess.GetLocationsFromFile(prevEditedFileId.Value);
            var prevTags = dbAccessRepository.DbAccess.GetTagsFromFile(prevEditedFileId.Value);

            var persons = dbAccessRepository.DbAccess.GetPersonsFromFile(fileId);
            var locations = dbAccessRepository.DbAccess.GetLocationsFromFile(fileId);
            var tags = dbAccessRepository.DbAccess.GetTagsFromFile(fileId);

            foreach (var person in prevPersons.Where(x => !persons.Select(x => x.Id).Contains(x.Id)))
            {
                dbAccessRepository.DbAccess.InsertFilePerson(fileId, person.Id);
            }
            foreach (var location in prevLocations.Where(x => !locations.Select(x => x.Id).Contains(x.Id)))
            {
                dbAccessRepository.DbAccess.InsertFileLocation(fileId, location.Id);
            }
            foreach (var tag in prevTags.Where(x => !tags.Select(x => x.Id).Contains(x.Id)))
            {
                dbAccessRepository.DbAccess.InsertFileTag(fileId, tag.Id);
            }
            
            dbAccessRepository.DbAccess.UpdateFileDescription(fileId, prevEditedFile.Description);
            SelectedFile.Description = prevEditedFile.Description;

            LoadFile(SelectedFile);
        }
        catch (DataValidationException e)
        {
            dialogs.ShowErrorDialog(e.Message);
        }
    }

    [RelayCommand]
    private void RotateFileClockwise()
    {
        RotateFile(RotationDirection.Clockwise);
    }

    [RelayCommand]
    private void RotateFileCounterClockwise()
    {
        RotateFile(RotationDirection.CounterClockwise);
    }

    private void RotateFile(RotationDirection imageRotationDirection)
    {
        if (SelectedFile != null)
        {
            int cameraNewDegrees = currentFileRotation;
            if (imageRotationDirection == RotationDirection.CounterClockwise)
            {
                cameraNewDegrees += 90;
                if (cameraNewDegrees > 270)
                {
                    cameraNewDegrees = 0;
                }
            }
            else if (imageRotationDirection == RotationDirection.Clockwise)
            {
                cameraNewDegrees -= 90;
                if (cameraNewDegrees < 0)
                {
                    cameraNewDegrees = 270;
                }
            }

            var newOrientation = DatabaseParsing.DegreesToOrientation(cameraNewDegrees);
            dbAccessRepository.DbAccess.UpdateFileOrientation(SelectedFile.Id, newOrientation);
            SelectedFile.Orientation = newOrientation;

            LoadFile(SelectedFile);
        }
    }

    [RelayCommand]
    private void UpdateFileOrientationFromMetaData()
    {
        if (SelectedFile != null)
        {
            if (dialogs.ShowConfirmDialog("Reload orientation from file meta-data?"))
            {
                var fileMetadata = filesystemAccessRepository.FilesystemAccess.ParseFileMetadata(filesystemAccessRepository.FilesystemAccess.ToAbsolutePath(SelectedFile.Path));
                dbAccessRepository.DbAccess.UpdateFileOrientation(SelectedFile.Id, fileMetadata.Orientation);
                SelectedFile.Orientation = fileMetadata.Orientation;
                LoadFile(SelectedFile);
            }
        }
    }

    [RelayCommand]
    private void UpdateFileFromMetaData()
    {
        if (SelectedFile != null)
        {
            if (dialogs.ShowConfirmDialog("Reload date and GPS position from file meta-data?"))
            {
                dbAccessRepository.DbAccess.UpdateFileFromMetaData(SelectedFile.Id, filesystemAccessRepository.FilesystemAccess);

                var updatedFile = dbAccessRepository.DbAccess.GetFileById(SelectedFile.Id)!;
                SelectedFile.Datetime = updatedFile.Datetime;
                SelectedFile.Position = updatedFile.Position;
                SelectedFile.Orientation = updatedFile.Orientation;
                LoadFile(SelectedFile);
            }
        }
    }

    [RelayCommand]
    private void CreatePerson()
    {
        var newPerson = dialogs.ShowAddPersonDialog();
        if (newPerson != null)
        {
            AddFilePersonToCurrentFile(newPerson.Id);
        }
    }

    [RelayCommand]
    private void CreateLocation()
    {
        var newLocation = dialogs.ShowAddLocationDialog();
        if (newLocation != null)
        {
            AddFileLocationToCurrentFile(new LocationToUpdate(newLocation.Id, newLocation.Name, Utils.CreateShortText(newLocation.Name, configRepository.Config.ShortItemNameMaxLength)));
        }
    }

    [RelayCommand]
    private void CreateTag()
    {
        var newTag = dialogs.ShowAddTagDialog();
        if (newTag != null)
        {
            AddFileTagToCurrentFile(new TagToUpdate(newTag.Id, newTag.Name, Utils.CreateShortText(newTag.Name, configRepository.Config.ShortItemNameMaxLength)));
        }
    }

    private void ReloadPersons()
    {
        Persons.Clear();
        var persons = dbAccessRepository.DbAccess.GetPersons().ToList();
        persons.Sort(new PersonModelByNameSorter());
        foreach (var person in persons.Select(p => new PersonToUpdate(p.Id, $"{p.Firstname} {p.Lastname}", Utils.CreateShortText($"{p.Firstname} {p.Lastname}", configRepository.Config.ShortItemNameMaxLength))))
        {
            Persons.Add(person);
        }
    }

    private void ReloadLocations()
    {
        Locations.Clear();
        LocationsWithPosition.Clear();
        
        var locations = dbAccessRepository.DbAccess.GetLocations().ToList();
        locations.Sort(new LocationModelByNameSorter());
        
        foreach (var location in locations)
        {
            var locationToUpdate = new LocationToUpdate(location.Id, location.Name, Utils.CreateShortText(location.Name, configRepository.Config.ShortItemNameMaxLength));
            Locations.Add(locationToUpdate);
            if (location.Position != null)
            {
                LocationsWithPosition.Add(locationToUpdate);
            }
        }
    }

    private void ReloadTags()
    {
        Tags.Clear();
        var tags = dbAccessRepository.DbAccess.GetTags().ToList();
        tags.Sort(new TagModelByNameSorter());
        foreach (var tag in tags.Select(t => new TagToUpdate(t.Id, t.Name, Utils.CreateShortText(t.Name, configRepository.Config.ShortItemNameMaxLength))))
        {
            Tags.Add(tag);
        }
    }

    private void AddUpdateHistoryItem(UpdateHistoryType type, int itemId, string itemName)
    {
        if (UpdateHistoryItems.Count >= 12)
        {
            return;
        }

        var duplicatedItem = UpdateHistoryItems.FirstOrDefault(x => x.Type == type && x.ItemName == itemName);
        if (duplicatedItem != null)
        {
            return;
        }

        for (int i = 1; i <= 12; i++)
        {
            var item = UpdateHistoryItems.FirstOrDefault(x => x.FunctionKey == i);
            if (item == null)
            {
                UpdateHistoryItems.Insert(i - 1, new UpdateHistoryItem()
                {
                    Type = type,
                    ItemId = itemId,
                    ItemName = itemName,
                    FunctionKey = i,
                    ShortItemName = Utils.CreateShortText(itemName, configRepository.Config.ShortItemNameMaxLength),
                });

                OnPropertyChanged(nameof(HasUpdateHistory));
                return;
            }
        }
    }

    [RelayCommand]
    private void FunctionKey(string parameter)
    {
        if (!ReadWriteMode || SelectedFile == null)
        {
            return;
        }

        var functionKey = int.Parse(parameter);
        var historyItem = UpdateHistoryItems.FirstOrDefault(x => x.FunctionKey == functionKey);
        if (historyItem == null)
        {
            return;
        }

        ToggleFromHistoryItem(historyItem);
    }

    [RelayCommand]
    private void ToggleFromHistoryItem(UpdateHistoryItem historyItem)
    {
        if (!ReadWriteMode || SelectedFile == null)
        {
            return;
        }

        var fileId = SelectedFile.Id;

        switch (historyItem.Type)
        {
            case UpdateHistoryType.TogglePerson:
                var personId = historyItem.ItemId;
                if (dbAccessRepository.DbAccess.GetPersonsFromFile(fileId).Any(x => x.Id == personId))
                {
                    dbAccessRepository.DbAccess.DeleteFilePerson(fileId, personId);
                }
                else
                {
                    AddFilePersonToCurrentFile(personId);
                }
                break;

            case UpdateHistoryType.ToggleLocation:
                var locationId = historyItem.ItemId;
                if (dbAccessRepository.DbAccess.GetLocationsFromFile(fileId).Any(x => x.Id == locationId))
                {
                    dbAccessRepository.DbAccess.DeleteFileLocation(fileId, locationId);
                }
                else
                {
                    dbAccessRepository.DbAccess.InsertFileLocation(fileId, locationId);
                }
                break;

            case UpdateHistoryType.ToggleTag:
                var tagId = historyItem.ItemId;
                if (dbAccessRepository.DbAccess.GetTagsFromFile(fileId).Any(x => x.Id == tagId))
                {
                    dbAccessRepository.DbAccess.DeleteFileTag(fileId, tagId);
                }
                else
                {
                    dbAccessRepository.DbAccess.InsertFileTag(fileId, tagId);
                }
                break;
        }

        LoadFile(SelectedFile);
    }

    [RelayCommand]
    private void RemoveHistoryItem(UpdateHistoryItem itemToRemove)
    {
        if (UpdateHistoryItems.Remove(itemToRemove))
        {
            OnPropertyChanged(nameof(HasUpdateHistory));
        }
    }

    [RelayCommand]
    private void OpenPresentationWindow()
    {
        var window = new PresentationWindow()
        {
            Owner = Application.Current.MainWindow,
            Title = $"{Utils.ApplicationName} {Utils.GetVersionString()} - Presentation"
        };

        if (CurrentFilePath != string.Empty && currentFileImage != null)
        {
            window.ShowImage(currentFileImage, -currentFileRotation);
        }

        window.Show();
    }
}
