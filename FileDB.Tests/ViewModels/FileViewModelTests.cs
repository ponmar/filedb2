using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Infrastructure;
using FileDB.Model;
using FileDB.Services;
using FileDB.ViewModels;
using FileDB.ViewModels.Search;
using FileDBInterface.Model;
using System.IO.Abstractions;
using Xunit;

namespace FileDB.Tests.ViewModels;

public class FileViewModelTests : IDisposable
{
    private readonly IConfigProvider fakeConfigProvider = A.Fake<IConfigProvider>();
    private readonly IDatabaseAccessProvider fakeDbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IFilesystemAccessProvider fakeFilesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
    private readonly IImageLoader fakeImageLoader = A.Fake<IImageLoader>();
    private readonly IFileSystem fakeFileSystem = A.Fake<IFileSystem>();
    private readonly IDialogs fakeDialogs = A.Fake<IDialogs>();
    private readonly IClipboardService fakeClipboardService = A.Fake<IClipboardService>();
    private readonly IFileSelector fakeFileSelector = A.Fake<IFileSelector>();
    private readonly ICriteriaViewModel fakeCriteriaViewModel = A.Fake<ICriteriaViewModel>();
    private readonly ISearchResultRepositoryManagement fakeSearchResultRepoManagement = A.Fake<ISearchResultRepositoryManagement>();
    private readonly IProcessUtils fakeProcessUtils = A.Fake<IProcessUtils>();

    private FileViewModel? _vm;

    private const string AbsolutePath = "/collection/file";

