using System.Windows.Input;

namespace FileDB2Browser.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public ICommand OpenSettingsPageCommand => openSettingsPageCommand ??= new CommandHandler(OpenSettingsPage);
        private ICommand openSettingsPageCommand;

        public ICommand OpenFindPageCommand => openFindPageCommand ??= new CommandHandler(OpenFindPage);
        private ICommand openFindPageCommand;

        public ICommand OpenBirthdaysPageCommand => openBirthdaysPageCommand ??= new CommandHandler(OpenBirthdaysPage);
        private ICommand openBirthdaysPageCommand;

        public ICommand OpenPersonsPageCommand => openPersonsPageCommand ??= new CommandHandler(OpenPersonsPage);
        private ICommand openPersonsPageCommand;

        public ICommand OpenLocationsPageCommand => openLocationsPageCommand ??= new CommandHandler(OpenLocationsPage);
        private ICommand openLocationsPageCommand;

        public ICommand OpenTagsPageCommand => openTagsPageCommand ??= new CommandHandler(OpenTagsPage);
        private ICommand openTagsPageCommand;

        public ICommand OpenImportPageCommand => openImportPageCommand ??= new CommandHandler(OpenImportPage);
        private ICommand openImportPageCommand;

        public ICommand OpenToolsPageCommand => openToolsPageCommand ??= new CommandHandler(OpenToolsPage);
        private ICommand openToolsPageCommand;

        public bool SettingsPageActive
        {
            get => settingsPageActive;
            private set { SetProperty(ref settingsPageActive, value); }
        }
        private bool settingsPageActive = false;

        public bool FindPageActive
        {
            get => findPageActive;
            private set { SetProperty(ref findPageActive, value); }
        }
        private bool findPageActive = false;

        public bool BirthdaysPageActive
        {
            get => birthdaysPageActive;
            private set { SetProperty(ref birthdaysPageActive, value); }
        }
        private bool birthdaysPageActive = false;

        public bool PersonsPageActive
        {
            get => personsPageActive;
            private set { SetProperty(ref personsPageActive, value); }
        }
        private bool personsPageActive = false;

        public bool LocationsPageActive
        {
            get => locationsPageActive;
            private set { SetProperty(ref locationsPageActive, value); }
        }
        private bool locationsPageActive = false;

        public bool TagsPageActive
        {
            get => tagsPageActive;
            private set { SetProperty(ref tagsPageActive, value); }
        }
        private bool tagsPageActive = false;

        public bool ImportPageActive
        {
            get => importPageActive;
            private set { SetProperty(ref importPageActive, value); }
        }
        private bool importPageActive = false;

        public bool ToolsPageActive
        {
            get => toolsPageActive;
            private set { SetProperty(ref toolsPageActive, value); }
        }
        private bool toolsPageActive = false;

        public MainViewModel()
        {
            OpenFindPage();
        }

        public void OpenSettingsPage()
        {
            ResetActivePage();
            SettingsPageActive = true;
        }

        public void OpenFindPage()
        {
            ResetActivePage();
            FindPageActive = true;
        }

        public void OpenBirthdaysPage()
        {
            ResetActivePage();
            BirthdaysPageActive = true;
        }

        public void OpenPersonsPage()
        {
            ResetActivePage();
            PersonsPageActive = true;
        }

        public void OpenLocationsPage()
        {
            ResetActivePage();
            LocationsPageActive = true;
        }

        public void OpenTagsPage()
        {
            ResetActivePage();
            TagsPageActive = true;
        }

        public void OpenImportPage()
        {
            ResetActivePage();
            ImportPageActive = true;
        }

        public void OpenToolsPage()
        {
            ResetActivePage();
            ToolsPageActive = true;
        }

        private void ResetActivePage()
        {
            SettingsPageActive = false;
            FindPageActive = false;
            BirthdaysPageActive = false;
            PersonsPageActive = false;
            LocationsPageActive = false;
            TagsPageActive = false;
            ImportPageActive = false;
            ToolsPageActive = false;
        }
    }
}
