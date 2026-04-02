using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using FileDBInterface.Model;
using System.Linq;
using FileDB.Model;
using FileDB.Dialogs;
using Avalonia.Threading;
using FileDB.Configuration;
using FileDB.Sorters;
using System.Threading.Tasks;

namespace FileDB.ViewModels.Search;

public class SearchResult
{
    public required List<FileModel> Files { get; init; }

    public override string ToString() => $"{Count} files";

    public int Count => Files.Count;
}

public interface ISearchResultRepository
{
    IEnumerable<FileModel> Files { get; }
}

public interface ISearchResultRepositoryManagement
{
    void PopulateRepo(IEnumerable<FileModel> files);
    void RemoveFileFromRepo(FileModel file);
}

public interface IFileSelector
{
    FileModel? SelectedFile { get; }
}

public partial class ResultViewModel : ObservableObject, ISearchResultRepository, ISearchResultRepositoryManagement, IFileSelector
{
    private readonly IConfigProvider configProvider;
    private readonly IDialogs dialogs;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IImageLoader imageLoader;
    private readonly ISpeeker speeker;
    private readonly IClipboardService clipboardService;

    private readonly Random random = new();
    private readonly DispatcherTimer slideshowTimer = new();

    public IEnumerable<FileModel> Files => SearchResult is null ? Enumerable.Empty<FileModel>() : SearchResult.Files;

