using System;
using System.Diagnostics;

namespace FileDB;

public interface IProcessUtils
{
    bool IsOpenUriInBrowserSupported();
    bool IsSelectFileInExplorerSupported();
    bool IsOpenDirectoryInExplorerSupported();
    bool IsOpenFileWithDefaultAppSupported();
    void OpenUriInBrowser(string uri);
    void SelectFileInExplorer(string path);
    void OpenDirectoryInExplorer(string path);
    void OpenFileWithDefaultApp(string path);
}

public class ProcessUtils : IProcessUtils
{
    public bool IsOpenUriInBrowserSupported() => OperatingSystem.IsWindows();
    public bool IsSelectFileInExplorerSupported() => OperatingSystem.IsWindows();
    public bool IsOpenDirectoryInExplorerSupported() => OperatingSystem.IsWindows();
    public bool IsOpenFileWithDefaultAppSupported() => OperatingSystem.IsWindows();

    public void OpenUriInBrowser(string uri)
    {
        Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
    }

    public void SelectFileInExplorer(string path)
    {
        var explorerPath = path.Replace("/", @"\");
        Process.Start("explorer.exe", "/select, " + explorerPath);
    }

    public void OpenDirectoryInExplorer(string path)
    {
        var explorerPath = path.Replace("/", @"\");
        Process.Start("explorer.exe", "/open, " + explorerPath);
    }

    public void OpenFileWithDefaultApp(string path)
    {
        var explorerPath = path.Replace("/", @"\");
        Process.Start("explorer", $"\"{explorerPath}\"");
    }
}
