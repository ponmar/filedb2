using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaDevices;
using System;
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

    public bool DeviceIsSelected => selectedDevice != null;

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
        SelectedDevice = null;
        DownloadResult = DefaultDownloadResult;

        devices.Clear();
        foreach (var device in MediaDevice.GetDevices())
        {
            devices.Add(device.FriendlyName);
        }
    }

    [RelayCommand]
    private void BrowseDestinationDirectory()
    {
        var selectedDir = Dialogs.BrowseExistingDirectory(tempDir, "Browse destination directory");
        if (selectedDir != null)
        {
            TempDir = selectedDir;
        }
    }

    [RelayCommand]
    private void DownloadFilesFromDevice()
    {
        if (selectedDevice == null)
        {
            return;
        }
        if (string.IsNullOrEmpty(tempDir))
        {
            return;
        }

        if (!Directory.Exists(tempDir))
        {
            Directory.CreateDirectory(tempDir);
        }

        using var device = MediaDevice.GetDevices().First(d => d.FriendlyName == selectedDevice || d.Description == selectedDevice);
        device.Connect();

        // TODO: need to check "Internal shared storage" to support all devices?
        var devicePhotoDir = device.GetDirectoryInfo(@"\Internal storage\DCIM\Camera");
        var files = devicePhotoDir.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly);

        if (!Dialogs.ShowConfirmDialog(Title, $"Download {files.Count()} from selected device?"))
        {
            return;
        }

        var numDownloadedFiles = 0;
        var numSkippedFiles = 0;

        foreach (var file in files)
        {
            var destinationFilePath = Path.Combine(tempDir, file.Name);
            if (File.Exists(destinationFilePath))
            {
                numSkippedFiles++;
                continue;
            }

            using var fs = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write);
            device.DownloadFile(file.FullName, fs);
            numDownloadedFiles++;
        }
        device.Disconnect();

        DownloadResult = $"{numDownloadedFiles} files downloaded ({numSkippedFiles} skipped)";
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
