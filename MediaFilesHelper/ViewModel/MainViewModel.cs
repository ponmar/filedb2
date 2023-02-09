using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaDevices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MediaFilesHelper.ViewModel;

public partial class MainViewModel : ObservableObject
{
    private const string DefaultDownloadResult = "No files downloaded";

    [ObservableProperty]
    private string title = "MediaFilesHelper";

    public Dialogs Dialogs { get; set; } = new();

    [ObservableProperty]
    private ObservableCollection<string> devices = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DeviceIsSelected))]
    private string? selectedDevice;

    partial void OnSelectedDeviceChanged(string? value)
    {
        DownloadResult = DefaultDownloadResult;
    }

    public bool DeviceIsSelected => SelectedDevice != null;

    [ObservableProperty]
    private string? tempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MediaFilesHelper");

    [ObservableProperty]
    private string downloadResult = DefaultDownloadResult;

    public MainViewModel()
    {
        RefreshDevices();
    }

    [RelayCommand]
    private void RefreshDevices()
    {
        DownloadResult = DefaultDownloadResult;

        Devices.Clear();
        foreach (var device in MediaDevice.GetDevices())
        {
            Devices.Add(device.FriendlyName ?? device.Description);
        }

        SelectedDevice = Devices.FirstOrDefault();
    }

    [RelayCommand]
    private void BrowseDestinationDirectory()
    {
        var selectedDir = Dialogs.BrowseExistingDirectory(TempDir, "Browse destination directory");
        if (selectedDir != null)
        {
            TempDir = selectedDir;
        }
    }

    [RelayCommand]
    private void DownloadFilesFromDevice()
    {
        if (SelectedDevice == null)
        {
            return;
        }
        if (string.IsNullOrEmpty(TempDir))
        {
            return;
        }

        if (!Dialogs.ShowConfirmDialog(Title, $"Download files from selected device?"))
        {
            return;
        }

        if (!Directory.Exists(TempDir))
        {
            Directory.CreateDirectory(TempDir);
        }

        using var device = MediaDevice.GetDevices().First(d => d.FriendlyName == SelectedDevice || d.Description == SelectedDevice);
        device.Connect();

        var deviceDirectories = new List<string>()
        {
            @"\Internal storage\DCIM\Camera",
            @"\Internal shared storage\DCIM\Camera",
        }.Where(device.DirectoryExists).ToList();

        if (deviceDirectories.Count == 0)
        {
            Dialogs.ShowErrorDialog(Title, "No directories with media files available on device");
            device.Disconnect();
            return;
        }

        var numDownloadedFiles = 0;
        var numAlreadyDownloadedFiles = 0;
        var numNameCollisions = 0;

        foreach (var directory in deviceDirectories)
        {
            var directoryInfo = device.GetDirectoryInfo(directory);
            var files = directoryInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                var destinationFilePath = Path.Combine(TempDir, file.Name);
                if (File.Exists(destinationFilePath))
                {
                    if ((long)file.Length == new System.IO.FileInfo(destinationFilePath).Length)
                    {
                        numAlreadyDownloadedFiles++;
                    }
                    else
                    {
                        numNameCollisions++;
                    }
                    continue;
                }

                using var fs = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write);
                device.DownloadFile(file.FullName, fs);
                numDownloadedFiles++;
            }
        }

        device.Disconnect();

        var result = $"{numDownloadedFiles} files downloaded.\n{numAlreadyDownloadedFiles} already downloaded.";
        if (numNameCollisions > 0)
        {
            result += $"\n{numNameCollisions} name collisions (same file already available but with another size)";
        }
        DownloadResult = result;
    }

    [RelayCommand]
    private void ShowDownloadedMediaFiles()
    {
        if (!string.IsNullOrEmpty(TempDir))
        {
            OpenDirectoryInExplorer(TempDir);
        }
    }

    private static void OpenDirectoryInExplorer(string path)
    {
        Process.Start("explorer.exe", "/open, " + path);
    }

    [RelayCommand]
    private void GroupImages()
    {
        if (!Directory.Exists(TempDir))
        {
            Dialogs.ShowErrorDialog(Title, $"Missing directory: {TempDir}");
            return;
        }

        var mediaFileGrouper = new MediaFileGrouper(TempDir);
        var files = mediaFileGrouper.ListFiles();
        mediaFileGrouper.GroupImagesByDate(files);
        mediaFileGrouper.MoveFiles(files);
    }
}
