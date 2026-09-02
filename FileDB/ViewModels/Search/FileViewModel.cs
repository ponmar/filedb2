using FileDBInterface.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO.Abstractions;
using FileDBInterface.Extensions;
using FileDB.Lang;
using FileDB.Model;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using FileDB.Dialogs;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using FileDB.Infrastructure;
using FileDB.Services;

namespace FileDB.ViewModels.Search;

public record Person(PersonModel Model, string Label);
public record Location(LocationModel Model, string Name, string? MapUrl);
public record Tag(TagModel Model, string Name);

public partial class FileViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOpenFileLocationButtonVisible))]
    [NotifyPropertyChangedFor(nameof(IsOpenFileWithDefaultAppButtonVisible))]
    public partial bool Maximize { get; set; } = false;

    partial void OnMaximizeChanged(bool value)
    {
        Messenger.Send(new FullscreenBrowsingRequested(value));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverlayFontSize))]
    [NotifyPropertyChangedFor(nameof(IsOpenFileLocationButtonVisible))]
    [NotifyPropertyChangedFor(nameof(IsOpenFileWithDefaultAppButtonVisible))]
    public partial bool LargeTextMode { get; set; } = false;

    public int OverlayFontSize => LargeTextMode ? configProvider.Config.OverlayTextSizeLarge : configProvider.Config.OverlayTextSize;

    public bool FileSelected => SelectedFile is not null;

    public bool FileHasTime => SelectedFile?.Datetime is not null && SelectedFile.Datetime.Contains('T');

    public bool IsOpenFileLocationButtonVisible => !Maximize && processUtils.IsSelectFileInExplorerSupported();
    public bool IsOpenFileWithDefaultAppButtonVisible => !Maximize && processUtils.IsOpenFileWithDefaultAppSupported();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileSelected))]
    public partial FileModel? SelectedFile { get; set; }

    [ObservableProperty]
    public partial string InternalPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DateTime { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Position { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? PositionLink { get; set; }
    public ObservableCollection<Person> Persons { get; } = [];

    public ObservableCollection<Location> Locations { get; } = [];

    public ObservableCollection<Tag> Tags { get; } = [];

    [ObservableProperty]
    public partial string FileLoadError { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Bitmap? Image { get; set; } = null;

    [ObservableProperty]
    public partial int ImageRotation { get; set; } = 0;

    [ObservableProperty]
    public partial string? TextContent { get; set; } = null;

    [ObservableProperty]
    public partial bool IsPlacingBoundingBox { get; set; } = false;

    [ObservableProperty]
    public partial int PlacingPersonId { get; set; } = -1;

    [ObservableProperty]
    public partial double ImageWidth { get; set; } = 1.0;

    [ObservableProperty]
    public partial double ImageHeight { get; set; } = 1.0;

    [ObservableProperty]
    public partial List<(PersonModel Person, PersonBoundingBox BBox)> FilePersonBoundingBoxes { get; set; } = [];

    private string absolutePath = string.Empty;

    private readonly IConfigProvider configProvider;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IImageLoader imageLoader;
    private readonly IFileSystem fileSystem;
    private readonly IDialogs dialogs;
    private readonly IClipboardService clipboardService;
    private readonly IFileSelector fileSelector;
    private readonly ICriteriaViewModel criteriaViewModel;
    private readonly ISearchResultRepositoryManagement searchResultRepoManagement;
    private readonly IProcessUtils processUtils;

    public FileViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IFilesystemAccessProvider filesystemAccessProvider, IImageLoader imageLoader, IFileSystem fileSystem, IDialogs dialogs, IClipboardService clipboardService, IFileSelector fileSelector, ICriteriaViewModel criteriaViewModel, ISearchResultRepositoryManagement searchResultRepoManagement, IProcessUtils processUtils)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.imageLoader = imageLoader;
        this.fileSystem = fileSystem;
        this.dialogs = dialogs;
        this.clipboardService = clipboardService;
        this.fileSelector = fileSelector;
        this.criteriaViewModel = criteriaViewModel;
        this.searchResultRepoManagement = searchResultRepoManagement;
        this.processUtils = processUtils;

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            OnPropertyChanged(nameof(LargeTextMode));
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
            OnPropertyChanged(nameof(FileHasTime));
        });

        this.RegisterForEvent<ImageLoaded>((x) =>
        {
            try
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    if (x.FilePath == absolutePath)
                    {
                        Image = x.Image;
                        if (x.Image is not null)
                        {
                            ImageWidth = x.Image.PixelSize.Width;
                            ImageHeight = x.Image.PixelSize.Height;
                        }
                    }
                });
            }
            catch (Exception)
            {
                // Ignore: application is shutting down
            }
        });

        this.RegisterForEvent<ImageLoadError>((x) =>
        {
            try
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    if (x.FilePath == absolutePath)
                    {
                        FileLoadError = Strings.SearchImageLoadingError + $"\n{x.Exception.Message}";
                        Image = null;
                    }
                });
            }
            catch (Exception)
            {
                // Ignore: application is shutting down
            }
        });

        this.RegisterForEvent<StartPersonBoundingBoxPlacement>((msg) =>
        {
            if (SelectedFile is not null && SelectedFile.Id == msg.FileId)
            {
                IsPlacingBoundingBox = true;
                PlacingPersonId = msg.PersonId;
            }
        });

        this.RegisterForEvent<PersonBoundingBoxPlacementAborted>((msg) =>
        {
            // Reset placement state when ESC is pressed or placement is otherwise aborted
            IsPlacingBoundingBox = false;
            PlacingPersonId = -1;
        });

        this.RegisterForEvent<FilePersonRemoved>((msg) =>
        {
            if (SelectedFile is not null && SelectedFile.Id == msg.FileId)
            {
                LoadFilePersonBoundingBoxes(msg.FileId);
            }
        });

        this.RegisterForEvent<FileEdited>((msg) =>
        {
            if (SelectedFile is not null)
            {
                LoadFileCategorization(SelectedFile);
                LoadFilePersonBoundingBoxes(SelectedFile.Id);
            }
        });
    }

    [RelayCommand]
    private void OpenFileLocation()
    {
        if (absolutePath.HasContent() && fileSystem.File.Exists(absolutePath))
        {
            processUtils.SelectFileInExplorer(absolutePath);
        }
    }

    [RelayCommand]
    private void OpenFileWithDefaultApp()
    {
        if (absolutePath.HasContent() && fileSystem.File.Exists(absolutePath))
        {
            processUtils.OpenFileWithDefaultApp(absolutePath);
        }
    }

    [RelayCommand]
    private void CopyFileId()
    {
        if (SelectedFile is not null)
        {
            clipboardService.SetTextAsync(Utils.CreateFileList([SelectedFile]));
        }
    }

    [RelayCommand]
    private void RemoveFileFromCurrentSearchResult()
    {
        if (SelectedFile is not null)
        {
            searchResultRepoManagement.RemoveFileFromRepo(SelectedFile);
        }
    }

    private void LoadFile(FileModel selection)
    {
        SelectedFile = selection;

        InternalPath = selection.Path;
        Description = selection.Description ?? string.Empty;
        DateTime = FileTextOverlayCreator.GetFileDateTimeText(selection) ?? string.Empty;
        Position = FileTextOverlayCreator.GetShortPositionText(selection) ?? string.Empty;
        PositionLink = FileTextOverlayCreator.GetPositionUri(configProvider, selection);

        LoadFileCategorization(selection);

        FileLoadError = string.Empty;
        Image = null;
        TextContent = null;
        IsPlacingBoundingBox = false;
        PlacingPersonId = -1;
        FilePersonBoundingBoxes = [];

        absolutePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(selection.Path);
        ImageRotation = -DatabaseParsing.OrientationToDegrees(selection.Orientation ?? 0);

        var fileExtension = System.IO.Path.GetExtension(selection.Path).ToLower();
        var fileType = FileTypeUtils.GetFileType(selection.Path);
        if (fileType == FileType.Picture)
        {
            imageLoader.LoadImage(absolutePath);
            LoadFilePersonBoundingBoxes(selection.Id);
        }
        else if (fileType == FileType.Document && (fileExtension == ".txt" || fileExtension == ".md"))
        {
            try
            {
                TextContent = fileSystem.File.ReadAllText(absolutePath);
            }
            catch (Exception e)
            {
                FileLoadError = Strings.SearchTextLoadingError + $"\n{e.Message}";
            }
        }
        else
        {
            FileLoadError = Strings.SearchFileTypeNotSupported;
        }
    }

    private void LoadFileCategorization(FileModel file)
    {
        Persons.Clear();
        FileTextOverlayCreator.GetPersons(dbAccessProvider.DbAccess, file).ToList().ForEach(Persons.Add);

        Locations.Clear();
        FileTextOverlayCreator.GetLocations(configProvider, dbAccessProvider.DbAccess, file).ToList().ForEach(Locations.Add);

        Tags.Clear();
        FileTextOverlayCreator.GetTags(dbAccessProvider.DbAccess, file).ToList().ForEach(Tags.Add);
    }

    private void CloseFile()
    {
        SelectedFile = null;

        InternalPath = string.Empty;
        Description = string.Empty;
        DateTime = string.Empty;
        Position = string.Empty;
        PositionLink = null;
        Persons.Clear();
        Locations.Clear();
        Tags.Clear();

        FileLoadError = Strings.SearchNoMatch;
        Image = null;
        TextContent = null;
        ImageRotation = 0;
        absolutePath = string.Empty;
        IsPlacingBoundingBox = false;
        PlacingPersonId = -1;
        FilePersonBoundingBoxes = [];
        ImageWidth = 1.0;
        ImageHeight = 1.0;
    }

    public virtual void LoadFilePersonBoundingBoxes(int fileId)
    {
        var bboxes = dbAccessProvider.DbAccess.GetFilePersonBoundingBoxes(fileId);
        var newList = new List<(PersonModel Person, PersonBoundingBox BBox)>();
        foreach (var (personId, bbox) in bboxes)
        {
            var person = dbAccessProvider.DbAccess.GetPersonById(personId);
            newList.Add((person, bbox));
        }
        FilePersonBoundingBoxes = newList;
    }

    [RelayCommand]
    private void OpenPresentationWindow()
    {
        dialogs.ShowPresentationWindow(Strings.PresentationWindowTitle);
    }

    [RelayCommand]
    private void OpenUrl(string url) => processUtils.OpenUriInBrowser(url);

    [RelayCommand]
    private static void PrevFile() => Messenger.Send<SelectPrevFile>();

    [RelayCommand]
    private static void NextFile() => Messenger.Send<SelectNextFile>();

    [RelayCommand]
    private static void FirstFile() => Messenger.Send<SelectFirstFile>();

    [RelayCommand]
    private static void LastFile() => Messenger.Send<SelectLastFile>();

    [RelayCommand]
    private static void NextDirectory() => Messenger.Send<SelectFileInNextDirectory>();

    [RelayCommand]
    private static void PrevDirectory() => Messenger.Send<SelectFileInPrevDirectory>();

    [RelayCommand]
    private void SearchForPerson(PersonModel person) => criteriaViewModel.SearchForPersonAsync(person);

    [RelayCommand]
    private void SearchForPersonGroup() => criteriaViewModel.SearchForPersonGroupAsync(Persons.Select(x => x.Model));

    [RelayCommand]
    private void SearchForDate()
    {
        var date = DatabaseParsing.ParseFilesDatetime(SelectedFile!.Datetime)!.Value.Date;
        criteriaViewModel.SearchForDateAsync(date);
    }

    [RelayCommand]
    private void SearchForAnnualDate()
    {
        var date = DatabaseParsing.ParseFilesDatetime(SelectedFile!.Datetime)!.Value.Date;
        criteriaViewModel.SearchForAnnualDateAsync(date.Month, date.Day);
    }

    [RelayCommand]
    private void SearchForTime()
    {
        var time = DatabaseParsing.ParseFilesDatetime(SelectedFile!.Datetime)!.Value.TimeOfDay;
        criteriaViewModel.SearchForTimeAsync(time);
    }

    [RelayCommand]
    private void SearchForBirthday(PersonModel person)
    {
        var birthday = DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth!);
        criteriaViewModel.SearchForAnnualDateAsync(birthday.Month, birthday.Day);
    }

    [RelayCommand]
    private void SearchForLocation(LocationModel location) => criteriaViewModel.SearchForLocationAsync(location);

    [RelayCommand]
    private void SearchForTag(Tag tag) => criteriaViewModel.SearchForTagAsync(tag.Model);

    [RelayCommand]
    private void SearchForTags() => criteriaViewModel.SearchForTagsAsync(Tags.Select(x => x.Model));

    [RelayCommand]
    private void AddPersonSearchFilter(PersonModel person) => criteriaViewModel.AddPersonSearchFilter(person);

    [RelayCommand]
    private void AddPersonGroupSearchFilter() => criteriaViewModel.AddPersonGroupSearchFilter(Persons.Select(x => x.Model));

    [RelayCommand]
    private void AddBirthdayDateSearchFilter(PersonModel person)
    {
        var birthday = DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth!);
        criteriaViewModel.AddAnnualDateSearchFilter(birthday.Month, birthday.Day);
    }

    [RelayCommand]
    private void AddLocationSearchFilter(LocationModel location) => criteriaViewModel.AddLocationSearchFilter(location);

    [RelayCommand]
    private void AddTagSearchFilter(Tag tag) => criteriaViewModel.AddTagSearchFilter(tag.Model);

    [RelayCommand]
    private void AddTagsSearchFilter() => criteriaViewModel.AddTagsSearchFilter(Tags.Select(x => x.Model));

    [RelayCommand]
    private void AddSearchForDate()
    {
        var date = DatabaseParsing.ParseFilesDatetime(SelectedFile!.Datetime)!.Value.Date;
        criteriaViewModel.AddDateSearchFilter(date);
    }

    [RelayCommand]
    private void AddSearchForAnnualDate()
    {
        var date = DatabaseParsing.ParseFilesDatetime(SelectedFile!.Datetime)!.Value.Date;
        criteriaViewModel.AddAnnualDateSearchFilter(date.Month, date.Day);
    }

    [RelayCommand]
    private void AddSearchForTime()
    {
        var time = DatabaseParsing.ParseFilesDatetime(SelectedFile!.Datetime)!.Value.TimeOfDay;
        criteriaViewModel.AddTimeSearchFilter(time);
    }
}



