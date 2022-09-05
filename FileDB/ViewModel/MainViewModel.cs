using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Configuration;

namespace FileDB.ViewModel
{
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
            model.NotificationsUpdated += Model_NotificationsUpdated;

            model.TemporaryFullscreenRequested += Model_TemporaryFullscreenRequested;
            model.ConfigLoaded += Model_ConfigLoaded;
        }

        private void Model_ConfigLoaded(object? sender, System.EventArgs e)
        {
            ReadWriteMode = !model.Config.ReadOnly;
        }

        private void Model_TemporaryFullscreenRequested(object? sender, bool fullscreen)
        {
            WindowState = fullscreen ? WindowState.Maximized : DefaultWindowState;
            WindowStyle = fullscreen ? WindowStyle.None : DefaultWindowStyle;
        }

        private void Model_NotificationsUpdated(object? sender, System.EventArgs e)
        {
            NumNotifications = Model.Model.Instance.Notifications.Count;
        }
    }
}
