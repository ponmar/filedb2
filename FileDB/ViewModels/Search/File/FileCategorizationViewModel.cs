using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.Lang;
using FileDB.Model;
using FileDB.Validators;
using FileDBInterface.Exceptions;
using FileDBInterface.Extensions;
using FileDBInterface.Model;
using FileDBInterface.Validators;

namespace FileDB.ViewModels.Search.File;

public enum RotationDirection { Clockwise, CounterClockwise }

public partial class FileCategorizationViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileSelected))]
    [NotifyPropertyChangedFor(nameof(CanApplyMetaDataFromPrevEdit))]
    [NotifyPropertyChangedFor(nameof(CanMarkCurrentFileAsPrevEdited))]
    private FileModel? selectedFile;

    public bool FileSelected => SelectedFile is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    private bool isExpanded = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HistoryHeader))]
    private bool historyIsExpanded = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatePersonsHeader))]
    private bool updatePersonsIsExpanded = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdateLocationsHeader))]
    private bool updateLocationsIsExpanded = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdateTagsHeader))]
    private bool updateTagsIsExpanded = false;

    public string Header => IsExpanded ? Strings.CategorizationUpdateTitle : char.ConvertFromUtf32(0x1F589);
    public string HistoryHeader => HistoryIsExpanded ? Strings.CategorizationUpdateHistoryTitle : char.ConvertFromUtf32(0x1F4DC);
    public string UpdatePersonsHeader => UpdatePersonsIsExpanded ? string.Format(Strings.CategorizationUpdatePersonsTitle, Persons.Count(x => x.IsVisible), Persons.Count) : char.ConvertFromUtf32(0x1F6B6);
    public string UpdateLocationsHeader => UpdateLocationsIsExpanded ? string.Format(Strings.CategorizationUpdateLocationsTitle, Locations.Count(x => x.IsVisible), Locations.Count) : char.ConvertFromUtf32(0x1F3E0);
    public string UpdateTagsHeader => UpdateTagsIsExpanded ? string.Format(Strings.CategorizationUpdateTagsTitle, Tags.Count(x => x.IsVisible), Tags.Count) : char.ConvertFromUtf32(0x1F516);

    [ObservableProperty]
    private string newFileDescription = string.Empty;

    partial void OnNewFileDescriptionChanged(string value)
    {
        IsDirty = true;
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [IsDateAndTime(ErrorMessage = "Format error")]
    private string newFileDateTime = string.Empty;

    partial void OnNewFileDateTimeChanged(string value)
    {
        IsDirty = true;
        
        // Notify that CanSave might have changed due to validation
        OnPropertyChanged(nameof(CanSave));
    }

    public bool CanSave => !HasErrors && IsDirty;

    [ObservableProperty]
    private string filePosition = string.Empty;

    partial void OnFilePositionChanged(string value)
    {
        IsDirty = true;
    }

    [ObservableProperty]
    private bool readWriteMode;

    [ObservableProperty]
    private int imageRotation = 0;

    public ObservableCollection<UpdateHistoryItemViewModel> UpdateHistoryItems { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApplyMetaDataFromPrevEdit))]
    [NotifyPropertyChangedFor(nameof(CanMarkCurrentFileAsPrevEdited))]
    private int? prevEditedFileId = null;

    public bool CanApplyMetaDataFromPrevEdit => SelectedFile is not null && PrevEditedFileId is not null && SelectedFile.Id != PrevEditedFileId;

    public bool CanMarkCurrentFileAsPrevEdited => SelectedFile is not null && SelectedFile.Id != PrevEditedFileId;

    public ObservableCollection<TogglePersonViewModel> Persons { get; } = [];
    public ObservableCollection<ToggleLocationViewModel> Locations { get; } = [];
    public ObservableCollection<ToggleTagViewModel> Tags { get; } = [];

    public bool HasVisiblePersons => Persons.Any(x => x.IsVisible);
    public bool HasVisibleLocations => Locations.Any(x => x.IsVisible);
    public bool HasVisibleTags => Tags.Any(x => x.IsVisible);

    [ObservableProperty]
    private string personsFilterText = string.Empty;

    partial void OnPersonsFilterTextChanged(string value)
    {
        foreach (var person in Persons)
        {
            person.ApplyFilter(value);
        }
        OnPropertyChanged(nameof(HasVisiblePersons));
        OnPropertyChanged(nameof(UpdatePersonsHeader));
    }

    [ObservableProperty]
    private string locationsFilterText = string.Empty;

    partial void OnLocationsFilterTextChanged(string value)
    {
        foreach (var location in Locations)
        {
            location.ApplyFilter(value);
        }
        OnPropertyChanged(nameof(HasVisibleLocations));
        OnPropertyChanged(nameof(UpdateLocationsHeader));
    }

    [ObservableProperty]
    private string tagsFilterText = string.Empty;

    partial void OnTagsFilterTextChanged(string value)
    {
        foreach (var tag in Tags)
        {
            tag.ApplyFilter(value);
        }
        OnPropertyChanged(nameof(HasVisibleTags));
        OnPropertyChanged(nameof(UpdateTagsHeader));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private bool isDirty;

    private readonly IConfigProvider configProvider;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IPersonsRepository personsRepository;
    private readonly ILocationsRepository locationsRepository;
    private readonly ITagsRepository tagsRepository;
    private readonly IFileSelector fileSelector;

    public FileCategorizationViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IDialogs dialogs, IFilesystemAccessProvider filesystemAccessProvider, IPersonsRepository personsRepository, ILocationsRepository locationsRepository, ITagsRepository tagsRepository, IFileSelector fileSelector)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.dialogs = dialogs;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.personsRepository = personsRepository;
        this.locationsRepository = locationsRepository;
        this.tagsRepository = tagsRepository;
        this.fileSelector = fileSelector;

        ReadWriteMode = !configProvider.Config.ReadOnly;

        ReloadPersons();
        ReloadLocations();
        ReloadTags();

        this.RegisterForEvent<PersonsUpdated>((x) => ReloadPersons());
        this.RegisterForEvent<LocationsUpdated>((x) => ReloadLocations());
        this.RegisterForEvent<TagsUpdated>((x) => ReloadTags());

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            ReadWriteMode = !configProvider.Config.ReadOnly;
        });

        this.RegisterForEvent<FileSelectionChanged>((x) =>
        {
            if (fileSelector.SelectedFile is null)
            {
                CloseFile();
            }
            else
            {
                LoadFile(fileSelector.SelectedFile);
            }
        });

        this.RegisterForEvent<CategorizationFunctionKeyPressed>(async (x) =>
        {
            await FunctionKeyAsync(x.FunctionKey);
        });

        ValidateAllProperties();
    }

    private void ReloadPersons()
    {
        var personsInSelectedFile = SelectedFile is null ? [] : dbAccessProvider.DbAccess.GetPersonsFromFile(SelectedFile.Id);

        Persons.Clear();
        foreach (var person in personsRepository.Persons)
        {
            var personToUpdate = new TogglePersonViewModel(person, configProvider)
            {
                IsChecked = personsInSelectedFile.Any(per => per.Id == person.Id),
            };
            personToUpdate.ApplyFilter(PersonsFilterText);
            Persons.Add(personToUpdate);
        }
        OnPropertyChanged(nameof(HasVisiblePersons));
        OnPropertyChanged(nameof(UpdatePersonsHeader));
    }

    private void ReloadLocations()
    {
        var locationsInSelectedFile = SelectedFile is null ? [] : dbAccessProvider.DbAccess.GetLocationsFromFile(SelectedFile.Id);

        Locations.Clear();
        foreach (var location in locationsRepository.Locations)
        {
            var locationToUpdate = new ToggleLocationViewModel(location, configProvider)
            {
                IsChecked = locationsInSelectedFile.Any(l => l.Id == location.Id),
            };
            locationToUpdate.ApplyFilter(LocationsFilterText);
            Locations.Add(locationToUpdate);
        }
        OnPropertyChanged(nameof(HasVisibleLocations));
        OnPropertyChanged(nameof(UpdateLocationsHeader));
    }

    private void ReloadTags()
    {
        var tagsInSelectedFile = SelectedFile is null ? [] : dbAccessProvider.DbAccess.GetTagsFromFile(SelectedFile.Id);

        Tags.Clear();
        foreach (var tag in tagsRepository.Tags)
        {
            var tagToUpdate = new ToggleTagViewModel(tag, configProvider)
            {
                IsChecked = tagsInSelectedFile.Any(t => t.Id == tag.Id),
            };
            tagToUpdate.ApplyFilter(TagsFilterText);
            Tags.Add(tagToUpdate);
        }
        OnPropertyChanged(nameof(HasVisibleTags));
        OnPropertyChanged(nameof(UpdateTagsHeader));
    }

    private void LoadFile(FileModel file)
    {
        SelectedFile = file;

        NewFileDescription = SelectedFile.Description ?? string.Empty;
        NewFileDateTime = SelectedFile.Datetime ?? string.Empty;
        FilePosition = FileTextOverlayCreator.GetShortPositionText(SelectedFile) ?? string.Empty;

        var filePersons = dbAccessProvider.DbAccess.GetPersonsFromFile(SelectedFile.Id);
        foreach (var person in Persons)
        {
            person.IsChecked = filePersons.Any(p => p.Id == person.Id);
        }

        var fileLocations = dbAccessProvider.DbAccess.GetLocationsFromFile(SelectedFile.Id);
        foreach (var location in Locations)
        {
            location.IsChecked = fileLocations.Any(l => l.Id == location.Id);
        }

        var fileTags = dbAccessProvider.DbAccess.GetTagsFromFile(SelectedFile.Id);
        foreach (var tag in Tags)
        {
            tag.IsChecked = fileTags.Any(t => t.Id == tag.Id);
        }

        ImageRotation = DatabaseParsing.OrientationToDegrees(SelectedFile.Orientation ?? 0);
        IsDirty = false;
    }

    private void CloseFile()
    {
        SelectedFile = null;
        NewFileDescription = string.Empty;
        NewFileDateTime = string.Empty;
        FilePosition = string.Empty;
        ImageRotation = 0;
        IsDirty = false;
    }

    private void SetEditedFile()
    {
        PrevEditedFileId = SelectedFile!.Id;
        Messenger.Send<FileEdited>();
    }

    [RelayCommand]
    private async Task AddMetaDataFromPrevEditedFileAsync()
    {
        if (SelectedFile is null || PrevEditedFileId is null)
        {
            return;
        }

        var fileId = SelectedFile.Id;

        try
        {
            var prevEditedFile = dbAccessProvider.DbAccess.GetFileById(PrevEditedFileId.Value)!;

            var prevPersons = dbAccessProvider.DbAccess.GetPersonsFromFile(PrevEditedFileId.Value);
            var prevLocations = dbAccessProvider.DbAccess.GetLocationsFromFile(PrevEditedFileId.Value);
            var prevTags = dbAccessProvider.DbAccess.GetTagsFromFile(PrevEditedFileId.Value);

            var persons = dbAccessProvider.DbAccess.GetPersonsFromFile(fileId);
            var locations = dbAccessProvider.DbAccess.GetLocationsFromFile(fileId);
            var tags = dbAccessProvider.DbAccess.GetTagsFromFile(fileId);

            foreach (var person in prevPersons.Where(x => !persons.Select(x => x.Id).Contains(x.Id)))
            {
                dbAccessProvider.DbAccess.InsertFilePerson(fileId, person.Id);
            }
            foreach (var location in prevLocations.Where(x => !locations.Select(x => x.Id).Contains(x.Id)))
            {
                dbAccessProvider.DbAccess.InsertFileLocation(fileId, location.Id);
            }
            foreach (var tag in prevTags.Where(x => !tags.Select(x => x.Id).Contains(x.Id)))
            {
                dbAccessProvider.DbAccess.InsertFileTag(fileId, tag.Id);
            }

            dbAccessProvider.DbAccess.UpdateFileDescription(fileId, prevEditedFile.Description);
            SelectedFile.Description = prevEditedFile.Description;

            Messenger.Send<FileEdited>();
        }
        catch (DataValidationException e)
        {
            await dialogs.ShowErrorDialogAsync(e.Message);
        }
    }

    [RelayCommand]
    private void MarkPrevEditedFile()
    {
        if (SelectedFile is not null)
        {
            PrevEditedFileId = SelectedFile.Id;
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
        if (SelectedFile is not null)
        {
            int cameraNewDegrees = ImageRotation;
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
            dbAccessProvider.DbAccess.UpdateFileOrientation(SelectedFile.Id, newOrientation);
            SelectedFile.Orientation = newOrientation;

            Messenger.Send<FileEdited>();
        }
    }

    [RelayCommand]
    private async Task CreatePersonAsync()
    {
        var newPerson = await dialogs.ShowAddPersonDialogAsync();
        if (newPerson is not null && SelectedFile is not null)
        {
            await AddFilePersonToCurrentFileAsync(newPerson.Id);
        }
    }

    [RelayCommand]
    private async Task CreateLocationAsync()
    {
        var newLocation = await dialogs.ShowAddLocationDialogAsync();
        if (newLocation is not null && SelectedFile is not null)
        {
            AddFileLocationToCurrentFile(newLocation.Id);
        }
    }

    [RelayCommand]
    private async Task CreateTagAsync()
    {
        var newTag = await dialogs.ShowAddTagDialogAsync();
        if (newTag is not null && SelectedFile is not null)
        {
            AddFileTagToCurrentFile(newTag.Id);
        }
    }

    private async Task AddFilePersonToCurrentFileAsync(int personId)
    {
        if (SelectedFile is not null)
        {
            var person = dbAccessProvider.DbAccess.GetPersonById(personId);

            if (SelectedFile.Datetime is not null)
            {
                var fileDatetime = DatabaseParsing.ParseFilesDatetime(SelectedFile.Datetime);

                if (person.DateOfBirth is not null)
                {
                    var dateOfBirth = DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth);
                    if (fileDatetime < dateOfBirth &&
                        !await dialogs.ShowConfirmDialogAsync(string.Format(Strings.FileCetagorizationPersonNotBornInFile, $"{person.Firstname} {person.Lastname}")))
                    {
                        return;
                    }
                }

                if (person.Deceased is not null)
                {
                    var deceased = DatabaseParsing.ParsePersonDeceasedDate(person.Deceased);
                    if (fileDatetime > deceased &&
                        !await dialogs.ShowConfirmDialogAsync(Strings.CategorizationPersonDeceased))
                    {
                        return;
                    }
                }
            }

            if (!dbAccessProvider.DbAccess.GetPersonsFromFile(SelectedFile.Id).Any(p => p.Id == personId))
            {
                dbAccessProvider.DbAccess.InsertFilePerson(SelectedFile.Id, personId);
                Persons.First(p => p.Id == personId).IsChecked = true;
                AddUpdateHistoryItem(UpdateHistoryType.TogglePerson, personId, $"{person.Firstname} {person.Lastname}", true);
                SetEditedFile();
            }
        }
    }

    private void RemoveFilePersonFromCurrentFile(int personId)
    {
        if (SelectedFile is not null)
        {
            dbAccessProvider.DbAccess.DeleteFilePerson(SelectedFile.Id, personId);
            Persons.First(x => x.Id == personId).IsChecked = false;
            var person = dbAccessProvider.DbAccess.GetPersonById(personId);
            AddUpdateHistoryItem(UpdateHistoryType.TogglePerson, person.Id, $"{person.Firstname} {person.Lastname}", false);
            SetEditedFile();
        }
    }

    private void AddFileLocationToCurrentFile(int locationId)
    {
        if (SelectedFile is not null)
        {
            var fileId = SelectedFile.Id;
            if (!dbAccessProvider.DbAccess.GetLocationsFromFile(fileId).Any(l => l.Id == locationId))
            {
                dbAccessProvider.DbAccess.InsertFileLocation(fileId, locationId);
                Locations.First(x => x.Id == locationId).IsChecked = true;
                var location = dbAccessProvider.DbAccess.GetLocationById(locationId);
                AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, location.Id, location.Name, true);
                SetEditedFile();
            }
        }
    }

    private void RemoveFileLocationFromCurrentFile(int locationId)
    {
        if (SelectedFile is not null)
        {
            var fileId = SelectedFile.Id;
            dbAccessProvider.DbAccess.DeleteFileLocation(fileId, locationId);
            Locations.First(x => x.Id == locationId).IsChecked = false;
            var location = dbAccessProvider.DbAccess.GetLocationById(locationId);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, location.Id, location.Name, false);
            SetEditedFile();
        }
    }

    private void AddFileTagToCurrentFile(int tagId)
    {
        if (SelectedFile is not null)
        {
            var fileId = SelectedFile!.Id;
            if (!dbAccessProvider.DbAccess.GetTagsFromFile(fileId).Any(t => t.Id == tagId))
            {
                dbAccessProvider.DbAccess.InsertFileTag(fileId, tagId);
                Tags.First(x => x.Id == tagId).IsChecked = true;
                var tag = dbAccessProvider.DbAccess.GetTagById(tagId);
                AddUpdateHistoryItem(UpdateHistoryType.ToggleTag, tag.Id, tag.Name, true);
                SetEditedFile();
            }
        }
    }

    private void RemoveFileTagFromCurrentFile(int tagId)
    {
        if (SelectedFile is not null)
        {
            var fileId = SelectedFile!.Id;
            dbAccessProvider.DbAccess.DeleteFileTag(fileId, tagId);
            Tags.First(x => x.Id == tagId).IsChecked = false;
            var tag = dbAccessProvider.DbAccess.GetTagById(tagId);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleTag, tag.Id, tag.Name, false);
            SetEditedFile();
        }
    }

    private void AddUpdateHistoryItem(UpdateHistoryType type, int itemId, string itemName, bool itemIncluded)
    {
        var existingItem = UpdateHistoryItems.FirstOrDefault(x => x.Type == type && x.ItemId == itemId);
        if (existingItem is not null)
        {
            existingItem.IsChecked = itemIncluded;
            return;
        }

        var newHistoryItem = new UpdateHistoryItemViewModel()
        {
            Type = type,
            ItemId = itemId,
            ItemName = itemName,
            FunctionKey = -1,
            ShortItemName = Utils.CreateShortText(itemName, configProvider.Config.ShortItemNameMaxLength),
            IsChecked = itemIncluded,
        };

        for (int i = 1; i <= 12; i++)
        {
            if (!UpdateHistoryItems.Any(x => x.FunctionKey == i))
            {
                newHistoryItem.FunctionKey = i;
                UpdateHistoryItems.Insert(i - 1, newHistoryItem);
                return;
            }
        }

        UpdateHistoryItems.Add(newHistoryItem);
    }

    private async Task FunctionKeyAsync(int functionKey)
    {
        if (!ReadWriteMode || SelectedFile is null)
        {
            return;
        }

        var historyItem = UpdateHistoryItems.FirstOrDefault(x => x.FunctionKey == functionKey);
        if (historyItem is not null)
        {
            await ToggleFromHistoryItemAsync(historyItem);
        }
    }

    [RelayCommand]
    private void RemovePersons()
    {
        foreach (var person in Persons.Where(x => x.IsVisible && x.IsChecked))
        {
            RemoveFilePersonFromCurrentFile(person.Id);
        }
    }

    [RelayCommand]
    private async Task AddPersonsAsync()
    {
        foreach (var person in Persons.Where(x => x.IsVisible && !x.IsChecked))
        {
            await AddFilePersonToCurrentFileAsync(person.Id);
        }
    }

    [RelayCommand]
    private async Task TogglePersonAsync(TogglePersonViewModel person)
    {
        if (!ReadWriteMode || SelectedFile is null)
        {
            return;
        }
        if (person.IsChecked)
        {
            RemoveFilePersonFromCurrentFile(person.Id);
        }
        else
        {
            await AddFilePersonToCurrentFileAsync(person.Id);
        }
    }

    [RelayCommand]
    private void RemoveLocations()
    {
        foreach (var location in Locations.Where(x => x.IsVisible && x.IsChecked))
        {
            RemoveFileLocationFromCurrentFile(location.Id);
        }
    }

    [RelayCommand]
    private void AddLocations()
    {
        foreach (var location in Locations.Where(x => x.IsVisible && !x.IsChecked))
        {
            AddFileLocationToCurrentFile(location.Id);
        }
    }

    [RelayCommand]
    private void ToggleLocation(ToggleLocationViewModel location)
    {
        if (!ReadWriteMode || SelectedFile is null)
        {
            return;
        }
        if (location.IsChecked)
        {
            RemoveFileLocationFromCurrentFile(location.Id);
        }
        else
        {
            AddFileLocationToCurrentFile(location.Id);
        }
    }

    [RelayCommand]
    private void RemoveTags()
    {
        foreach (var tag in Tags.Where(x => x.IsVisible && x.IsChecked))
        {
            RemoveFileTagFromCurrentFile(tag.Id);
        }
    }

    [RelayCommand]
    private void AddTags()
    {
        foreach (var tag in Tags.Where(x => x.IsVisible && !x.IsChecked))
        {
            AddFileTagToCurrentFile(tag.Id);
        }
    }

    [RelayCommand]
    private void ToggleTag(ToggleTagViewModel tag)
    {
        if (!ReadWriteMode || SelectedFile is null)
        {
            return;
        }
        if (tag.IsChecked)
        {
            RemoveFileTagFromCurrentFile(tag.Id);
        }
        else
        {
            AddFileTagToCurrentFile(tag.Id);
        }
    }

    [RelayCommand]
    private async Task ToggleFromHistoryItemAsync(UpdateHistoryItemViewModel historyItem)
    {
        if (!ReadWriteMode || SelectedFile is null)
        {
            return;
        }

        var fileId = SelectedFile.Id;

        switch (historyItem.Type)
        {
            case UpdateHistoryType.TogglePerson:
                var personId = historyItem.ItemId;
                if (dbAccessProvider.DbAccess.GetPersonsFromFile(fileId).Any(x => x.Id == personId))
                {
                    RemoveFilePersonFromCurrentFile(personId);
                    historyItem.IsChecked = false;
                }
                else
                {
                    await AddFilePersonToCurrentFileAsync(personId);
                    historyItem.IsChecked = true;
                }
                break;

            case UpdateHistoryType.ToggleLocation:
                var locationId = historyItem.ItemId;
                if (dbAccessProvider.DbAccess.GetLocationsFromFile(fileId).Any(x => x.Id == locationId))
                {
                    RemoveFileLocationFromCurrentFile(locationId);
                    historyItem.IsChecked = true;
                }
                else
                {
                    AddFileLocationToCurrentFile(locationId);
                    historyItem.IsChecked = false;
                }
                break;

            case UpdateHistoryType.ToggleTag:
                var tagId = historyItem.ItemId;
                if (dbAccessProvider.DbAccess.GetTagsFromFile(fileId).Any(x => x.Id == tagId))
                {
                    RemoveFileTagFromCurrentFile(tagId);
                    historyItem.IsChecked = false;
                }
                else
                {
                    AddFileTagToCurrentFile(tagId);
                    historyItem.IsChecked = true;
                }
                break;
        }

        Messenger.Send<FileEdited>();
    }

    [RelayCommand]
    private void RemoveHistoryItem(UpdateHistoryItemViewModel itemToRemove)
    {
        UpdateHistoryItems.Remove(itemToRemove);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedFile is not null)
        {
            try
            {
                NewFileDateTime = NewFileDateTime.Trim();
                var dateTimeToSave = NewFileDateTime.HasContent() ? NewFileDateTime : null;
                dbAccessProvider.DbAccess.UpdateFileDatetime(SelectedFile.Id, dateTimeToSave);
                SelectedFile.Datetime = dateTimeToSave;

                NewFileDescription = NewFileDescription.Trim().ReplaceLineEndings(FileModelValidator.DescriptionLineEnding);
                var descriptionToSave = NewFileDescription.HasContent() ? NewFileDescription : null;
                dbAccessProvider.DbAccess.UpdateFileDescription(SelectedFile.Id, descriptionToSave);
                SelectedFile.Description = descriptionToSave;

                SetEditedFile();
                IsDirty = false;
            }
            catch (DataValidationException e)
            {
                await dialogs.ShowErrorDialogAsync(e.Message);
            }
        }
    }

    [RelayCommand]
    private void Reset()
    {
        if (SelectedFile is not null)
        {
            LoadFile(SelectedFile);
        }
    }

    [RelayCommand]
    private async Task ReloadAsync()
    {
        if (SelectedFile is not null)
        {
            if (await dialogs.ShowConfirmDialogAsync(Strings.CategorizationReloadMetaData))
            {
                dbAccessProvider.DbAccess.UpdateFileFromMetaData(SelectedFile.Id, filesystemAccessProvider.FilesystemAccess);

                var updatedFile = dbAccessProvider.DbAccess.GetFileById(SelectedFile.Id)!;
                SelectedFile.Datetime = updatedFile.Datetime;
                SelectedFile.Position = updatedFile.Position;
                SelectedFile.Orientation = updatedFile.Orientation;
                Messenger.Send<FileEdited>();
            }
        }
    }
}
