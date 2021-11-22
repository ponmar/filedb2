﻿namespace FileDB.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }
        private string title;

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
            var title = $"{Utils.ApplicationName} {Shared.Version.Major}.{Shared.Version.Minor}";

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
