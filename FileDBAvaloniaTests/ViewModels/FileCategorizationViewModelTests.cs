using FakeItEasy;
using FileDBInterface.Model;
using FileDBAvalonia.Model;
using FileDBAvalonia.Dialogs;
using FileDBAvalonia;
using FileDBAvalonia.ViewModels.Search.File;
using FileDBAvalonia.ViewModels.Search;
using Xunit;

namespace FileDBAvaloniaTests.ViewModels;

public class FileCategorizationViewModelTests
{
    private readonly IConfigProvider configProvider = A.Fake<IConfigProvider>();
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly IFilesystemAccessProvider filesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
    private readonly IPersonsRepository personsRepository = A.Fake<IPersonsRepository>();
    private readonly ILocationsRepository locationsRepository = A.Fake<ILocationsRepository>();
    private readonly ITagsRepository tagsRepository = A.Fake<ITagsRepository>();
    private readonly IFileSelector fileSelector = A.Fake<IFileSelector>();

    private readonly List<PersonModel> persons = [];
    private readonly List<LocationModel> locations = [];
    private readonly List<TagModel> tags = [];

    private readonly EventRecorder eventRecorder = new();

    public FileCategorizationViewModelTests()
    {
        persons.Clear();
        locations.Clear();
        tags.Clear();

        A.CallTo(() => personsRepository.Persons).Returns(persons);
        A.CallTo(() => locationsRepository.Locations).Returns(locations);
        A.CallTo(() => tagsRepository.Tags).Returns(tags);

        eventRecorder.Reset();
        eventRecorder.Record<FileEdited>();
    }

    [Fact]
    public void Constructor_ReposEmpty()
    {
        var viewModel = CreateViewModel();

        Assert.Empty(viewModel.Persons);
        Assert.Empty(viewModel.Locations);
        Assert.Empty(viewModel.Tags);

        Assert.Empty(viewModel.UpdateHistoryItems);
        Assert.Null(viewModel.PrevEditedFileId);

        Assert.False(viewModel.SelectedPersonCanBeAdded);
        Assert.False(viewModel.SelectedPersonCanBeRemoved);

        Assert.False(viewModel.SelectedLocationCanBeAdded);
        Assert.False(viewModel.SelectedLocationCanBeRemoved);

        Assert.False(viewModel.SelectedTagCanBeAdded);
        Assert.False(viewModel.SelectedTagCanBeRemoved);
    }

    [Fact]
    public void Constructor_ReposFilled()
    {
        PopulateRepositories();
        var viewModel = CreateViewModel();

        Assert.Equal(2, viewModel.Persons.Count);
        Assert.Equal(2, viewModel.Locations.Count);
        Assert.Single(viewModel.Tags);

        Assert.Empty(viewModel.UpdateHistoryItems);
        Assert.Null(viewModel.PrevEditedFileId);

        Assert.False(viewModel.SelectedPersonCanBeAdded);
        Assert.False(viewModel.SelectedPersonCanBeRemoved);

        Assert.False(viewModel.SelectedLocationCanBeAdded);
        Assert.False(viewModel.SelectedLocationCanBeRemoved);

        Assert.False(viewModel.SelectedTagCanBeAdded);
        Assert.False(viewModel.SelectedTagCanBeRemoved);
    }

    [Fact]
    public void SelectSearchResultFile()
    {
        var viewModel = CreateViewModel();
        LoadAFile();
    }

    [Fact]
    public void PersonUpdated()
    {
        var viewModel = CreateViewModel();
        Assert.Empty(viewModel.Persons);

        persons.Add(new() { Id = 1, Firstname = "Alice", Lastname = "Smith" });
        Messenger.Send<PersonsUpdated>();

        Assert.Single(viewModel.Persons);
    }

    [Fact]
    public void LocationsUpdated()
    {
        var viewModel = CreateViewModel();
        Assert.Empty(viewModel.Locations);

        locations.Add(new() { Id = 1, Name = "Home" });
        Messenger.Send<LocationsUpdated>();

        Assert.Single(viewModel.Locations);
    }

    [Fact]
    public void TagsUpdated()
    {
        var viewModel = CreateViewModel();
        Assert.Empty(viewModel.Tags);

        tags.Add(new() { Id = 1, Name = "Favorites" });
        Messenger.Send<TagsUpdated>();

        Assert.Single(viewModel.Tags);
    }

    [Fact]
    public void SetFileDescription()
    {
        var viewModel = CreateViewModel();

        var editedFileId = 1;
        LoadAFile(editedFileId);

        var newDescription = "New desciption";
        viewModel.NewFileDescription = newDescription;
        viewModel.SetFileDescriptionCommand.Execute(null);

        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFileDescription(editedFileId, newDescription)).MustHaveHappened();
        eventRecorder.AssertEventRecorded<FileEdited>();
        Assert.Equal(editedFileId, viewModel.PrevEditedFileId);
        Assert.Empty(viewModel.UpdateHistoryItems);
    }

    private FileCategorizationViewModel CreateViewModel()
    {
        return new FileCategorizationViewModel(configProvider, dbAccessProvider, dialogs, filesystemAccessProvider, personsRepository, locationsRepository, tagsRepository, fileSelector);
    }

    private void PopulateRepositories()
    {
        persons.Clear();
        persons.Add(new() { Id = 1, Firstname = "Alice", Lastname = "Smith" });
        persons.Add(new() { Id = 2, Firstname = "Bob", Lastname = "Smith" });

        locations.Clear();
        locations.Add(new() { Id = 1, Name = "Home" });
        locations.Add(new() { Id = 2, Name = "Work" });

        tags.Clear();
        tags.Add(new() { Id = 1, Name = "Favorites" });
    }

    private void LoadAFile(int fileId = 1)
    {
        A.CallTo(() => fileSelector.SelectedFile).Returns(new FileModel() { Id = fileId, Path = "file.jpg", });
        Messenger.Send<FileSelectionChanged>();
    }
}
