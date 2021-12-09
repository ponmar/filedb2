using System.Windows;

namespace FileDB.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
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
        private WindowState windowState = Utils.Config.WindowMode == Config.WindowMode.Normal ? WindowState.Normal : WindowState.Maximized;

        public WindowStyle WindowStyle
        {
            get => windowStyle;
            set => SetProperty(ref windowStyle, value);
        }
        private WindowStyle windowStyle = Utils.Config.WindowMode == Config.WindowMode.Fullscreen ? WindowStyle.None : WindowStyle.ThreeDBorderWindow;

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
        private bool readWriteMode = !Utils.Config.ReadOnly;

        public MainViewModel()
        {
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            var title = $"{Utils.ApplicationName} {ReleaseInformation.Version.Major}.{ReleaseInformation.Version.Minor}";

            if (!string.IsNullOrEmpty(Utils.Config.Name))
            {
                title += $" [{Utils.Config.Name}]";
            }

            if (!ReadWriteMode)
            {
                title += " (read only)";
            }

            Title = title;
        }
    }
}
