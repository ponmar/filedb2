using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Configuration;
using FileDB.Model;

namespace FileDB.ViewModel;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private int numNotifications = 0;

    public string Title
    {
        get
        {
            var title = $"{Utils.ApplicationName} {Utils.GetVersionString()}";
            if (!string.IsNullOrEmpty(model.Config.Name))
            {
                title += $" [{model.Config.Name}]";
            }
            if (!ReadWriteMode)
            {
                title += " (read only)";
            }
            return title;
        }
    }

    [ObservableProperty]
    private WindowState windowState = DefaultWindowState;

    private static WindowState DefaultWindowState => Model.Model.Instance.Config.WindowMode == WindowMode.Normal ? WindowState.Normal : WindowState.Maximized;

    [ObservableProperty]
    private WindowStyle windowStyle = DefaultWindowStyle;

    private static WindowStyle DefaultWindowStyle => Model.Model.Instance.Config.WindowMode == WindowMode.Fullscreen ? WindowStyle.None : WindowStyle.ThreeDBorderWindow;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public bool readWriteMode = !Model.Model.Instance.Config.ReadOnly;

    private readonly Model.Model model = Model.Model.Instance;

    public MainViewModel()
    {
        var model = Model.Model.Instance;

        NumNotifications = model.Notifications.Count;
        WeakReferenceMessenger.Default.Register<NotificationsUpdated>(this, (r, m) =>
        {
            NumNotifications = Model.Model.Instance.Notifications.Count;
        });

        WeakReferenceMessenger.Default.Register<FullscreenBrowsingRequested>(this, (r, m) =>
        {
            WindowState = m.Fullscreen ? WindowState.Maximized : DefaultWindowState;
            WindowStyle = m.Fullscreen ? WindowStyle.None : DefaultWindowStyle;
        });

        WeakReferenceMessenger.Default.Register<ConfigLoaded>(this, (r, m) =>
        {
            ReadWriteMode = !model.Config.ReadOnly;
        });
    }
}
