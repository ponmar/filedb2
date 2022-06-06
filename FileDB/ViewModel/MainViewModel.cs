using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Configuration;

namespace FileDB.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        public int NumNotifications
        {
            get => numNotifications;
            set
            {
                if (SetProperty(ref numNotifications, value))
                {
                    UpdateTitle();
                }
            }
        }
        private int numNotifications = 0;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private WindowState windowState = DefaultWindowState;

        // TODO: update when new config loaded
        private static WindowState DefaultWindowState => Model.Model.Instance.Config.WindowMode == WindowMode.Normal ? WindowState.Normal : WindowState.Maximized;

        [ObservableProperty]
        private WindowStyle windowStyle = DefaultWindowStyle;

        // TODO: update when new config loaded
        private static WindowStyle DefaultWindowStyle => Model.Model.Instance.Config.WindowMode == WindowMode.Fullscreen ? WindowStyle.None : WindowStyle.ThreeDBorderWindow;

        public bool ReadWriteMode
        {
            get => readWriteMode;
            set
            {
                if (SetProperty(ref readWriteMode, value))
                {
                    UpdateTitle();
                }
            }
        }
        private bool readWriteMode = !Model.Model.Instance.Config.ReadOnly;

        private readonly Model.Model model = Model.Model.Instance;

        public MainViewModel()
        {
            UpdateTitle();

            var model = Model.Model.Instance;

            NumNotifications = model.Notifications.Count;
            model.NotificationsUpdated += Model_NotificationsUpdated;

            model.TemporaryFullscreenRequested += Model_TemporaryFullscreenRequested;
            model.ConfigLoaded += Model_ConfigLoaded;
        }

        private void Model_ConfigLoaded(object sender, System.EventArgs e)
        {
            ReadWriteMode = !model.Config.ReadOnly;
        }

        private void Model_TemporaryFullscreenRequested(object sender, bool fullscreen)
        {
            WindowState = fullscreen ? WindowState.Maximized : DefaultWindowState;
            WindowStyle = fullscreen ? WindowStyle.None : DefaultWindowStyle;
        }

        private void Model_NotificationsUpdated(object sender, System.EventArgs e)
        {
            NumNotifications = Model.Model.Instance.Notifications.Count;
        }

        private void UpdateTitle()
        {
            var title = $"{Utils.ApplicationName} {ReleaseInformation.VersionString}";

            if (!string.IsNullOrEmpty(model.Config.Name))
            {
                title += $" [{model.Config.Name}]";
            }

            if (!ReadWriteMode)
            {
                title += " (read only)";
            }

            if (NumNotifications > 1)
            {
                title += $" {NumNotifications} new notifications!";
            }
            else if (NumNotifications == 1)
            {
                title += $" {NumNotifications} new notification!";
            }

            Title = title;
        }
    }
}
