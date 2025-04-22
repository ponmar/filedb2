using FileDBInterface.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO.Abstractions;
using FileDBInterface.FileFormats;
using FileDBInterface.Extensions;
using FileDB.Lang;
using FileDB.Model;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using FileDB.Dialogs;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileDB.ViewModels.Search;

public record Person(PersonModel Model, string Label);
public record Location(LocationModel Model, string Name, string? MapUrl);
public record Tag(TagModel Model, string Name);

public enum UpdateHistoryType
{
    TogglePerson,
    ToggleLocation,
    ToggleTag,
}

public partial class FileViewModel : ObservableObject
{
    [ObservableProperty]
    private bool maximize = false;

    partial void OnMaximizeChanged(bool value)
    {
        Messenger.Send(new FullscreenBrowsingRequested(value));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverlayFontSize))]
    private bool largeTextMode = false;

    public int OverlayFontSize => LargeTextMode ? configProvider.Config.OverlayTextSizeLarge : configProvider.Config.OverlayTextSize;

    public bool FileSelected => SelectedFile is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileSelected))]
    private FileModel? selectedFile;

    [ObservableProperty]
    private string internalPath = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string dateTime = string.Empty;

    [ObservableProperty]
    private string position = string.Empty;

    [ObservableProperty]
    private string? positionLink;

    public ObservableCollection<Person> Persons { get; } = [];

    public ObservableCollection<Location> Locations { get; } = [];

    public ObservableCollection<Tag> Tags { get; } = [];

    [ObservableProperty]
    private string fileLoadError = string.Empty;

    [ObservableProperty]
    private Bitmap? image = null;

    [ObservableProperty]
    private int imageRotation = 0;

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

    public FileViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IFilesystemAccessProvider filesystemAccessProvider, IImageLoader imageLoader, IFileSystem fileSystem, IDialogs dialogs, IClipboardService clipboardService, IFileSelector fileSelector, ICriteriaViewModel criteriaViewModel)
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
        });

        this.RegisterForEvent<ImageLoaded>((x) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (x.FilePath == absolutePath)
                {
                    Image = x.Image;
                }
            });
        });

        this.RegisterForEvent<ImageLoadError>((x) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (x.FilePath == absolutePath)
                {
                    FileLoadError = Strings.SearchImageLoadingError + $"\n{x.Exception.Message}";
                    Image = null;
                }
            });
        });
    }

    [RelayCommand]
    private void OpenFileLocation()
    {
        if (absolutePath.HasContent() && fileSystem.File.Exists(absolutePath))
        {
            Utils.SelectFileInExplorer(absolutePath);
        }
    }

    [RelayCommand]
    private void OpenFileWithDefaultApp()
    {
        if (absolutePath.HasContent() && fileSystem.File.Exists(absolutePath))
        {
            Utils.OpenFileWithDefaultApp(absolutePath);
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
            Messenger.Send(new RemoveFileFromSearchResult(SelectedFile));
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

        Persons.Clear();
        FileTextOverlayCreator.GetPersons(dbAccessProvider.DbAccess, selection).ToList().ForEach(Persons.Add);

        Locations.Clear();
        FileTextOverlayCreator.GetLocations(configProvider, dbAccessProvider.DbAccess, selection).ToList().ForEach(Locations.Add);

        Tags.Clear();
        FileTextOverlayCreator.GetTags(dbAccessProvider.DbAccess, selection).ToList().ForEach(Tags.Add);

        FileLoadError = string.Empty;
        Image = null;

        absolutePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(selection.Path);
        ImageRotation = -DatabaseParsing.OrientationToDegrees(selection.Orientation ?? 0);

        var fileType = FileTypeUtils.GetFileType(selection.Path);
        if (fileType == FileType.Picture)
        {
            imageLoader.LoadImage(absolutePath);
        }
        else
        {
            FileLoadError = Strings.SearchFileTypeNotSupported;
        }
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
        ImageRotation = 0;
        absolutePath = string.Empty;
    }

    [RelayCommand]
    private void OpenPresentationWindow()
    {
        dialogs.ShowPresentationWindow(Strings.PresentationWindowTitle);
    }

    [RelayCommand]
    private static void OpenUrl(string url) => Utils.OpenUriInBrowser(url);

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
}
