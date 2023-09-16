using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Extensions;
using FileDB.Model;
using FileDBInterface.Exceptions;
using FileDBShared.Model;
using FileDBShared.Validators;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileDB.ViewModel;

public partial class FileCategorizationViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileSelected))]
    [NotifyPropertyChangedFor(nameof(CanApplyMetaDataFromPrevEdit))]
    private FileModel? selectedFile;

    public bool FileSelected => SelectedFile != null;

    [ObservableProperty]
    private string? newFileDescription;

    [ObservableProperty]
    private string? newFileDateTime;

    [ObservableProperty]
    private bool readWriteMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedPersonCanBeAdded))]
    [NotifyPropertyChangedFor(nameof(SelectedPersonCanBeRemoved))]
    private PersonToUpdate? selectedPersonToUpdate;

    private int imageRotation = 0;

    private IEnumerable<PersonModel> personList = new List<PersonModel>();
    private IEnumerable<LocationModel> locationList = new List<LocationModel>();
    private IEnumerable<TagModel> tagList = new List<TagModel>();

    public bool SelectedPersonCanBeAdded =>
        SelectedFile != null &&
        SelectedPersonToUpdate != null &&
        !personList.Any(x => x.Id == SelectedPersonToUpdate.Id);

    public bool SelectedPersonCanBeRemoved =>
        SelectedFile != null &&
        SelectedPersonToUpdate != null &&
        personList.Any(x => x.Id == SelectedPersonToUpdate.Id);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedLocationCanBeAdded))]
    [NotifyPropertyChangedFor(nameof(SelectedLocationCanBeRemoved))]
    public LocationToUpdate? selectedLocationToUpdate;

    public bool SelectedLocationCanBeAdded =>
        SelectedFile != null &&
        SelectedLocationToUpdate != null &&
        !locationList.Any(x => x.Id == SelectedLocationToUpdate.Id);

    public bool SelectedLocationCanBeRemoved =>
        SelectedFile != null &&
        SelectedLocationToUpdate != null &&
        locationList.Any(x => x.Id == SelectedLocationToUpdate.Id);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedTagCanBeAdded))]
    [NotifyPropertyChangedFor(nameof(SelectedTagCanBeRemoved))]
    private TagToUpdate? selectedTagToUpdate;

    public bool SelectedTagCanBeAdded =>
        SelectedFile != null &&
        SelectedTagToUpdate != null &&
        !tagList.Any(x => x.Id == SelectedTagToUpdate.Id);

    public bool SelectedTagCanBeRemoved =>
        SelectedFile != null &&
        SelectedTagToUpdate != null &&
        tagList.Any(x => x.Id == SelectedTagToUpdate.Id);

    public ObservableCollection<UpdateHistoryItem> UpdateHistoryItems { get; } = new();

    public bool HasUpdateHistory => UpdateHistoryItems.Count > 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApplyMetaDataFromPrevEdit))]
    private int? prevEditedFileId = null;

    public bool CanApplyMetaDataFromPrevEdit => SelectedFile != null && PrevEditedFileId != null;

    public ObservableCollection<PersonToUpdate> Persons { get; } = new();
    public ObservableCollection<LocationToUpdate> Locations { get; } = new();
    public ObservableCollection<TagToUpdate> Tags { get; } = new();

    private readonly IConfigRepository configRepository;
    private readonly IDbAccessRepository dbAccessRepository;
    private readonly IDialogs dialogs;
    private readonly IFilesystemAccessRepository filesystemAccessRepository;
    private readonly IPersonsRepository personsRepository;
    private readonly ILocationsRepository locationsRepository;
    private readonly ITagsRepository tagsRepository;

    public FileCategorizationViewModel(IConfigRepository configRepository, IDbAccessRepository dbAccessRepository, IDialogs dialogs, IFilesystemAccessRepository filesystemAccessRepository, IPersonsRepository personsRepository, ILocationsRepository locationsRepository, ITagsRepository tagsRepository)
    {
        this.configRepository = configRepository;
        this.dbAccessRepository = dbAccessRepository;
        this.dialogs = dialogs;
        this.filesystemAccessRepository = filesystemAccessRepository;
        this.personsRepository = personsRepository;
        this.locationsRepository = locationsRepository;
        this.tagsRepository = tagsRepository;

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
        });

        this.RegisterForEvent<SelectSearchResultFile>((x) =>
        {
            LoadFile(x.File);
        });

        this.RegisterForEvent<CloseSearchResultFile>((x) =>
        {
            CloseFile();
        });

        this.RegisterForEvent<CategorizationFunctionKeyPressed>((x) =>
        {
            FunctionKey(x.FunctionKey);
        });
    }

    private void ReloadPersons()
    {
        Persons.Clear();
        foreach (var p in personsRepository.Persons)
        {
            Persons.Add(new PersonToUpdate(p.Id, $"{p.Firstname} {p.Lastname}", Utils.CreateShortText($"{p.Firstname} {p.Lastname}", configRepository.Config.ShortItemNameMaxLength)));
        }
    }

    private void ReloadLocations()
    {
        Locations.Clear();
        foreach (var location in locationsRepository.Locations)
        {
            var locationToUpdate = new LocationToUpdate(location.Id, location.Name, Utils.CreateShortText(location.Name, configRepository.Config.ShortItemNameMaxLength));
            Locations.Add(locationToUpdate);
        }
    }

    private void ReloadTags()
    {
        Tags.Clear();
        foreach (var tag in tagsRepository.Tags.Select(t => new TagToUpdate(t.Id, t.Name, Utils.CreateShortText(t.Name, configRepository.Config.ShortItemNameMaxLength))))
        {
            Tags.Add(tag);
        }
    }

    private void LoadFile(FileModel file)
    {
        SelectedFile = file;

        NewFileDescription = SelectedFile.Description;
        NewFileDateTime = SelectedFile.Datetime;

        personList = dbAccessRepository.DbAccess.GetPersonsFromFile(SelectedFile.Id);
        OnPropertyChanged(nameof(SelectedPersonCanBeAdded));
        OnPropertyChanged(nameof(SelectedPersonCanBeRemoved));

        locationList = dbAccessRepository.DbAccess.GetLocationsFromFile(SelectedFile.Id);
        OnPropertyChanged(nameof(SelectedLocationCanBeAdded));
        OnPropertyChanged(nameof(SelectedLocationCanBeRemoved));

        tagList = dbAccessRepository.DbAccess.GetTagsFromFile(SelectedFile.Id);
        OnPropertyChanged(nameof(SelectedTagCanBeAdded));
        OnPropertyChanged(nameof(SelectedTagCanBeRemoved));

        imageRotation = DatabaseParsing.OrientationToDegrees(SelectedFile.Orientation ?? 0);
    }

    private void CloseFile()
    {
        SelectedFile = null;
        NewFileDescription = string.Empty;
        NewFileDateTime = string.Empty;
        imageRotation = 0;
    }

    [RelayCommand]
    private void SetFileDescription()
    {
        if (SelectedFile != null)
        {
            NewFileDescription = NewFileDescription?.Trim().ReplaceLineEndings(FileModelValidator.DescriptionLineEnding);
            var description = NewFileDescription.HasContent() ? NewFileDescription : null;

            try
            {
                dbAccessRepository.DbAccess.UpdateFileDescription(SelectedFile.Id, description);
                SelectedFile.Description = description;
                SetEditedFile();
            }
            catch (DataValidationException e)
            {
                dialogs.ShowErrorDialog(e.Message);
            }
        }
    }

    private void SetEditedFile()
    {
        PrevEditedFileId = SelectedFile!.Id;
        Events.Send<FileEdited>();
    }

    [RelayCommand]
    private void SetFileDateTime()
    {
        if (SelectedFile != null)
        {
            NewFileDateTime = NewFileDateTime?.Trim();

            var dateTime = NewFileDateTime.HasContent() ? NewFileDateTime : null;

            try
            {
                dbAccessRepository.DbAccess.UpdateFileDatetime(SelectedFile.Id, dateTime);
                SelectedFile.Datetime = dateTime;
                SetEditedFile();
            }
            catch (DataValidationException e)
            {
                dialogs.ShowErrorDialog(e.Message);
            }
        }
    }

    [RelayCommand]
    private void AddMetaDataFromPrevEditedFile()
    {
        if (SelectedFile == null || PrevEditedFileId == null)
        {
            return;
        }

        var fileId = SelectedFile.Id;

        try
        {
            var prevEditedFile = dbAccessRepository.DbAccess.GetFileById(PrevEditedFileId.Value)!;

            var prevPersons = dbAccessRepository.DbAccess.GetPersonsFromFile(PrevEditedFileId.Value);
            var prevLocations = dbAccessRepository.DbAccess.GetLocationsFromFile(PrevEditedFileId.Value);
            var prevTags = dbAccessRepository.DbAccess.GetTagsFromFile(PrevEditedFileId.Value);

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

            Events.Send<FileEdited>();
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
            int cameraNewDegrees = imageRotation;
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

            Events.Send<FileEdited>();
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
                Events.Send<FileEdited>();
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
                Events.Send<FileEdited>();
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

    [RelayCommand]
    private void AddFilePerson()
    {
        if (SelectedFile != null && SelectedPersonToUpdate != null)
        {
            AddFilePersonToCurrentFile(SelectedPersonToUpdate.Id);
        }
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
            AddUpdateHistoryItem(UpdateHistoryType.TogglePerson, personId, $"{person.Firstname} {person.Lastname}");
            SetEditedFile();
        }
    }

    [RelayCommand]
    private void RemoveFilePerson()
    {
        if (SelectedFile != null && SelectedPersonToUpdate != null)
        {
            dbAccessRepository.DbAccess.DeleteFilePerson(SelectedFile.Id, SelectedPersonToUpdate.Id);
            AddUpdateHistoryItem(UpdateHistoryType.TogglePerson, SelectedPersonToUpdate.Id, SelectedPersonToUpdate.Name);
            SetEditedFile();
        }
    }

    [RelayCommand]
    private void AddFileLocation()
    {
        if (SelectedFile != null && SelectedLocationToUpdate != null)
        {
            AddFileLocationToCurrentFile(SelectedLocationToUpdate);
        }
    }

    private void AddFileLocationToCurrentFile(LocationToUpdate location)
    {
        if (SelectedFile != null)
        {
            var fileId = SelectedFile.Id;
            if (!dbAccessRepository.DbAccess.GetLocationsFromFile(fileId).Any(l => l.Id == location.Id))
            {
                dbAccessRepository.DbAccess.InsertFileLocation(fileId, location.Id);
                AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, location.Id, location.Name);
                SetEditedFile();
            }
        }
    }

    [RelayCommand]
    private void RemoveFileLocation()
    {
        if (SelectedFile != null && SelectedLocationToUpdate != null)
        {
            var fileId = SelectedFile.Id;
            dbAccessRepository.DbAccess.DeleteFileLocation(fileId, SelectedLocationToUpdate.Id);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, SelectedLocationToUpdate.Id, SelectedLocationToUpdate.Name);
            SetEditedFile();
        }
    }

    [RelayCommand]
    private void AddFileTag()
    {
        if (SelectedFile != null && SelectedTagToUpdate != null)
        {
            AddFileTagToCurrentFile(SelectedTagToUpdate);
        }
    }

    private void AddFileTagToCurrentFile(TagToUpdate tag)
    {
        var fileId = SelectedFile!.Id;
        if (!dbAccessRepository.DbAccess.GetTagsFromFile(fileId).Any(t => t.Id == tag.Id))
        {
            dbAccessRepository.DbAccess.InsertFileTag(fileId, tag.Id);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleTag, tag.Id, tag.Name);
            SetEditedFile();
        }
    }

    [RelayCommand]
    private void RemoveFileTag()
    {
        if (SelectedFile != null && SelectedTagToUpdate != null)
        {
            var fileId = SelectedFile.Id;
            dbAccessRepository.DbAccess.DeleteFileTag(fileId, SelectedTagToUpdate.Id);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleTag, SelectedTagToUpdate.Id, SelectedTagToUpdate.Name);
            SetEditedFile();
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

    private void FunctionKey(int functionKey)
    {
        if (!ReadWriteMode || SelectedFile == null)
        {
            return;
        }

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

        Events.Send<FileEdited>();
    }

    [RelayCommand]
    private void RemoveHistoryItem(UpdateHistoryItem itemToRemove)
    {
        if (UpdateHistoryItems.Remove(itemToRemove))
        {
            OnPropertyChanged(nameof(HasUpdateHistory));
        }
    }
}
