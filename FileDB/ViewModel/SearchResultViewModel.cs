﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Threading;
using FileDB.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using TextCopy;
using FileDB.Configuration;
using FileDB.Sorters;
using FileDBShared.Model;
using System.Linq;

namespace FileDB.ViewModel;

public class SearchResult
{
    public required List<FileModel> Files { get; init; }

    public string Name => $"{Count} files";

    public int Count => Files.Count;
}

public partial class SearchResultViewModel : ObservableObject
{
    private readonly IConfigProvider configProvider;
    private readonly IDialogs dialogs;
    private readonly ISearchResultRepository searchResultRepository;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IImageLoader imageLoader;
    private readonly ISpeeker speeker;

    private readonly Random random = new();
    private readonly DispatcherTimer slideshowTimer = new();

    private SearchResult? SearchResult
    {
        get => searchResult;
        set
        {
            if (!EqualityComparer<SearchResult>.Default.Equals(searchResult, value))
            {
                searchResult = value;

                var updateViaHistorySelection = searchResult == SearchResultHistorySelection;
                SearchResultHistorySelection = null;

                if (searchResult is not null)
                {
                    if (searchResult.Count > 0)
                    {
                        SelectedFileIndex = 0;
                        SortSearchResult(false);
                        if (!updateViaHistorySelection)
                        {
                            // Searching via history should not add more items to history
                            AddSearchResultToHistory();
                        }
                    }
                    else
                    {
                        SelectedFileIndex = -1;
                        Events.Send<CloseSearchResultFile>();
                    }
                }
                else
                {
                    SelectedFileIndex = -1;
                    Events.Send<CloseSearchResultFile>();
                }

                FireSearchResultUpdatedEvents();
            }
        }
    }
    private SearchResult? searchResult = null;

    public ObservableCollection<SearchResult> SearchResultHistory { get; } = [];

    [ObservableProperty]
    private SearchResult? searchResultHistorySelection;

    public bool FindFilesFromHistoryEnabled => SearchResultHistory.Count >= 2;

    partial void OnSearchResultHistorySelectionChanged(SearchResult? value)
    {
        if (value is not null)
        {
            StopSlideshow();
            SearchResult = value;
        }
    }

    [ObservableProperty]
    private bool slideshowActive = false;

    [ObservableProperty]
    private bool randomActive = false;

    [ObservableProperty]
    private bool repeatActive = false;

    [ObservableProperty]
    private List<SortMethod> sortMethods = Enum.GetValues<SortMethod>().ToList();

    [ObservableProperty]
    private SortMethod selectedSortMethod;

