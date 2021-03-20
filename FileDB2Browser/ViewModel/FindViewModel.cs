﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using FileDB2Interface;
using FileDB2Interface.Model;

namespace FileDB2Browser.ViewModel
{
    public class FindViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly FileDB2Handle fileDB2Handle;

        public ICommand PrevFileCommand
        {
            get
            {
                return prevFileCommand ??= new CommandHandler(() => PrevFile());
            }
        }
        private ICommand prevFileCommand;

        public ICommand NextFileCommand
        {
            get
            {
                return nextFileCommand ??= new CommandHandler(() => NextFile());
            }
        }
        private ICommand nextFileCommand;

        public ICommand FindRandomFilesCommand
        {
            get
            {
                return findRandomFilesCommand ??= new CommandHandler(() => FindRandomFiles());
            }
        }
        private ICommand findRandomFilesCommand;

        private List<FilesModel> SearchResult
        {
            get => searchResult;
            set
            {
                if (searchResult != value)
                {
                    searchResult = value;
                    if (searchResult != null)
                    {
                        SearchNumberOfHits = searchResult.Count;
                        if (searchResult.Count > 0)
                        {
                            LoadFile(0);
                        }
                        else
                        {
                            ResetFile();
                        }
                    }
                    else
                    {
                        SearchNumberOfHits = 0;
                        ResetFile();
                    }

                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SearchNumberOfHits)));
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(HasSearchResult)));
                }
            }
        }
        private List<FilesModel> searchResult = null;

        public int SearchResultIndex
        {
            get => searchResultIndex;
            private set
            {
                if (value != searchResultIndex)
                {
                    searchResultIndex = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SearchResultIndex)));
                }
            }
        }
        private int searchResultIndex = -1;

        public int SearchNumberOfHits { get; private set; } = 0;

        public int TotalNumberOfFiles { get; }

        public bool HasSearchResult => SearchResult != null;

        public string CurrentFileInternalPath
        {
            get => currentFileInternalPath;
            private set
            {
                if (value != currentFileInternalPath)
                {
                    currentFileInternalPath = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFileInternalPath)));
                }
            }
        }
        private string currentFileInternalPath;

        public string CurrentFilePath
        {
            get => currentFilePath;
            private set
            {
                if (value != currentFilePath)
                {
                    currentFilePath = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFilePath)));
                }
            }
        }
        private string currentFilePath;

        public string CurrentFileDescription
        {
            get => currentFileDescription;
            private set
            {
                if (value != currentFileDescription)
                {
                    currentFileDescription = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFileDescription)));
                }
            }
        }
        private string currentFileDescription;

        // TODO: add datetime property in string format? For example: 2015-07-15 19:01:47 (5 years ago)

        public string CurrentFilePosition
        {
            get => currentFilePosition;
            private set
            {
                if (value != currentFilePosition)
                {
                    currentFilePosition = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFilePosition)));
                }
            }
        }
        private string currentFilePosition;

        public string CurrentFilePersons
        {
            get => currentFilePersons;
            private set
            {
                if (value != currentFilePersons)
                {
                    currentFilePersons = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFilePersons)));
                }
            }
        }
        private string currentFilePersons;

        public string CurrentFileLocations
        {
            get => currentFileLocations;
            private set
            {
                if (value != currentFileLocations)
                {
                    currentFileLocations = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFileLocations)));
                }
            }
        }
        private string currentFileLocations;

        public string CurrentFileTags
        {
            get => currentFileTags;
            private set
            {
                if (value != currentFileTags)
                {
                    currentFileTags = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFileTags)));
                }
            }
        }
        private string currentFileTags;

        public void FindRandomFiles()
        {
            SearchResult = fileDB2Handle.SearchFilesRandom(10);
        }

        public FindViewModel(FileDB2Handle fileDB2Handle)
        {
            this.fileDB2Handle = fileDB2Handle;
            TotalNumberOfFiles = fileDB2Handle.GetFileCount();
        }

        public void PrevFile()
        {
            LoadFile(SearchResultIndex - 1);
        }

        public void NextFile()
        {
            LoadFile(SearchResultIndex + 1);
        }

        private void LoadFile(int index)
        {
            if (SearchResult != null &&
                index >= 0 && index < SearchResult.Count)
            {
                SearchResultIndex = index;

                var selection = SearchResult[SearchResultIndex];

                CurrentFileInternalPath = fileDB2Handle.InternalPathToPath(selection.path);
                CurrentFilePath = selection.path;
                CurrentFileDescription = selection.description ?? string.Empty;
                CurrentFilePosition = selection.position ?? string.Empty;
                CurrentFilePersons = GetFilePersonsString(selection.id);
                CurrentFileLocations = GetFileLocationsString(selection.id);
                CurrentFileTags = GetFileTagsString(selection.id);
            }
        }

        private void ResetFile()
        {
            SearchResultIndex = -1;

            CurrentFileInternalPath = string.Empty;
            CurrentFilePath = string.Empty;
            CurrentFileDescription = string.Empty;
            CurrentFilePosition = string.Empty;
            CurrentFilePersons = string.Empty;
        }

        private string GetFilePersonsString(int fileId)
        {
            var persons = fileDB2Handle.GetPersonsFromFile(fileId);
            var personStrings = persons.Select(p => $"{p.firstname} {p.lastname}{GetPersonAgeString(p)}");
            return string.Join(", ", personStrings);
        }

        private string GetPersonAgeString(PersonModel person)
        {
            var age = Utils.GetBornYearsAgo(person.dateofbirth);
            if (age != string.Empty)
            {
                age = $" ({age})";
            }
            return age;
        }

        private string GetFileLocationsString(int fileId)
        {
            // TODO: include gps position if available?
            var locations = fileDB2Handle.GetLocationsFromFile(fileId);
            var locationStrings = locations.Select(l => l.name);
            return string.Join(", ", locationStrings);
        }

        private string GetFileTagsString(int fileId)
        {
            var tags = fileDB2Handle.GetTagsFromFile(fileId);
            var tagStrings = tags.Select(t => t.name);
            return string.Join(", ", tagStrings);
        }
    }
}
