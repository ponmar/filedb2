using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using FileDB.View;
using FileDBShared.Model;
using TextCopy;
using FileDBInterface.DbAccess;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using System.Windows.Media;
using FileDB.Resources;
using FileDB.Extensions;

namespace FileDB.ViewModel;

public enum RotationDirection { Clockwise, CounterClockwise }

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

public partial class SearchViewModel : ObservableObject
{
    [ObservableProperty]
    private bool maximize = false;

    partial void OnMaximizeChanged(bool value)
    {
        Events.Send(new FullscreenBrowsingRequested(value));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverlayFontSize))]
    private bool largeTextMode = false;

    public int OverlayFontSize => LargeTextMode ? configRepository.Config.OverlayTextSizeLarge : configRepository.Config.OverlayTextSize;

    public bool FileSelected => SelectedFile != null;

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
    private Uri? positionLink;

    [ObservableProperty]
    private string persons = string.Empty;

    [ObservableProperty]
    private string locations = string.Empty;

    [ObservableProperty]
    private string tags = string.Empty;

    [ObservableProperty]
    private string fileLoadError = string.Empty;

    [ObservableProperty]
    private ImageSource? imageSource = null;

    private string absolutePath = string.Empty;
    private BitmapImage? image = null;
    private int rotation = 0;

    private IEnumerable<PersonModel> personList = new List<PersonModel>();
    private IEnumerable<LocationModel> locationList = new List<LocationModel>();
    private IEnumerable<TagModel> tagList = new List<TagModel>();

    private readonly IConfigRepository configRepository;
    private readonly IDbAccessRepository dbAccessRepository;
    private readonly IFilesystemAccessRepository filesystemAccessRepository;
    private readonly IImageLoader imageLoader;

    public SearchViewModel(IConfigRepository configRepository, IDbAccessRepository dbAccessRepository, IFilesystemAccessRepository filesystemAccessRepository, IImageLoader imageLoader)
    {
        this.configRepository = configRepository;
        this.dbAccessRepository = dbAccessRepository;
        this.filesystemAccessRepository = filesystemAccessRepository;
        this.imageLoader = imageLoader;

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            OnPropertyChanged(nameof(LargeTextMode));
        });

        this.RegisterForEvent<SelectSearchResultFile>((x) =>
        {
            LoadFile(x.File);
        });

        this.RegisterForEvent<CloseSearchResultFile>((x) =>
        {
            CloseFile();
        });

        this.RegisterForEvent<ImageLoaded>((x) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (x.FilePath == absolutePath)
                {
                    image = x.Image;
                    ImageSource = ImageUtils.Rotate(image, -rotation);
                }
            });
        });

        this.RegisterForEvent<ImageLoadError>((x) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (x.FilePath == absolutePath)
                {
                    FileLoadError = $"Image loading error:\n{x.Exception.Message}";
                    image = null;
                    ImageSource = null;
                }
            });
        });
    }

    [RelayCommand]
    private void OpenFileLocation()
    {
        if (absolutePath.HasContent() && File.Exists(absolutePath))
        {
            Utils.SelectFileInExplorer(absolutePath);
        }
    }

    [RelayCommand]
    private void OpenFileWithDefaultApp()
    {
        if (absolutePath.HasContent() && File.Exists(absolutePath))
        {
            Utils.OpenFileWithDefaultApp(absolutePath);
        }
    }

    [RelayCommand]
    private void CopyFileId()
    {
        if (SelectedFile != null)
        {
            ClipboardService.SetText(Utils.CreateFileList(new List<FileModel>() { SelectedFile }));
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

    private void LoadFile(FileModel selection)
    {
        SelectedFile = selection;

        personList = dbAccessRepository.DbAccess.GetPersonsFromFile(selection.Id);
        locationList = dbAccessRepository.DbAccess.GetLocationsFromFile(selection.Id);
        tagList = dbAccessRepository.DbAccess.GetTagsFromFile(selection.Id);

        InternalPath = selection.Path;
        Description = selection.Description ?? string.Empty;
        DateTime = GetFileDateTimeString(selection.Datetime);
        Position = selection.Position != null ? Utils.CreateShortFilePositionString(selection.Position) : string.Empty;
        PositionLink = selection.Position != null ? Utils.CreatePositionUri(selection.Position, configRepository.Config.LocationLink) : null;
        Persons = GetFilePersonsString(selection);
        Locations = GetFileLocationsString();
        Tags = GetFileTagsString();
        FileLoadError = string.Empty;

        image = null;
        ImageSource = null;

        absolutePath = filesystemAccessRepository.FilesystemAccess.ToAbsolutePath(selection.Path);
        rotation = DatabaseParsing.OrientationToDegrees(selection.Orientation ?? 0);

        var fileType = FileTypeUtils.GetFileType(selection.Path);
        if (fileType == FileType.Picture)
        {
            imageLoader.LoadImage(absolutePath);
        }
        else
        {
            FileLoadError = "File type not supported.";
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
        Persons = string.Empty;
        Locations = string.Empty;
        Tags = string.Empty;
        FileLoadError = "No match";

        rotation = 0;
        absolutePath = string.Empty;
        image = null;

        ImageSource = null;
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

        var now = System.DateTime.Now;
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

    private string GetFilePersonsString(FileModel selection)
    {
        var personStrings = personList.Select(p => $"{p.Firstname} {p.Lastname}{Utils.GetPersonAgeInFileString(selection.Datetime, p.DateOfBirth)}");
        return string.Join("\n", personStrings);
    }

    private string GetFileLocationsString()
    {
        var locationStrings = locationList.Select(l => l.Name);
        return string.Join("\n", locationStrings);
    }

    private string GetFileTagsString()
    {
        var tagStrings = tagList.Select(t => t.Name);
        return string.Join("\n", tagStrings);
    }

    [RelayCommand]
    private void OpenPresentationWindow()
    {
        var window = new PresentationWindow()
        {
            Owner = Application.Current.MainWindow,
            Title = string.Format(Strings.PresentationWindowTitle, Utils.ApplicationName, Utils.GetVersionString()),
        };

        window.Show();
    }

    [RelayCommand]
    private void FunctionKey(string parameter)
    {
        var functionKey = int.Parse(parameter);
        Events.Send(new CategorizationFunctionKeyPressed(functionKey));
    }

    [RelayCommand]
    private void PrevFile() => Events.Send<SelectPrevFile>();

    [RelayCommand]
    private void NextFile() => Events.Send<SelectNextFile>();

    [RelayCommand]
    private void FirstFile() => Events.Send<SelectFirstFile>();

    [RelayCommand]
    private void LastFile() => Events.Send<SelectLastFile>();

    [RelayCommand]
    private void NextDirectory() => Events.Send<SelectFileInNextDirectory>();

    [RelayCommand]
    private void PrevDirectory() => Events.Send<SelectFileInPrevDirectory>();
}
