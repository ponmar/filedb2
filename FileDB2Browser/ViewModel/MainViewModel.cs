using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileDB2Browser.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
            private set
            {
                if (value != startPageActive)
                {
                    startPageActive = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(StartPageActive)));
                }
            }
        }
        private bool startPageActive = true;

        public bool FindPageActive
        {
            get => findPageActive;
            private set
            {
                if (value != findPageActive)
                {
                    findPageActive = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FindPageActive)));
                }
            }
        }
        private bool findPageActive = false;

        public bool BirthdaysPageActive
        {
            get => birthdaysPageActive;
            private set
            {
                if (value != birthdaysPageActive)
                {
                    birthdaysPageActive = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(BirthdaysPageActive)));
                }
            }
        }
        private bool birthdaysPageActive = false;

        public bool PersonsPageActive
        {
            get => personsPageActive;
            private set
            {
                if (value != personsPageActive)
                {
                    personsPageActive = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(PersonsPageActive)));
                }
            }
        }
        private bool personsPageActive = false;

        public bool LocationsPageActive
        {
            get => locationsPageActive;
            private set
            {
                if (value != locationsPageActive)
                {
                    locationsPageActive = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(LocationsPageActive)));
                }
            }
        }
        private bool locationsPageActive = false;

        public bool TagsPageActive
        {
            get => tagsPageActive;
            private set
            {
                if (value != tagsPageActive)
                {
                    tagsPageActive = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TagsPageActive)));
                }
            }
        }
        private bool tagsPageActive = false;

        public bool ImportPageActive
        {
            get => importPageActive;
            private set
            {
                if (value != importPageActive)
                {
                    importPageActive = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ImportPageActive)));
                }
            }
        }
        private bool importPageActive = false;

        public bool ToolsPageActive
        {
            get => toolsPageActive;
            private set
            {
                if (value != toolsPageActive)
                {
                    toolsPageActive = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ToolsPageActive)));
                }
            }
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