    partial void OnSelectedSortMethodChanged(SortMethod value)
    {
        SortSearchResult(configProvider.Config.KeepSelectionAfterSort);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedFileNumber))]
    private int selectedFileIndex = -1;

    public int SelectedFileNumber => SelectedFileIndex + 1;

    public string CurrentFileInternalPath => HasNonEmptySearchResult ? SearchResult!.Files[SelectedFileIndex].Path : string.Empty;

    public int SearchNumberOfHits => SearchResult is not null ? SearchResult.Count : 0;

    public bool HasSearchResult => SearchResult is not null;

    public bool HasNonEmptySearchResult => SearchResult is not null && SearchResult.Count > 0;

    [ObservableProperty]
    private bool speekActive;

    partial void OnSpeekActiveChanged(bool value)
    {
        if (value)
        {
            SpeekFileDescription();
        }
        else
        {
            speeker.CancelSpeek();
        }
    }

    public SearchResultViewModel(IConfigProvider configProvider, IDialogs dialogs, ISearchResultRepository searchResultRepository, IFilesystemAccessProvider filesystemAccessProvider, IImageLoader imageLoader, ISpeeker speeker)
    {
        this.configProvider = configProvider;
        this.dialogs = dialogs;
        this.searchResultRepository = searchResultRepository;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.imageLoader = imageLoader;
        this.speeker = speeker;

        slideshowTimer.Tick += SlideshowTimer_Tick;
        slideshowTimer.Interval = TimeSpan.FromSeconds(configProvider.Config.SlideshowDelay);

        SelectedSortMethod = configProvider.Config.DefaultSortMethod;

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            slideshowTimer.Interval = TimeSpan.FromSeconds(configProvider.Config.SlideshowDelay);
        });

        this.RegisterForEvent<NewSearchResult>((x) =>
        {
            StopSlideshow();
            SearchResult = new SearchResult() { Files = this.searchResultRepository.Files.ToList() };
        });

        this.RegisterForEvent<RemoveFileFromSearchResult>((x) =>
        {
            if (SearchResult is null)
            {
                return;
            }

            var fileIndex = SearchResult.Files.IndexOf(x.File);
            if (fileIndex == -1)
            {
                return;
            }

            SearchResult!.Files.RemoveAt(fileIndex);
            if (SearchResult.Files.Count > 0)
            {
                FireSearchResultUpdatedEvents();
                var newIndex = SelectedFileIndex == 0 ? 0 : SelectedFileIndex - 1;
                LoadFile(newIndex);
            }
            else
            {
                SearchResult = null;
            }
        });

        this.RegisterForEvent<FileEdited>((x) =>
        {
            // Reload current file that another viewmodel edited
            LoadFile(SelectedFileIndex);
        });

        this.RegisterForEvent<SelectPrevFile>((x) => PrevFile());
        this.RegisterForEvent<SelectNextFile>((x) => NextFile());
        this.RegisterForEvent<SelectFirstFile>((x) => FirstFile());
        this.RegisterForEvent<SelectLastFile>((x) => LastFile());
        this.RegisterForEvent<SelectFileInNextDirectory>((x) => NextDirectory());
        this.RegisterForEvent<SelectFileInPrevDirectory>((x) => PrevDirectory());
    }

    [RelayCommand]
    private void PrevFile()
    {
        StopSlideshow();
        SelectPrevFile();
    }

    [RelayCommand]
    public void NextFile()
    {
        StopSlideshow();
        SelectNextFile();
    }

    private void SelectPrevFile()
    {
        LoadFile(SelectedFileIndex - 1);
        FireBrowsingEnabledEvents();
    }

    private void SelectNextFile()
    {
        LoadFile(SelectedFileIndex + 1);
        FireBrowsingEnabledEvents();
    }

    private void SelectNextRandomFile()
    {
        LoadFile(random.Next(SearchResult!.Count));
        FireBrowsingEnabledEvents();
    }

    [RelayCommand]
    private void PrevDirectory()
    {
        if (!PrevDirectoryAvailable)
        {
            return;
        }

        StopSlideshow();

        if (SelectedFileIndex < 1)
            return;

        var currentDirectory = Path.GetDirectoryName(SearchResult!.Files[SelectedFileIndex].Path);

        for (int i = SelectedFileIndex - 1; i >= 0; i--)
        {
            var directory = Path.GetDirectoryName(SearchResult.Files[i].Path);
            if (directory != currentDirectory)
            {
                LoadFile(i);
                return;
            }
        }

        FireBrowsingEnabledEvents();
    }

    [RelayCommand]
    private void NextDirectory()
    {
        if (!NextDirectoryAvailable)
        {
            return;
        }

        StopSlideshow();

        if (SelectedFileIndex == -1 || SelectedFileIndex == SearchResult!.Count - 1)
            return;

        var currentDirectory = Path.GetDirectoryName(SearchResult.Files[SelectedFileIndex].Path);

        for (int i = SelectedFileIndex + 1; i < SearchResult.Count; i++)
        {
            var directory = Path.GetDirectoryName(SearchResult.Files[i].Path);
            if (directory != currentDirectory)
            {
                LoadFile(i);
                return;
            }
        }

        FireBrowsingEnabledEvents();
    }

    [RelayCommand]
    private void FirstFile()
    {
        StopSlideshow();
        LoadFile(0);
        FireBrowsingEnabledEvents();
    }

    [RelayCommand]
    private void LastFile()
    {
        StopSlideshow();
        if (searchResult is not null)
        {
            LoadFile(SearchResult!.Count - 1);
        }
        FireBrowsingEnabledEvents();
    }

    [RelayCommand]
    private void ToggleSlideshow()
    {
        if (SlideshowActive)
        {
            StartSlideshow();
        }
        else
        {
            StopSlideshow();
        }
    }

    private void StartSlideshow()
    {
        if (SearchResult is not null && SearchResult.Count > 1)
        {
            slideshowTimer.Start();
        }
    }

    private void StopSlideshow()
    {
        slideshowTimer.Stop();
        SlideshowActive = false;
    }

    private void SlideshowTimer_Tick(object? sender, EventArgs e)
    {
        if (RandomActive)
        {
            SelectNextRandomFile();
        }
        else
        {
            if (RepeatActive)
            {
                if (SelectedFileIndex == SearchResult!.Count - 1)
                {
                    LoadFile(0);
                }
                else
                {
                    SelectNextFile();
                }
            }
            else
            {
                SelectNextFile();
                if (SelectedFileIndex == SearchResult!.Count - 1)
                {
                    StopSlideshow();
                }
            }
        }
    }

    [RelayCommand]
    private void ExportFileList()
    {
        dialogs.ShowExportDialog(SearchResult!);
    }

    [RelayCommand]
    private void CopyFileList()
    {
        ClipboardService.SetText(Utils.CreateFileList(SearchResult!.Files));
    }

    private void LoadFile(int index)
    {
        if (SearchResult is not null &&
            index >= 0 && index < SearchResult.Count)
        {
            SelectedFileIndex = index;
            OnPropertyChanged(nameof(CurrentFileInternalPath));
            var selection = SearchResult.Files[SelectedFileIndex];

            // TODO: remove support for null value for pictures by running an automatic database migration at startup (replace null values for pictures with value read from files)?
            // Note: reading of orientation from Exif is done here to get correct visualization for files added to database before orientation was parsed
            if (selection.Orientation is null)
            {
                var fileAbsolutePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(selection.Path);
                selection.Orientation = filesystemAccessProvider.FilesystemAccess.ParseFileMetadata(fileAbsolutePath).Orientation;
            }

            Events.Send(new SelectSearchResultFile(selection));

            var numImagesToLoad = Math.Max(1, configProvider.Config.NumImagesToPreload);
            for (int preloadIndex = index + 1; preloadIndex <= index + numImagesToLoad; preloadIndex++)
            {
                if (preloadIndex == SearchResult.Count)
                {
                    break;
                }

                var preLoadFile = SearchResult.Files[preloadIndex];
                var fileAbsolutePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(preLoadFile.Path);
                imageLoader.LoadImage(fileAbsolutePath);
            }

            SpeekFileDescription();
        }
    }

    private void SpeekFileDescription()
    {
        if (SpeekActive && HasNonEmptySearchResult)
        {
            var selection = SearchResult!.Files[SelectedFileIndex];
            if (selection.Description is not null)
            {
                speeker.Speek(selection.Description);
            }
            else
            {
                speeker.CancelSpeek();
            }
        }
    }    

    public bool PrevFileAvailable => SelectedFileIndex > 0;
    public bool NextFileAvailable => searchResult is not null && SelectedFileIndex < searchResult.Count - 1;
    public bool FirstFileAvailable => searchResult is not null && SelectedFileIndex > 0;
    public bool LastFileAvailable => searchResult is not null && SelectedFileIndex < searchResult.Count - 1;
    public bool PrevDirectoryAvailable => HasNonEmptySearchResult;
    public bool NextDirectoryAvailable => HasNonEmptySearchResult;

    private void FireSearchResultUpdatedEvents()
    {
        OnPropertyChanged(nameof(HasSearchResult));
        OnPropertyChanged(nameof(HasNonEmptySearchResult));
        OnPropertyChanged(nameof(SearchNumberOfHits));
        FireBrowsingEnabledEvents();
    }

    private void FireBrowsingEnabledEvents()
    {
        OnPropertyChanged(nameof(PrevFileAvailable));
        OnPropertyChanged(nameof(NextFileAvailable));
        OnPropertyChanged(nameof(FirstFileAvailable));
        OnPropertyChanged(nameof(LastFileAvailable));
        OnPropertyChanged(nameof(PrevDirectoryAvailable));
        OnPropertyChanged(nameof(NextDirectoryAvailable));
        OnPropertyChanged(nameof(CurrentFileInternalPath));
    }

    private void SortFiles(IComparer<FileModel> comparer, bool desc, bool preserveSelection)
    {
        StopSlideshow();
        if (HasNonEmptySearchResult)
        {
            var selectedFile = SearchResult!.Files[SelectedFileIndex];
            if (desc)
            {
                SearchResult.Files.Sort((x, y) => comparer.Compare(y, x));
            }
            else
            {
                SearchResult.Files.Sort(comparer);
            }
            LoadFile(preserveSelection ? SearchResult.Files.IndexOf(selectedFile) : 0);
        }
    }

    private void SortSearchResult(bool preserveSelection)
    {
        switch (SelectedSortMethod)
        {
            case SortMethod.Date:
                SortFiles(new FileModelByDateSorter(), false, preserveSelection);
                break;

            case SortMethod.DateDesc:
                SortFiles(new FileModelByDateSorter(), true, preserveSelection);
                break;

            case SortMethod.Path:
                SortFiles(new FileModelByPathSorter(), false, preserveSelection);
                break;

            case SortMethod.PathDesc:
                SortFiles(new FileModelByPathSorter(), true, preserveSelection);
                break;
        }
    }

    private void AddSearchResultToHistory()
    {
        if (SearchResultHistory.Count == configProvider.Config.SearchHistorySize)
        {
            SearchResultHistory.RemoveAt(0);
        }

        SearchResultHistory.Add(searchResult!);

        OnPropertyChanged(nameof(FindFilesFromHistoryEnabled));
        OnPropertyChanged(nameof(SearchResultHistory));
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchResult = null;
    }
}
