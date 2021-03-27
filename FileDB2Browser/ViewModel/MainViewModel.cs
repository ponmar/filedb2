using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileDB2Browser.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public ICommand OpenStartPageCommand
        {
            get
            {
                return openStartPageCommand ??= new CommandHandler(OpenStartPage);
            }
        }
        private ICommand openStartPageCommand;

        public ICommand OpenFindPageCommand
        {
            get
            {
                return openFindPageCommand ??= new CommandHandler(OpenFindPage);
            }
        }
        private ICommand openFindPageCommand;

        public ICommand OpenBirthdaysPageCommand
        {
            get
            {
                return openBirthdaysPageCommand ??= new CommandHandler(OpenBirthdaysPage);
            }
        }
        private ICommand openBirthdaysPageCommand;

        public ICommand OpenPersonsPageCommand
        {
            get
            {
                return openPersonsPageCommand ??= new CommandHandler(OpenPersonsPage);
            }
        }
        private ICommand openPersonsPageCommand;

        public ICommand OpenLocationsPageCommand
        {
            get
            {
                return openLocationsPageCommand ??= new CommandHandler(OpenLocationsPage);
            }
        }
        private ICommand openLocationsPageCommand;

        public ICommand OpenTagsPageCommand
        {
            get
            {
                return openTagsPageCommand ??= new CommandHandler(OpenTagsPage);
            }
        }
        private ICommand openTagsPageCommand;

        public ICommand OpenImportPageCommand
        {
            get
            {
                return openImportPageCommand ??= new CommandHandler(OpenImportPage);
            }
        }
        private ICommand openImportPageCommand;

        public ICommand OpenToolsPageCommand
        {
            get
            {
                return openToolsPageCommand ??= new CommandHandler(OpenToolsPage);
            }
        }
        private ICommand openToolsPageCommand;


        public bool StartPageActive
        {
            get => startPageActive;
            private set { SetProperty(ref startPageActive, value); }
        }
        private bool startPageActive = true;

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
        }

        public void OpenStartPage(object parameter)
        {
            ResetActivePage();
            StartPageActive = true;
        }

        public void OpenFindPage(object parameter)
        {
            ResetActivePage();
            FindPageActive = true;
        }

        public void OpenBirthdaysPage(object parameter)
        {
            ResetActivePage();
            BirthdaysPageActive = true;
        }

        public void OpenPersonsPage(object parameter)
        {
            ResetActivePage();
            PersonsPageActive = true;
        }

        public void OpenLocationsPage(object parameter)
        {
            ResetActivePage();
            LocationsPageActive = true;
        }

        public void OpenTagsPage(object parameter)
        {
            ResetActivePage();
            TagsPageActive = true;
        }

        public void OpenImportPage(object parameter)
        {
            ResetActivePage();
            ImportPageActive = true;
        }

        public void OpenToolsPage(object parameter)
        {
            ResetActivePage();
            ToolsPageActive = true;
        }

        private void ResetActivePage()
        {
            StartPageActive = false;
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
