﻿using System.Windows;

namespace FileDB.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public int NumNotifications
        {
            get => numNotifications;
            set => SetProperty(ref numNotifications, value);
        }
        private int numNotifications = 0;

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }
        private string title;

        public WindowState WindowState
        {
            get => windowState;
            set => SetProperty(ref windowState, value);
        }
        private WindowState windowState = Model.Model.Instance.Config.WindowMode == Config.WindowMode.Normal ? WindowState.Normal : WindowState.Maximized;

        public WindowStyle WindowStyle
        {
            get => windowStyle;
            set => SetProperty(ref windowStyle, value);
        }
        private WindowStyle windowStyle = Model.Model.Instance.Config.WindowMode == Config.WindowMode.Fullscreen ? WindowStyle.None : WindowStyle.ThreeDBorderWindow;

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
        }

        private void Model_NotificationsUpdated(object sender, System.EventArgs e)
        {
            NumNotifications = Model.Model.Instance.Notifications.Count;
        }

        private void UpdateTitle()
        {
            var title = $"{Utils.ApplicationName} {ReleaseInformation.Version.Major}.{ReleaseInformation.Version.Minor}";

            if (!string.IsNullOrEmpty(model.Config.Name))
            {
                title += $" [{model.Config.Name}]";
            }

            if (!ReadWriteMode)
            {
                title += " (read only)";
            }

            Title = title;
        }
    }
}