    public FileModel? SelectedFile => SelectedFileIndex == -1 ? null : SearchResult!.Files[SelectedFileIndex];

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
                        Messenger.Send<FileSelectionChanged>();
                    }
                }
                else
                {
                    SelectedFileIndex = -1;
                    Messenger.Send<FileSelectionChanged>();
                }

                FireSearchResultUpdatedEvents();
            }
        }
    }
    private SearchResult? searchResult = null;

    public ObservableCollection<SearchResult> SearchResultHistory { get; } = [];

    [ObservableProperty]
    public partial SearchResult? SearchResultHistorySelection { get; set; }

    public bool FindFilesFromHistoryEnabled => SearchResultHistory.Count >= 2;

    partial void OnSearchResultHistorySelectionChanged(SearchResult? value)
    {
        if (value is not null)
        {
            SlideshowActive = false;
            SearchResult = value;
        }
    }

    [ObservableProperty]
    public partial bool SlideshowActive { get; set; } = false;

    partial void OnSlideshowActiveChanged(bool value)
    {
        if (value)
        {
            if (SearchResult is not null && SearchResult.Count > 1)
            {
                slideshowTimer.Start();
            }
        }
        else
        {
            slideshowTimer.Stop();
        }
    }

    [ObservableProperty]
    public partial bool RandomActive { get; set; } = false;

    [ObservableProperty]
    public partial bool RepeatActive { get; set; } = false;

    [ObservableProperty]
    public partial int SlideshowDelay { get; set; }

    partial void OnSlideshowDelayChanged(int value)
    {
        var slideshowWasActive = SlideshowActive;
        SlideshowActive = false;
        slideshowTimer.Interval = TimeSpan.FromSeconds(value);
        SlideshowActive = slideshowWasActive;
    }

    [ObservableProperty]
    public partial List<SortMethod> SortMethods { get; set; } = [.. Enum.GetValues<SortMethod>()];

    [ObservableProperty]
    public partial SortMethod SelectedSortMethod { get; set; }

    partial void OnSelectedSortMethodChanged(SortMethod value)
    {
        SortSearchResult(configProvider.Config.KeepSelectionAfterSort);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedFileNumber))]
    public partial int SelectedFileIndex { get; set; } = -1;

    public int SelectedFileNumber => SelectedFileIndex + 1;

    public string CurrentFileInternalPath => HasNonEmptySearchResult ? SearchResult!.Files[SelectedFileIndex].Path : string.Empty;

    public int SearchNumberOfHits => SearchResult is not null ? SearchResult.Count : 0;

    public bool HasSearchResult => SearchResult is not null;

    public bool HasNonEmptySearchResult => SearchResult is not null && SearchResult.Count > 0;

    [ObservableProperty]
    public partial bool SpeekActive { get; set; }

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

    public ResultViewModel(IConfigProvider configProvider, IDialogs dialogs, IFilesystemAccessProvider filesystemAccessProvider, IImageLoader imageLoader, ISpeeker speeker, IClipboardService clipboardService)
    {
        this.configProvider = configProvider;
        this.dialogs = dialogs;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.imageLoader = imageLoader;
        this.speeker = speeker;
        this.clipboardService = clipboardService;

        SlideshowDelay = configProvider.Config.SlideshowDelay;

        slideshowTimer.Tick += SlideshowTimer_Tick;
        slideshowTimer.Interval = TimeSpan.FromSeconds(SlideshowDelay);

        SelectedSortMethod = configProvider.Config.DefaultSortMethod;

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            slideshowTimer.Interval = TimeSpan.FromSeconds(configProvider.Config.SlideshowDelay);
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
        SlideshowActive = false;
        SelectPrevFile();
    }

    [RelayCommand]
    public void NextFile()
    {
        SlideshowActive = false;
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

        SlideshowActive = false;

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

        SlideshowActive = false;

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
        SlideshowActive = false;
        LoadFile(0);
        FireBrowsingEnabledEvents();
    }

    [RelayCommand]
    private void LastFile()
    {
        SlideshowActive = false;
        if (searchResult is not null)
        {
            LoadFile(SearchResult!.Count - 1);
        }
        FireBrowsingEnabledEvents();
    }

    private void StartSlideshow()
    {
        if (SearchResult is not null && SearchResult.Count > 1)
        {
            slideshowTimer.Start();
        }
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
                    SlideshowActive = false;
                }
            }
        }
    }

    [RelayCommand]
    private async Task ExportFileListAsync()
    {
        await dialogs.ShowExportSearchResultDialogAsync(SearchResult!);
    }

    [RelayCommand]
    private void CopyFileList()
    {
        clipboardService.SetTextAsync(Utils.CreateFileList(SearchResult!.Files));
    }

    private void LoadFile(int index)
    {
        if (SearchResult is not null &&
            index >= 0 && index < SearchResult.Count)
        {
            SelectedFileIndex = index;
            OnPropertyChanged(nameof(CurrentFileInternalPath));

            if (configProvider.Config.LoadExifOrientationFromFileWhenMissingInDatabase)
            {
                var selection = SearchResult.Files[SelectedFileIndex];
                if (selection.Orientation is null)
                {
                    var fileAbsolutePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(selection.Path);
                    selection.Orientation = filesystemAccessProvider.FilesystemAccess.ParseFileMetadata(fileAbsolutePath).Orientation;
                }
            }

            Messenger.Send<FileSelectionChanged>();

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
        Messenger.Send<SearchResultRepositoryUpdated>();
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
        SlideshowActive = false;
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

            case SortMethod.Random:
                SortFiles(new FileModelByRandomSorter(), false, preserveSelection);
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

    public void PopulateRepo(IEnumerable<FileModel> files)
    {
        SlideshowActive = false;
        SearchResult = new SearchResult() { Files = files.ToList() };
    }

    public void RemoveFileFromRepo(FileModel file)
    {
        if (SearchResult is null)
        {
            return;
        }

        var fileIndex = SearchResult.Files.IndexOf(file);
        if (fileIndex == -1)
        {
            return;
        }

        SearchResult!.Files.RemoveAt(fileIndex);
        if (SearchResult.Files.Count > 0)
        {
            var newIndex = SelectedFileIndex == 0 ? 0 : SelectedFileIndex - 1;
            LoadFile(newIndex);
            FireSearchResultUpdatedEvents();
        }
        else
        {
            SearchResult = null;
        }
    }
}
