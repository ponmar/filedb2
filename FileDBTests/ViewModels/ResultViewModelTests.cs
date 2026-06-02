using FakeItEasy;
using FileDB.Configuration;
using FileDB.Dialogs;
using FileDB.Model;
using FileDB.Services;
using FileDB.ViewModels.Search;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.ViewModels;

public class ResultViewModelTests
{
    private readonly IConfigProvider fakeConfigProvider = A.Fake<IConfigProvider>();
    private readonly IDialogs fakeDialogs = A.Fake<IDialogs>();
    private readonly IFilesystemAccessProvider fakeFilesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
    private readonly IFilesystemAccess fakeFilesystemAccess = A.Fake<IFilesystemAccess>();
    private readonly IDatabaseAccessProvider fakeDbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IImageLoader fakeImageLoader = A.Fake<IImageLoader>();
    private readonly ISpeeker fakeSpeeker = A.Fake<ISpeeker>();
    private readonly IClipboardService fakeClipboardService = A.Fake<IClipboardService>();

    public ResultViewModelTests()
    {
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess).Returns(fakeFilesystemAccess);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([]);
        SetupConfig();
    }

    private void SetupConfig(SortMethod defaultSort = SortMethod.Date, bool keepSelection = false, int searchHistorySize = 10, int slideshowDelay = 3)
    {
        A.CallTo(() => fakeConfigProvider.Config).Returns(
            new ConfigBuilder
            {
                DefaultSortMethod = defaultSort,
                KeepSelectionAfterSort = keepSelection,
                SearchHistorySize = searchHistorySize,
                SlideshowDelay = slideshowDelay,
            }.Build());
    }

    private ResultViewModel CreateViewModel() =>
        new(fakeConfigProvider, fakeDialogs, fakeFilesystemAccessProvider, fakeDbAccessProvider, fakeImageLoader, fakeSpeeker, fakeClipboardService);

    private static List<FileModel> SomeFiles() =>
    [
        new() { Id = 1, Path = "dirA/a.jpg", Datetime = "2000-01-01" },
        new() { Id = 2, Path = "dirA/b.jpg", Datetime = "2001-01-01" },
        new() { Id = 3, Path = "dirB/c.jpg", Datetime = "1999-01-01" },
    ];

    // --- Constructor ---

    [Fact]
    public void Constructor_NoSearchResult()
    {
        // Arrange + Act
        var vm = CreateViewModel();

        // Assert
        Assert.False(vm.HasSearchResult);
        Assert.False(vm.HasNonEmptySearchResult);
        Assert.Equal(0, vm.SearchNumberOfHits);
        Assert.Equal(-1, vm.SelectedFileIndex);
        Assert.Null(vm.SelectedFile);
    }

    [Fact]
    public void Constructor_SortMethodsContainsAllValues()
    {
        // Arrange + Act
        var vm = CreateViewModel();

        // Assert
        var expected = Enum.GetValues<SortMethod>();
        Assert.Equal(expected.Length, vm.SortMethods.Count);
        foreach (var method in expected)
        {
            Assert.Contains(method, vm.SortMethods);
        }
    }

    [Fact]
    public void Constructor_DefaultSortMethodFromConfig()
    {
        // Arrange
        SetupConfig(defaultSort: SortMethod.PathDesc);

        // Act
        var vm = CreateViewModel();

        // Assert
        Assert.Equal(SortMethod.PathDesc, vm.SelectedSortMethod);
    }

    // --- PopulateRepo ---

    [Fact]
    public void PopulateRepo_SetsSearchResultAndSelectsFirstFile()
    {
        // Arrange
        var vm = CreateViewModel();
        var files = SomeFiles();

        // Act
        vm.PopulateRepo(files);

        // Assert
        Assert.True(vm.HasSearchResult);
        Assert.True(vm.HasNonEmptySearchResult);
        Assert.Equal(files.Count, vm.SearchNumberOfHits);
        Assert.Equal(0, vm.SelectedFileIndex);
        Assert.NotNull(vm.SelectedFile);
    }

    [Fact]
    public void PopulateRepo_EmptyList_HasSearchResultButNotNonEmpty()
    {
        // Arrange
        var vm = CreateViewModel();

        // Act
        vm.PopulateRepo([]);

        // Assert
        Assert.True(vm.HasSearchResult);
        Assert.False(vm.HasNonEmptySearchResult);
        Assert.Equal(0, vm.SearchNumberOfHits);
        Assert.Equal(-1, vm.SelectedFileIndex);
        Assert.Null(vm.SelectedFile);
    }

    [Fact]
    public void PopulateRepo_SendsSearchResultRepositoryUpdatedMessage()
    {
        // Arrange
        var vm = CreateViewModel();
        var recorder = new SingleEventRecorder<SearchResultRepositoryUpdated>();

        // Act
        vm.PopulateRepo(SomeFiles());

        // Assert
        recorder.AssertEventRecorded();
    }

    [Fact]
    public void PopulateRepo_SendsFileSelectionChangedMessage()
    {
        // Arrange
        var vm = CreateViewModel();
        var recorder = new SingleEventRecorder<FileSelectionChanged>();

        // Act
        vm.PopulateRepo(SomeFiles());

        // Assert
        recorder.AssertEventRecorded();
    }

    [Fact]
    public void PopulateRepo_AddsToSearchHistory()
    {
        // Arrange
        var vm = CreateViewModel();

        // Act
        vm.PopulateRepo(SomeFiles());

        // Assert
        Assert.Single(vm.SearchResultHistory);
    }

    [Fact]
    public void PopulateRepo_Twice_HistoryHasTwoEntries()
    {
        // Arrange
        var vm = CreateViewModel();

        // Act
        vm.PopulateRepo(SomeFiles());
        vm.PopulateRepo([new() { Id = 10, Path = "other/x.jpg" }]);

        // Assert
        Assert.Equal(2, vm.SearchResultHistory.Count);
    }

    [Fact]
    public void PopulateRepo_ExceedsHistorySize_OldestEntryDropped()
    {
        // Arrange
        SetupConfig(searchHistorySize: 2);
        var vm = CreateViewModel();

        // Act
        var first = new SearchResult { Files = [new() { Id = 1, Path = "a.jpg" }] };
        vm.PopulateRepo([new() { Id = 1, Path = "a.jpg" }]);
        vm.PopulateRepo([new() { Id = 2, Path = "b.jpg" }]);
        vm.PopulateRepo([new() { Id = 3, Path = "c.jpg" }]);

        // Assert
        Assert.Equal(2, vm.SearchResultHistory.Count);
        Assert.DoesNotContain(vm.SearchResultHistory, r => r.Files[0].Id == 1);
    }

    // --- Navigation ---

    [Fact]
    public void NextFileCommand_AdvancesSelection()
    {
        // Arrange
        var vm = CreateViewModel();
        vm.PopulateRepo(SomeFiles());
        Assert.Equal(0, vm.SelectedFileIndex);

        // Act
        vm.NextFile();

        // Assert
        Assert.Equal(1, vm.SelectedFileIndex);
    }

    [Fact]
    public void NextFileCommand_AtLastFile_DoesNotAdvance()
    {
        // Arrange
        var vm = CreateViewModel();
        vm.PopulateRepo([new() { Id = 1, Path = "only.jpg" }]);

        // Act
        vm.NextFile();

        // Assert
        Assert.Equal(0, vm.SelectedFileIndex);
    }

    [Fact]
    public void PrevFileAvailable_AtFirst_IsFalse()
    {
        // Arrange + Act
        var vm = CreateViewModel();
        vm.PopulateRepo(SomeFiles());

        // Assert
        Assert.Equal(0, vm.SelectedFileIndex);
        Assert.False(vm.PrevFileAvailable);
    }

    [Fact]
    public void NextFileAvailable_AtLast_IsFalse()
    {
        // Arrange + Act
        var vm = CreateViewModel();
        vm.PopulateRepo([new() { Id = 1, Path = "only.jpg" }]);

        // Assert
        Assert.False(vm.NextFileAvailable);
    }

    [Fact]
    public void NextFileAvailable_NotAtLast_IsTrue()
    {
        // Arrange + Act
        var vm = CreateViewModel();
        vm.PopulateRepo(SomeFiles());

        // Assert
        Assert.True(vm.NextFileAvailable);
    }

    // --- Sort ---

    [Fact]
    public void ChangeSort_ToDateDesc_ReversesDateOrder()
    {
        // Arrange
        SetupConfig(defaultSort: SortMethod.Date);
        var vm = CreateViewModel();
        var files = new List<FileModel>
        {
            new() { Id = 1, Path = "a.jpg", Datetime = "2000-01-01" },
            new() { Id = 2, Path = "b.jpg", Datetime = "2001-01-01" },
            new() { Id = 3, Path = "c.jpg", Datetime = "1999-01-01" },
        };
        vm.PopulateRepo(files);

        // Act
        vm.SelectedSortMethod = SortMethod.DateDesc;

        // Assert: newest first → id 2, id 1, id 3
        Assert.Equal(2, vm.Files.First().Id);
    }

    [Fact]
    public void ChangeSort_ToPath_SortsByPath()
    {
        // Arrange
        SetupConfig(defaultSort: SortMethod.Date);
        var vm = CreateViewModel();
        var files = new List<FileModel>
        {
            new() { Id = 3, Path = "c.jpg" },
            new() { Id = 1, Path = "a.jpg" },
            new() { Id = 2, Path = "b.jpg" },
        };
        vm.PopulateRepo(files);

        // Act
        vm.SelectedSortMethod = SortMethod.Path;

        // Assert
        Assert.Equal(1, vm.Files.First().Id);
    }

    [Fact]
    public void ChangeSort_ToDirectoryDate_SortsByDirectoryThenDate()
    {
        // Arrange
        SetupConfig(defaultSort: SortMethod.Path);
        var vm = CreateViewModel();
        var files = new List<FileModel>
        {
            new() { Id = 3, Path = "dirB/c.jpg", Datetime = "2000-01-01" },
            new() { Id = 1, Path = "dirA/a.jpg", Datetime = "1999-01-01" },
            new() { Id = 2, Path = "dirA/b.jpg", Datetime = "2001-01-01" },
        };
        vm.PopulateRepo(files);

        // Act
        vm.SelectedSortMethod = SortMethod.DirectoryDate;

        // Assert: dirA before dirB; within dirA, 1999 before 2001
        var sorted = vm.Files.ToList();
        Assert.Equal(1, sorted[0].Id);
        Assert.Equal(2, sorted[1].Id);
        Assert.Equal(3, sorted[2].Id);
    }

    [Fact]
    public void ChangeSort_ToDirectoryDateDesc_SortsByDirectoryDescThenDateDesc()
    {
        // Arrange
        SetupConfig(defaultSort: SortMethod.Path);
        var vm = CreateViewModel();
        var files = new List<FileModel>
        {
            new() { Id = 1, Path = "dirA/a.jpg", Datetime = "1999-01-01" },
            new() { Id = 2, Path = "dirA/b.jpg", Datetime = "2001-01-01" },
            new() { Id = 3, Path = "dirB/c.jpg", Datetime = "2000-01-01" },
        };
        vm.PopulateRepo(files);

        // Act
        vm.SelectedSortMethod = SortMethod.DirectoryDateDesc;

        // Assert: dirB before dirA (desc); within dirA, 2001 before 1999 (desc)
        var sorted = vm.Files.ToList();
        Assert.Equal(3, sorted[0].Id);
        Assert.Equal(2, sorted[1].Id);
        Assert.Equal(1, sorted[2].Id);
    }

    // --- ClearSearch ---

    [Fact]
    public void ClearSearchCommand_ClearsResult()
    {
        // Arrange
        var vm = CreateViewModel();
        vm.PopulateRepo(SomeFiles());
        Assert.True(vm.HasSearchResult);

        // Act
        vm.ClearSearchCommand.Execute(null);

        // Assert
        Assert.False(vm.HasSearchResult);
        Assert.Equal(0, vm.SearchNumberOfHits);
        Assert.Equal(-1, vm.SelectedFileIndex);
    }

    // --- RemoveFileFromRepo ---

    [Fact]
    public void RemoveFileFromRepo_SingleFile_ClearsSearchResult()
    {
        // Arrange
        var vm = CreateViewModel();
        var file = new FileModel { Id = 1, Path = "a.jpg" };
        vm.PopulateRepo([file]);

        // Act
        vm.RemoveFileFromRepo(file);

        // Assert
        Assert.False(vm.HasSearchResult);
    }

    [Fact]
    public void RemoveFileFromRepo_OneOfMany_AdjustsCount()
    {
        // Arrange
        var vm = CreateViewModel();
        var files = SomeFiles();
        vm.PopulateRepo(files);

        // Act
        vm.RemoveFileFromRepo(files[1]);

        // Assert
        Assert.Equal(2, vm.SearchNumberOfHits);
    }

    [Fact]
    public void RemoveFileFromRepo_FileNotInResult_NoChange()
    {
        // Arrange
        var vm = CreateViewModel();
        vm.PopulateRepo(SomeFiles());
        var outsider = new FileModel { Id = 99, Path = "other.jpg" };

        // Act
        vm.RemoveFileFromRepo(outsider);

        // Assert
        Assert.Equal(3, vm.SearchNumberOfHits);
    }

    // --- ISearchResultRepository ---

    [Fact]
    public void Files_NoSearchResult_ReturnsEmpty()
    {
        // Arrange + Act
        var vm = CreateViewModel();

        // Assert
        Assert.Empty(vm.Files);
    }

    [Fact]
    public void Files_WithSearchResult_ReturnsFiles()
    {
        // Arrange
        var vm = CreateViewModel();

        // Act
        vm.PopulateRepo(SomeFiles());

        // Assert
        Assert.Equal(3, vm.Files.Count());
    }
}
