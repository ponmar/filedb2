using System.Windows.Input;

namespace FileDB2Browser.ViewModel
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
        private bool readWriteMode = !Utils.BrowserConfig.ReadOnly;

        public MainViewModel()
        {
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            Title = ReadWriteMode ? $"{Utils.ApplicationTitle}" : $"{Utils.ApplicationTitle} - Read only mode";
        }
    }
}
