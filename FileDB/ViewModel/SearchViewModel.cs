using System;
using System.Collections.Generic;
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
using System.IO.Abstractions;

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
    public string ToggleText => string.Format(Strings.SearchToggleText, FunctionKey, ShortItemName);
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

    private readonly IConfigProvider configProvider;
    private readonly IDbAccessProvider dbAccessProvider;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IImageLoader imageLoader;
    private readonly IFileSystem fileSystem;

    public SearchViewModel(IConfigProvider configProvider, IDbAccessProvider dbAccessProvider, IFilesystemAccessProvider filesystemAccessProvider, IImageLoader imageLoader, IFileSystem fileSystem)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.imageLoader = imageLoader;
        this.fileSystem = fileSystem;

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
                    FileLoadError = Strings.SearchImageLoadingError + $"\n{x.Exception.Message}";
                    image = null;
                    ImageSource = null;
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
            ClipboardService.SetText(Utils.CreateFileList(new List<FileModel>() { SelectedFile }));
        }
    }

    [RelayCommand]
    private void RemoveFileFromCurrentSearchResult()
    {
        if (SelectedFile is not null)
        {
            Events.Send(new RemoveFileFromSearchResult(SelectedFile));
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
        Persons = FileTextOverlayCreator.GetPersonsText(dbAccessProvider.DbAccess, selection, "\n");
        Locations = FileTextOverlayCreator.GetLocationsText(dbAccessProvider.DbAccess, selection, "\n");
        Tags = FileTextOverlayCreator.GetTagsText(dbAccessProvider.DbAccess, selection, "\n");

        FileLoadError = string.Empty;
        image = null;
        ImageSource = null;

        absolutePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(selection.Path);
        rotation = DatabaseParsing.OrientationToDegrees(selection.Orientation ?? 0);

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
        Persons = string.Empty;
        Locations = string.Empty;
        Tags = string.Empty;

        FileLoadError = Strings.SearchNoMatch;
        rotation = 0;
        absolutePath = string.Empty;
        image = null;

        ImageSource = null;
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

public class FileTextOverlayCreator
{
    public static string GetPersonsText(IDbAccess dbAccess, FileModel file, string separator)
    {
        var persons = dbAccess.GetPersonsFromFile(file.Id);
        return GetPersonsText(file, persons, separator);
    }

    public static string GetPersonsText(FileModel file, IEnumerable<PersonModel> persons, string separator)
    {
        return GetPersonsText(file.Datetime, persons, separator);
    }

    public static string GetPersonsText(string? fileDateTime, IEnumerable<PersonModel> persons, string separator)
    {
        var personStrings = GetPersonsTexts(fileDateTime, persons);
        return string.Join(separator, personStrings);
    }

    public static IEnumerable<string> GetPersonsTexts(string? fileDateTime, IEnumerable<PersonModel> persons)
    {
        var personStrings = persons.Select(p => $"{p.Firstname} {p.Lastname}{Utils.GetPersonAgeInFileString(fileDateTime, p.DateOfBirth)}").ToList();
        personStrings.Sort();
        return personStrings;
    }

    public static string GetLocationsText(IDbAccess dbAccess, FileModel file, string separator)
    {
        var locations = dbAccess.GetLocationsFromFile(file.Id);
        return GetLocationsText(locations, separator);
    }

    public static string GetLocationsText(IEnumerable<LocationModel> locations, string separator)
    {
        var locationStrings = GetLocationsTexts(locations);
        return string.Join(separator, locationStrings);
    }

    public static IEnumerable<string> GetLocationsTexts(IEnumerable<LocationModel> locations)
    {
        var locationStrings = locations.Select(l => l.Name).ToList();
        locationStrings.Sort();
        return locationStrings;
    }

    public static string GetTagsText(IDbAccess dbAccess, FileModel file, string separator)
    {
        var tags = dbAccess.GetTagsFromFile(file.Id);
        return GetTagsText(tags, separator);
    }

    public static string GetTagsText(IEnumerable<TagModel> tags, string separator)
    {
        var tagStrings = GetTagsTexts(tags);
        return string.Join(separator, tagStrings);
    }

    public static IEnumerable<string> GetTagsTexts(IEnumerable<TagModel> tags)
    {
        var tagStrings = tags.Select(t => t.Name).ToList();
        tagStrings.Sort();
        return tagStrings;
    }

    public static string? GetFileDateTimeText(FileModel file)
    {
        var datetime = DatabaseParsing.ParseFilesDatetime(file.Datetime);
        if (datetime is null)
        {
            return null;
        }

        // Note: when no time is available the string is used to avoid including time 00:00
        var resultString = file.Datetime!.Contains('T') ? datetime.Value.ToDateAndTime() : file.Datetime;

        var now = DateTime.Now;
        int yearsAgo = DatabaseUtils.GetAgeInYears(now, datetime.Value);
        if (yearsAgo == 0 && now.Year == datetime.Value.Year)
        {
            resultString += Strings.SearchFileDateTimeThisYear;
        }
        else if (yearsAgo <= 1)
        {
            resultString += string.Format(Strings.SearchFileDateTimeYearAgo, yearsAgo);
        }
        else if (yearsAgo > 1)
        {
            resultString += string.Format(Strings.SearchFileDateTimeYearsAgo, yearsAgo);
        }
        return resultString;
    }

    public static string? GetShortPositionText(FileModel file)
    {
        return file.Position is null ? null : Utils.CreateShortFilePositionString(file.Position);
    }

    public static Uri? GetPositionUri(IConfigProvider configProvider, FileModel file)
    {
        return file.Position is null ? null : Utils.CreatePositionUri(file.Position, configProvider.Config.LocationLink);
    }
}