    public FileViewModelTests()
    {
        A.CallTo(() => fakeFilesystemAccessProvider.FilesystemAccess.ToAbsolutePath(A<string>._)).Returns(AbsolutePath);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetLocationsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetTagsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder().Build());
    }

    public void Dispose()
    {
        if (_vm is not null)
            Messenger.Unregister(_vm);
    }

    private FileViewModel CreateViewModel()
    {
        _vm = new(fakeConfigProvider, fakeDbAccessProvider, fakeFilesystemAccessProvider,
            fakeImageLoader, fakeFileSystem, fakeDialogs, fakeClipboardService,
            fakeFileSelector, fakeCriteriaViewModel, fakeSearchResultRepoManagement, fakeProcessUtils);
        return _vm;
    }

    private void SelectFile(FileModel file)
    {
        A.CallTo(() => fakeFileSelector.SelectedFile).Returns(file);
        Messenger.Send<FileSelectionChanged>();
    }

    private void CloseFile()
    {
        A.CallTo(() => fakeFileSelector.SelectedFile).Returns(null);
        Messenger.Send<FileSelectionChanged>();
    }

    // --- Constructor ---

    [Fact]
    public void Constructor_NoFileSelected_FileSelectedIsFalse()
    {
        var vm = CreateViewModel();

        Assert.False(vm.FileSelected);
        Assert.Null(vm.SelectedFile);
    }

    [Fact]
    public void Constructor_NoFileSelected_ImageIsNull()
    {
        var vm = CreateViewModel();

        Assert.Null(vm.Image);
    }

    [Fact]
    public void Constructor_NoFileSelected_TextContentIsNull()
    {
        var vm = CreateViewModel();

        Assert.Null(vm.TextContent);
    }

    [Fact]
    public void Constructor_NoFileSelected_FileLoadErrorIsEmpty()
    {
        var vm = CreateViewModel();

        Assert.Empty(vm.FileLoadError);
    }

    // --- Picture ---

    [Fact]
    public void SelectPictureFile_ImageLoaderIsCalled()
    {
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "photo.jpg" });

        A.CallTo(() => fakeImageLoader.LoadImage(AbsolutePath)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void SelectPictureFile_TextContentIsNull()
    {
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "photo.jpg" });

        Assert.Null(vm.TextContent);
    }

    [Fact]
    public void SelectPictureFile_FileLoadErrorIsEmpty()
    {
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "photo.jpg" });

        Assert.Empty(vm.FileLoadError);
    }

    [Fact]
    public void SelectPictureFile_FileSelectedIsTrue()
    {
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "photo.jpg" });

        Assert.True(vm.FileSelected);
        Assert.NotNull(vm.SelectedFile);
    }

    // --- .txt ---

    [Fact]
    public void SelectTxtFile_TextContentIsSetFromFile()
    {
        const string content = "Hello world!";
        A.CallTo(() => fakeFileSystem.File.ReadAllText(AbsolutePath)).Returns(content);
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "notes.txt" });

        Assert.Equal(content, vm.TextContent);
    }

    [Fact]
    public void SelectTxtFile_ImageIsNull()
    {
        A.CallTo(() => fakeFileSystem.File.ReadAllText(AbsolutePath)).Returns("content");
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "notes.txt" });

        Assert.Null(vm.Image);
    }

    [Fact]
    public void SelectTxtFile_ImageLoaderIsNotCalled()
    {
        A.CallTo(() => fakeFileSystem.File.ReadAllText(AbsolutePath)).Returns("content");
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "notes.txt" });

        A.CallTo(() => fakeImageLoader.LoadImage(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void SelectTxtFile_FileLoadErrorIsEmpty()
    {
        A.CallTo(() => fakeFileSystem.File.ReadAllText(AbsolutePath)).Returns("content");
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "notes.txt" });

        Assert.Empty(vm.FileLoadError);
    }

    [Fact]
    public void SelectTxtFile_ReadFails_FileLoadErrorIsSet()
    {
        A.CallTo(() => fakeFileSystem.File.ReadAllText(AbsolutePath)).Throws<IOException>();
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "notes.txt" });

        Assert.NotEmpty(vm.FileLoadError);
        Assert.Null(vm.TextContent);
    }

    // --- .md ---

    [Fact]
    public void SelectMdFile_TextContentIsSetFromFile()
    {
        const string content = "# Title\nSome markdown.";
        A.CallTo(() => fakeFileSystem.File.ReadAllText(AbsolutePath)).Returns(content);
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "readme.md" });

        Assert.Equal(content, vm.TextContent);
    }

    [Fact]
    public void SelectMdFile_FileLoadErrorIsEmpty()
    {
        A.CallTo(() => fakeFileSystem.File.ReadAllText(AbsolutePath)).Returns("# Title");
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "readme.md" });

        Assert.Empty(vm.FileLoadError);
    }

    // --- Unsupported file types ---

    [Fact]
    public void SelectMovieFile_FileTypeNotSupportedError()
    {
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "video.mp4" });

        Assert.NotEmpty(vm.FileLoadError);
        Assert.Null(vm.TextContent);
    }

    [Fact]
    public void SelectPdfFile_FileTypeNotSupportedError()
    {
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "document.pdf" });

        Assert.NotEmpty(vm.FileLoadError);
        Assert.Null(vm.TextContent);
    }

    [Fact]
    public void SelectAudioFile_FileTypeNotSupportedError()
    {
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "song.mp3" });

        Assert.NotEmpty(vm.FileLoadError);
        Assert.Null(vm.TextContent);
    }

    // --- Close ---

    [Fact]
    public void CloseFile_AfterTxtFileSelected_TextContentBecomesNull()
    {
        A.CallTo(() => fakeFileSystem.File.ReadAllText(AbsolutePath)).Returns("content");
        var vm = CreateViewModel();
        SelectFile(new FileModel { Id = 1, Path = "notes.txt" });
        Assert.NotNull(vm.TextContent);

        CloseFile();

        Assert.Null(vm.TextContent);
    }

    [Fact]
    public void CloseFile_SelectedFileBecomesNull()
    {
        var vm = CreateViewModel();
        SelectFile(new FileModel { Id = 1, Path = "photo.jpg" });
        Assert.NotNull(vm.SelectedFile);

        CloseFile();

        Assert.Null(vm.SelectedFile);
    }

    [Fact]
    public void CloseFile_FileSelectedIsFalse()
    {
        var vm = CreateViewModel();
        SelectFile(new FileModel { Id = 1, Path = "photo.jpg" });

        CloseFile();

        Assert.False(vm.FileSelected);
    }

    // --- Metadata ---

    [Fact]
    public void SelectFile_InternalPathIsSet()
    {
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "photo.jpg" });

        Assert.Equal("photo.jpg", vm.InternalPath);
    }

    [Fact]
    public void SelectFile_DescriptionIsSet()
    {
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "photo.jpg", Description = "A nice photo" });

        Assert.Equal("A nice photo", vm.Description);
    }

    [Fact]
    public void SelectFile_NoDescription_DescriptionIsEmpty()
    {
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "photo.jpg", Description = null });

        Assert.Equal(string.Empty, vm.Description);
    }

    [Fact]
    public void FileEdited_RefreshesPersonsLocationsAndTags()
    {
        var persons = new List<PersonModel>();
        var locations = new List<LocationModel>();
        var tags = new List<TagModel>();
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetPersonsFromFile(1)).Returns(persons);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetLocationsFromFile(1)).Returns(locations);
        A.CallTo(() => fakeDbAccessProvider.DbAccess.GetTagsFromFile(1)).Returns(tags);
        var vm = CreateViewModel();

        SelectFile(new FileModel { Id = 1, Path = "photo.jpg" });

        persons.Add(new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith" });
        locations.Add(new LocationModel { Id = 2, Name = "Home" });
        tags.Add(new TagModel { Id = 3, Name = "Favorites" });
        Messenger.Send<FileEdited>();

        Assert.Collection(vm.Persons, person => Assert.Equal("Alice Smith", person.Label));
        Assert.Collection(vm.Locations, location => Assert.Equal("Home", location.Name));
        Assert.Collection(vm.Tags, tag => Assert.Equal("Favorites", tag.Name));
    }

    [Fact]
    public void CloseFile_InternalPathIsCleared()
    {
        var vm = CreateViewModel();
        SelectFile(new FileModel { Id = 1, Path = "photo.jpg" });

        CloseFile();

        Assert.Equal(string.Empty, vm.InternalPath);
    }

    [Fact]
    public void Maximize_SendsFullscreenBrowsingRequested()
    {
        var recorder = new SingleEventRecorder<FullscreenBrowsingRequested>();
        var vm = CreateViewModel();

        vm.Maximize = true;

        var message = recorder.AssertEventRecorded();
        Assert.True(message.Fullscreen);
    }

    [Fact]
    public void OpenFileLocationCommand_OpensExistingFile()
    {
        A.CallTo(() => fakeFileSystem.File.Exists(AbsolutePath)).Returns(true);
        var vm = CreateViewModel();
        SelectFile(new FileModel { Id = 1, Path = "photo.jpg" });

        vm.OpenFileLocationCommand.Execute(null);

        A.CallTo(() => fakeProcessUtils.SelectFileInExplorer(AbsolutePath)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void OpenFileWithDefaultAppCommand_OpensExistingFile()
    {
        A.CallTo(() => fakeFileSystem.File.Exists(AbsolutePath)).Returns(true);
        var vm = CreateViewModel();
        SelectFile(new FileModel { Id = 1, Path = "photo.jpg" });

        vm.OpenFileWithDefaultAppCommand.Execute(null);

        A.CallTo(() => fakeProcessUtils.OpenFileWithDefaultApp(AbsolutePath)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CopyFileIdCommand_CopiesSelectedFile()
    {
        var vm = CreateViewModel();
        SelectFile(new FileModel { Id = 1, Path = "photo.jpg" });

        vm.CopyFileIdCommand.Execute(null);

        A.CallTo(() => fakeClipboardService.SetTextAsync(A<string>.That.Contains("1"))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void RemoveFileFromCurrentSearchResultCommand_RemovesSelectedFile()
    {
        var vm = CreateViewModel();
        SelectFile(new FileModel { Id = 1, Path = "photo.jpg" });

        vm.RemoveFileFromCurrentSearchResultCommand.Execute(null);

        A.CallTo(() => fakeSearchResultRepoManagement.RemoveFileFromRepo(A<FileModel>.That.Matches(x => x.Id == 1))).MustHaveHappenedOnceExactly();
    }
}
