﻿using System.Collections.Generic;
using FileDBShared.Model;
using TextCopy;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO.Abstractions;
using FileDBShared.FileFormats;
using FileDBInterface.Extensions;
using FileDBAvalonia.Lang;
using FileDBAvalonia.Model;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using FileDBAvalonia.Dialogs;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileDBAvalonia.ViewModels.Search;

public enum RotationDirection { Clockwise, CounterClockwise }

public record PersonToUpdate(int Id, string Name, string ShortName);
public record LocationToUpdate(int Id, string Name, string ShortName);
public record TagToUpdate(int Id, string Name, string ShortName);

public record Location(string Name, string? MapUrl);

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

    [ObservableProperty]
    private string persons = string.Empty;

    public ObservableCollection<Location> Locations { get; } = [];

    [ObservableProperty]
    private string tags = string.Empty;

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

    public FileViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IFilesystemAccessProvider filesystemAccessProvider, IImageLoader imageLoader, IFileSystem fileSystem, IDialogs dialogs)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.imageLoader = imageLoader;
        this.fileSystem = fileSystem;
        this.dialogs = dialogs;

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
            ClipboardService.SetText(Utils.CreateFileList(new List<FileModel>() { SelectedFile }));
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
        Persons = FileTextOverlayCreator.GetPersonsText(dbAccessProvider.DbAccess, selection, "\n");
        Locations.Clear();
        FileTextOverlayCreator.GetLocations(configProvider, dbAccessProvider.DbAccess, selection).ToList().ForEach(Locations.Add);
        Tags = FileTextOverlayCreator.GetTagsText(dbAccessProvider.DbAccess, selection, "\n");

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
        Persons = string.Empty;
        Locations.Clear();
        Tags = string.Empty;

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
    private void OpenUrl(string url) => Utils.OpenUriInBrowser(url);

    [RelayCommand]
    private void PrevFile() => Messenger.Send<SelectPrevFile>();

    [RelayCommand]
    private void NextFile() => Messenger.Send<SelectNextFile>();

    [RelayCommand]
    private void FirstFile() => Messenger.Send<SelectFirstFile>();

    [RelayCommand]
    private void LastFile() => Messenger.Send<SelectLastFile>();

    [RelayCommand]
    private void NextDirectory() => Messenger.Send<SelectFileInNextDirectory>();

    [RelayCommand]
    private void PrevDirectory() => Messenger.Send<SelectFileInPrevDirectory>();
}
