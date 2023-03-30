﻿using System;
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

public partial class FileInfoViewModel : ObservableObject
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
    private FilesModel? selectedFile;

    [ObservableProperty]
    private string currentFileInternalPath = string.Empty;

    [ObservableProperty]
    private string currentFileInternalDirectoryPath = string.Empty;

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

    private string currentFilePath = string.Empty;
    private BitmapImage? currentFileImage = null;
    private int currentFileRotation = 0;

    private IEnumerable<PersonModel> currentFilePersonList = new List<PersonModel>();
    private IEnumerable<LocationModel> currentFileLocationList = new List<LocationModel>();
    private IEnumerable<TagModel> currentFileTagList = new List<TagModel>();

    private readonly IConfigRepository configRepository;
    private readonly IDbAccessRepository dbAccessRepository;
    private readonly IFilesystemAccessRepository filesystemAccessRepository;
    private readonly IImageLoader imageLoader;

    public FileInfoViewModel(IConfigRepository configRepository, IDbAccessRepository dbAccessRepository, IFilesystemAccessRepository filesystemAccessRepository, IImageLoader imageLoader)
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
                if (x.FilePath == currentFilePath)
                {
                    currentFileImage = x.Image;
                    Events.Send(new ShowImage(currentFileImage, -currentFileRotation));
                }
            });
        });

        this.RegisterForEvent<ImageLoadError>((x) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (x.FilePath == currentFilePath)
                {
                    CurrentFileLoadError = $"Image loading error:\n{x.Exception.Message}";
                    currentFileImage = null;
                    Events.Send<CloseImage>();
                }
            });
        });
    }

    [RelayCommand]
    private void OpenFileLocation()
    {
        if (!string.IsNullOrEmpty(currentFilePath) && File.Exists(currentFilePath))
        {
            Utils.SelectFileInExplorer(currentFilePath);
        }
    }

    [RelayCommand]
    private void OpenFileWithDefaultApp()
    {
        if (!string.IsNullOrEmpty(currentFilePath) && File.Exists(currentFilePath))
        {
            Utils.OpenFileWithDefaultApp(currentFilePath);
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
        currentFileLocationList = dbAccessRepository.DbAccess.GetLocationsFromFile(selection.Id);
        currentFileTagList = dbAccessRepository.DbAccess.GetTagsFromFile(selection.Id);

        CurrentFileInternalDirectoryPath = Path.GetDirectoryName(selection.Path)!.Replace(@"\", "/");
        CurrentFileInternalPath = selection.Path;
        CurrentFileDescription = selection.Description ?? string.Empty;
        CurrentFileDateTime = GetFileDateTimeString(selection.Datetime);
        CurrentFilePosition = selection.Position != null ? Utils.CreateShortFilePositionString(selection.Position) : string.Empty;
        CurrentFilePositionLink = selection.Position != null ? Utils.CreatePositionUri(selection.Position, configRepository.Config.LocationLink) : null;
        CurrentFilePersons = GetFilePersonsString(selection);
        CurrentFileLocations = GetFileLocationsString(selection.Id);
        CurrentFileTags = GetFileTagsString(selection.Id);
        CurrentFileLoadError = string.Empty;

        var fileType = FileTypeUtils.GetFileType(selection.Path);
        if (fileType != FileType.Picture)
        {
            CurrentFileLoadError = "File type not supported.";
            currentFileImage = null;
            Events.Send<CloseImage>();
            return;
        }

        currentFileRotation = DatabaseParsing.OrientationToDegrees(selection.Orientation ?? 0);
        currentFilePath = filesystemAccessRepository.FilesystemAccess.ToAbsolutePath(selection.Path);

        imageLoader.LoadImage(currentFilePath);
    }

    private void CloseFile()
    {
        SelectedFile = null;

        CurrentFileInternalPath = string.Empty;
        CurrentFileInternalDirectoryPath = string.Empty;
        CurrentFileDescription = string.Empty;
        CurrentFileDateTime = string.Empty;
        CurrentFilePosition = string.Empty;
        CurrentFilePositionLink = null;
        CurrentFilePersons = string.Empty;
        CurrentFileLocations = string.Empty;
        CurrentFileTags = string.Empty;
        CurrentFileLoadError = "No match";

        currentFileRotation = 0;
        currentFilePath = string.Empty;
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
    private void OpenPresentationWindow()
    {
        var window = new PresentationWindow()
        {
            Owner = Application.Current.MainWindow,
            Title = $"{Utils.ApplicationName} {Utils.GetVersionString()} - Presentation"
        };

        if (currentFilePath != string.Empty && currentFileImage != null)
        {
            window.ShowImage(currentFileImage, -currentFileRotation);
        }

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
