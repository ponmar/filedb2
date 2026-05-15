using System;
using System.Collections.Generic;
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
    [NotifyPropertyChangedFor(nameof(CanCategorize))]
    [NotifyPropertyChangedFor(nameof(CanApplyCategorizationFromPrevEdit))]
    [NotifyPropertyChangedFor(nameof(CanMarkCurrentFileAsPrevEdited))]
    [NotifyPropertyChangedFor(nameof(UpdateItemsVisible))]
    public partial FileModel? SelectedFile { get; set; }

    public bool FileSelected => SelectedFile is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MetaDataHeader))]
    [NotifyPropertyChangedFor(nameof(MetaDataHeaderToolTip))]
    public partial bool MetaDataIsExpanded { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HistoryHeader))]
    [NotifyPropertyChangedFor(nameof(HistoryHeaderToolTip))]
    public partial bool HistoryIsExpanded { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CategorizationHeader))]
    [NotifyPropertyChangedFor(nameof(CategorizationHeaderToolTip))]
    public partial bool CategorizationIsExpanded { get; set; } = false;

    public bool UpdateItemsVisible => CanCategorize;

    public string MetaDataHeader => MetaDataIsExpanded ? (IsDirty ? Strings.CategorizationUpdateTitle + " *" : Strings.CategorizationUpdateTitle) : char.ConvertFromUtf32(0x1F589);
    public string? MetaDataHeaderToolTip => MetaDataIsExpanded ? null : Strings.CategorizationUpdateTitle;

    public string HistoryHeader => HistoryIsExpanded ? Strings.CategorizationUpdateHistoryTitle : char.ConvertFromUtf32(0x1F4DC);
    public string? HistoryHeaderToolTip => HistoryIsExpanded ? null : Strings.CategorizationUpdateHistoryTitle;

    public string CategorizationHeader => CategorizationIsExpanded ? string.Format(Strings.CategorizationUpdateCombinedTitle, Items.Count(x => x.IsChecked), Items.Count) : char.ConvertFromUtf32(0x1F4DD);
    public string? CategorizationHeaderToolTip => CategorizationIsExpanded ? null : string.Format(Strings.CategorizationUpdateCombinedTitle, Items.Count(x => x.IsChecked), Items.Count);

    [ObservableProperty]
    public partial string NewFileDescription { get; set; } = string.Empty;

    partial void OnNewFileDescriptionChanged(string value)
    {
        IsDirty = true;
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [IsDateAndTime(ErrorMessage = "Format error")]
    public partial string NewFileDateTime { get; set; } = string.Empty;

    partial void OnNewFileDateTimeChanged(string value)
    {
        IsDirty = true;
        
        // Notify that CanSave might have changed due to validation
        OnPropertyChanged(nameof(CanSave));
    }

    public bool CanSave => !HasErrors && IsDirty;

    [ObservableProperty]
    public partial string FilePosition { get; set; } = string.Empty;

    partial void OnFilePositionChanged(string value)
    {
        IsDirty = true;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCategorize))]
    [NotifyPropertyChangedFor(nameof(UpdateItemsVisible))]
    public partial bool ReadOnly { get; set; }

    public bool CanCategorize => !ReadOnly && FileSelected;

    [ObservableProperty]
    public partial int ImageRotation { get; set; } = 0;
    public ObservableCollection<HistoryItemViewModel> UpdateHistoryItems { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApplyCategorizationFromPrevEdit))]
    [NotifyPropertyChangedFor(nameof(CanMarkCurrentFileAsPrevEdited))]
    public partial int? PrevEditedFileId { get; set; } = null;

    public bool CanApplyCategorizationFromPrevEdit => SelectedFile is not null && PrevEditedFileId is not null && SelectedFile.Id != PrevEditedFileId;

    public bool CanMarkCurrentFileAsPrevEdited => SelectedFile is not null && SelectedFile.Id != PrevEditedFileId;

    public ObservableCollection<ItemViewModel> Items { get; } = [];

    public bool HasVisibleItems => Items.Any(x => x.IsVisible);

    [ObservableProperty]
    public partial string ItemsFilterText { get; set; } = string.Empty;

    partial void OnItemsFilterTextChanged(string value)
    {
        ApplyItemsFilter();
    }

    [ObservableProperty]
    public partial bool ItemsFilterTextCaseSensitive { get; set; }

    partial void OnItemsFilterTextCaseSensitiveChanged(bool value)
    {
        ApplyItemsFilter();
    }

    private void ApplyItemsFilter()
    {
        var itemFilters = GetSearchFiltersFromText(ItemsFilterText);
        foreach (var item in Items)
        {
            item.ApplyFilters(itemFilters, ItemsFilterTextCaseSensitive);
        }
        OnPropertyChanged(nameof(HasVisibleItems));
        OnPropertyChanged(nameof(CategorizationHeader));
        OnPropertyChanged(nameof(CategorizationHeaderToolTip));
    }

    private void PopulateCategorizationItems()
    {
        Items.Clear();

        var personsInSelectedFile = SelectedFile is null ? [] : dbAccessProvider.DbAccess.GetPersonsFromFile(SelectedFile.Id);
        foreach (var person in personsRepository.Persons)
        {
            Items.Add(new ItemViewModel(person, configProvider)
            {
                IsChecked = personsInSelectedFile.Any(x => x.Id == person.Id),
            });
        }

        var locationsInSelectedFile = SelectedFile is null ? [] : dbAccessProvider.DbAccess.GetLocationsFromFile(SelectedFile.Id);
        foreach (var location in locationsRepository.Locations)
        {
            Items.Add(new ItemViewModel(location, configProvider)
            {
                IsChecked = locationsInSelectedFile.Any(x => x.Id == location.Id),
            });
        }

        var tagsInSelectedFile = SelectedFile is null ? [] : dbAccessProvider.DbAccess.GetTagsFromFile(SelectedFile.Id);
        foreach (var tag in tagsRepository.Tags)
        {
            Items.Add(new ItemViewModel(tag, configProvider)
            {
                IsChecked = tagsInSelectedFile.Any(x => x.Id == tag.Id),
            });
        }

        ApplyItemsFilter();
    }

    private void UpdateItemsChecked(CombinedItemType type, int id, bool isChecked)
    {
        var item = Items.FirstOrDefault(x => x.Type == type && x.Id == id);
        item?.IsChecked = isChecked;
        OnPropertyChanged(nameof(HasVisibleItems));
    }

    [RelayCommand]
    private async Task ToggleCombinedAsync(ItemViewModel item)
    {
        if (ReadOnly || SelectedFile is null || item is null)
        {
            return;
        }

        if (item.IsChecked)
        {
            // currently included -> remove
            switch (item.Type)
            {
                case CombinedItemType.Person:
                    RemoveFilePersonFromCurrentFile(item.Id);
                    break;
                case CombinedItemType.Location:
                    RemoveFileLocationFromCurrentFile(item.Id);
                    break;
                case CombinedItemType.Tag:
                    RemoveFileTagFromCurrentFile(item.Id);
                    break;
            }
            item.IsChecked = false;
        }
        else
        {
            // currently not included -> add
            switch (item.Type)
            {
                case CombinedItemType.Person:
                    await AddFilePersonToCurrentFileAsync(item.Id);
                    break;
                case CombinedItemType.Location:
                    AddFileLocationToCurrentFile(item.Id);
                    break;
                case CombinedItemType.Tag:
                    AddFileTagToCurrentFile(item.Id);
                    break;
            }
            item.IsChecked = true;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyPropertyChangedFor(nameof(MetaDataHeader))]
    public partial bool IsDirty { get; set; }

    partial void OnIsDirtyChanged(bool value)
    {
        Messenger.Send(new MetaDataEdited(value));
    }

    private readonly IConfigProvider configProvider;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IPersonsRepository personsRepository;
    private readonly ILocationsRepository locationsRepository;
    private readonly ITagsRepository tagsRepository;
    private readonly IFileSelector fileSelector;
    private readonly IFileRotator fileRotator;

    public FileCategorizationViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IDialogs dialogs, IFilesystemAccessProvider filesystemAccessProvider, IPersonsRepository personsRepository, ILocationsRepository locationsRepository, ITagsRepository tagsRepository, IFileSelector fileSelector, IFileRotator fileRotator)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.dialogs = dialogs;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.personsRepository = personsRepository;
        this.locationsRepository = locationsRepository;
        this.tagsRepository = tagsRepository;
        this.fileSelector = fileSelector;
        this.fileRotator = fileRotator;

        ReadOnly = configProvider.Config.ReadOnly;

        PopulateCategorizationItems();

        this.RegisterForEvent<PersonsUpdated>((x) => PopulateCategorizationItems());
        this.RegisterForEvent<LocationsUpdated>((x) => PopulateCategorizationItems());
        this.RegisterForEvent<TagsUpdated>((x) => PopulateCategorizationItems());

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            ReadOnly = configProvider.Config.ReadOnly;
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

    private void LoadFile(FileModel file)
    {
        SelectedFile = file;

        NewFileDescription = SelectedFile.Description ?? string.Empty;
        NewFileDateTime = SelectedFile.Datetime ?? string.Empty;
        FilePosition = FileTextOverlayCreator.GetShortPositionText(SelectedFile) ?? string.Empty;

        var filePersons = dbAccessProvider.DbAccess.GetPersonsFromFile(SelectedFile.Id);
        var fileLocations = dbAccessProvider.DbAccess.GetLocationsFromFile(SelectedFile.Id);
        var fileTags = dbAccessProvider.DbAccess.GetTagsFromFile(SelectedFile.Id);

        foreach (var item in Items)
        {
            item.IsChecked = item.Type switch
            {
                CombinedItemType.Person => filePersons.Any(p => p.Id == item.Id),
                CombinedItemType.Location => fileLocations.Any(l => l.Id == item.Id),
                CombinedItemType.Tag => fileTags.Any(t => t.Id == item.Id),
                _ => item.IsChecked,
            };
        }
        OnPropertyChanged(nameof(CategorizationHeader));
        OnPropertyChanged(nameof(CategorizationHeaderToolTip));

        foreach (var historyItem in UpdateHistoryItems)
        {
            switch (historyItem.Type)
            {
                case CombinedItemType.Person:
                    historyItem.IsChecked = filePersons.Any(p => p.Id == historyItem.ItemId);
                    break;
                case CombinedItemType.Location:
                    historyItem.IsChecked = fileLocations.Any(l => l.Id == historyItem.ItemId);
                    break;
                case CombinedItemType.Tag:
                    historyItem.IsChecked = fileTags.Any(t => t.Id == historyItem.ItemId);
                    break;
            }
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
        foreach (var historyItem in UpdateHistoryItems)
        {
            historyItem.IsChecked = false;
        }
        IsDirty = false;
    }

    private void SetEditedFile()
    {
        PrevEditedFileId = SelectedFile!.Id;
        Messenger.Send<FileEdited>();
    }

    [RelayCommand]
    private async Task AddCategorizationFromPrevEditedFileAsync()
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
            ImageRotation = fileRotator.Rotate(SelectedFile, ImageRotation, imageRotationDirection);
            Messenger.Send<FileEdited>();
        }
    }

    [RelayCommand]
    private async Task CreatePersonAsync()
    {
        var newPerson = await dialogs.ShowAddPersonDialogAsync();
        if (newPerson is not null && SelectedFile is not null)
        {
            PopulateCategorizationItems();
            await AddFilePersonToCurrentFileAsync(newPerson.Id);
        }
    }

    [RelayCommand]
    private async Task CreateLocationAsync()
    {
        var newLocation = await dialogs.ShowAddLocationDialogAsync();
        if (newLocation is not null && SelectedFile is not null)
        {
            PopulateCategorizationItems();
            AddFileLocationToCurrentFile(newLocation.Id);
        }
    }

    [RelayCommand]
    private async Task CreateTagAsync()
    {
        var newTag = await dialogs.ShowAddTagDialogAsync();
        if (newTag is not null && SelectedFile is not null)
        {
            PopulateCategorizationItems();
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
                        !await dialogs.ShowConfirmDialogAsync(string.Format(Strings.FileCetagorizationPersonNotBornInFile, person.FullName)))
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
            }

            Items.First(p => p.Type == CombinedItemType.Person && p.Id == personId).IsChecked = true;
            OnPropertyChanged(nameof(CategorizationHeader));
            OnPropertyChanged(nameof(CategorizationHeaderToolTip));

            UpdateItemsChecked(CombinedItemType.Person, personId, true);
            AddUpdateHistoryItem(CombinedItemType.Person, personId, person.FullName, true);
            SetEditedFile();
        }
    }

    private void RemoveFilePersonFromCurrentFile(int personId)
    {
        if (SelectedFile is not null)
        {
            dbAccessProvider.DbAccess.DeleteFilePerson(SelectedFile.Id, personId);

            Items.First(x => x.Type == CombinedItemType.Person && x.Id == personId).IsChecked = false;
            OnPropertyChanged(nameof(CategorizationHeader));
            OnPropertyChanged(nameof(CategorizationHeaderToolTip));

            UpdateItemsChecked(CombinedItemType.Person, personId, false);
            var person = dbAccessProvider.DbAccess.GetPersonById(personId);
            AddUpdateHistoryItem(CombinedItemType.Person, person.Id, person.FullName, false);
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
            }

            Items.First(x => x.Type == CombinedItemType.Location && x.Id == locationId).IsChecked = true;
            UpdateItemsChecked(CombinedItemType.Location, locationId, true);
            OnPropertyChanged(nameof(CategorizationHeader));
            OnPropertyChanged(nameof(CategorizationHeaderToolTip));

            var location = dbAccessProvider.DbAccess.GetLocationById(locationId);
            AddUpdateHistoryItem(CombinedItemType.Location, location.Id, location.Name, true);
            SetEditedFile();
        }
    }

    private void RemoveFileLocationFromCurrentFile(int locationId)
    {
        if (SelectedFile is not null)
        {
            var fileId = SelectedFile.Id;
            dbAccessProvider.DbAccess.DeleteFileLocation(fileId, locationId);
            
            Items.First(x => x.Type == CombinedItemType.Location && x.Id == locationId).IsChecked = false;
            UpdateItemsChecked(CombinedItemType.Location, locationId, false);
            OnPropertyChanged(nameof(CategorizationHeader));
            OnPropertyChanged(nameof(CategorizationHeaderToolTip));

            var location = dbAccessProvider.DbAccess.GetLocationById(locationId);
            AddUpdateHistoryItem(CombinedItemType.Location, location.Id, location.Name, false);
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
                }

                Items.First(x => x.Type == CombinedItemType.Tag && x.Id == tagId).IsChecked = true;
                UpdateItemsChecked(CombinedItemType.Tag, tagId, true);
                OnPropertyChanged(nameof(CategorizationHeader));
                OnPropertyChanged(nameof(CategorizationHeaderToolTip));

                var tag = dbAccessProvider.DbAccess.GetTagById(tagId);
                AddUpdateHistoryItem(CombinedItemType.Tag, tag.Id, tag.Name, true);
                SetEditedFile();
        }
    }

    private void RemoveFileTagFromCurrentFile(int tagId)
    {
        if (SelectedFile is not null)
        {
            var fileId = SelectedFile!.Id;
            dbAccessProvider.DbAccess.DeleteFileTag(fileId, tagId);
            
            Items.First(x => x.Type == CombinedItemType.Tag && x.Id == tagId).IsChecked = false;
            UpdateItemsChecked(CombinedItemType.Tag, tagId, false);
            OnPropertyChanged(nameof(CategorizationHeader));
            OnPropertyChanged(nameof(CategorizationHeaderToolTip));

            var tag = dbAccessProvider.DbAccess.GetTagById(tagId);
            AddUpdateHistoryItem(CombinedItemType.Tag, tag.Id, tag.Name, false);
            SetEditedFile();
        }
    }

    private void AddUpdateHistoryItem(CombinedItemType type, int itemId, string itemName, bool itemIncluded)
    {
        var existingItem = UpdateHistoryItems.FirstOrDefault(x => x.Type == type && x.ItemId == itemId);
        if (existingItem is not null)
        {
            existingItem.IsChecked = itemIncluded;
            return;
        }

        var newHistoryItem = new HistoryItemViewModel()
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
        if (ReadOnly || SelectedFile is null)
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
    private void RemoveItems()
    {
        foreach (var item in Items.Where(x => x.IsVisible && x.IsChecked))
        {
            switch (item.Type)
            {
                case CombinedItemType.Person:
                    RemoveFilePersonFromCurrentFile(item.Id);
                    break;
                case CombinedItemType.Location:
                    RemoveFileLocationFromCurrentFile(item.Id);
                    break;
                case CombinedItemType.Tag:
                    RemoveFileTagFromCurrentFile(item.Id);
                    break;
            }
        }
    }

    [RelayCommand]
    private async Task AddItemsAsync()
    {
        foreach (var item in Items.Where(x => x.IsVisible && !x.IsChecked))
        {
            switch (item.Type)
            {
                case CombinedItemType.Person:
                     await AddFilePersonToCurrentFileAsync(item.Id);
                    break;
                case CombinedItemType.Location:
                    AddFileLocationToCurrentFile(item.Id);
                    break;
                case CombinedItemType.Tag:
                    AddFileTagToCurrentFile(item.Id);
                    break;
            }
        }
    }

    [RelayCommand]
    private async Task ToggleFromHistoryItemAsync(HistoryItemViewModel historyItem)
    {
        if (ReadOnly || SelectedFile is null)
        {
            return;
        }

        var fileId = SelectedFile.Id;

        switch (historyItem.Type)
        {
            case CombinedItemType.Person:
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

            case CombinedItemType.Location:
                var locationId = historyItem.ItemId;
                if (dbAccessProvider.DbAccess.GetLocationsFromFile(fileId).Any(x => x.Id == locationId))
                {
                    RemoveFileLocationFromCurrentFile(locationId);
                    historyItem.IsChecked = false;
                }
                else
                {
                    AddFileLocationToCurrentFile(locationId);
                    historyItem.IsChecked = true;
                }
                break;

            case CombinedItemType.Tag:
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
    }

    [RelayCommand]
    private void RemoveHistoryItem(HistoryItemViewModel itemToRemove)
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

    private IEnumerable<string> GetSearchFiltersFromText(string text)
    {
        return text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}
