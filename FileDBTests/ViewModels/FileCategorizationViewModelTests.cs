using FakeItEasy;
using FileDBInterface.Model;
using FileDB.Model;
using FileDB.Dialogs;
using FileDB;
using FileDB.ViewModels.Search.File;
using FileDB.ViewModels.Search;
using Xunit;
using FileDBInterface.FilesystemAccess;

namespace FileDBTests.ViewModels;

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
    private readonly IFileRotator fileRotator = A.Fake<IFileRotator>();

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
        Assert.Empty(viewModel.Items);

        Assert.Empty(viewModel.UpdateHistoryItems);
        Assert.Null(viewModel.PrevEditedFileId);
    }

    [Fact]
    public void Constructor_ReposFilled()
    {
        PopulateRepositories();
        var viewModel = CreateViewModel();

        Assert.Equal(2, viewModel.Items.Count(x => x.Type == CombinedItemType.Person));
        Assert.Equal(2, viewModel.Items.Count(x => x.Type == CombinedItemType.Location));
        Assert.Equal(1, viewModel.Items.Count(x => x.Type == CombinedItemType.Tag));

        Assert.Empty(viewModel.UpdateHistoryItems);
        Assert.Null(viewModel.PrevEditedFileId);
    }

    [Fact]
    public void SelectSearchResultFile()
    {
        _ = CreateViewModel();
        LoadAFile();
    }

    [Fact]
    public void PersonUpdated()
    {
        var viewModel = CreateViewModel();
        Assert.Empty(viewModel.Items);

        persons.Add(new() { Id = 1, ShortName = "Alice", FullName = "Alice Smith" });
        Messenger.Send<PersonsUpdated>();

        Assert.Single(viewModel.Items);
    }

    [Fact]
    public void LocationsUpdated()
    {
        var viewModel = CreateViewModel();
        Assert.Empty(viewModel.Items);

        locations.Add(new() { Id = 1, Name = "Home" });
        Messenger.Send<LocationsUpdated>();

        Assert.Single(viewModel.Items);
    }

    [Fact]
    public void TagsUpdated()
    {
        var viewModel = CreateViewModel();
        Assert.Empty(viewModel.Items);

        tags.Add(new() { Id = 1, Name = "Favorites" });
        Messenger.Send<TagsUpdated>();

        Assert.Single(viewModel.Items);
    }

    [Fact]
    public void SaveCommand_NewDescription_DatabaseUpdated()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var editedFileId = 1;
        LoadAFile(editedFileId);

        Assert.False(viewModel.CanSave);
        Assert.False(viewModel.IsDirty);

        var newDescription = "New desciption";
        viewModel.NewFileDescription = newDescription;

        Assert.True(viewModel.CanSave);
        Assert.True(viewModel.IsDirty);

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFileDescription(editedFileId, newDescription)).MustHaveHappened();
        eventRecorder.AssertEventRecorded<FileEdited>();
        Assert.Equal(editedFileId, viewModel.PrevEditedFileId);
        Assert.Empty(viewModel.UpdateHistoryItems);
    }

    [Fact]
    public void SaveCommand_NewFileDateTime_DatabaseUpdated()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var editedFileId = 1;
        LoadAFile(editedFileId);

        Assert.False(viewModel.CanSave);
        Assert.False(viewModel.IsDirty);

        var newFileDateTime = "2025-09-26";
        viewModel.NewFileDateTime = newFileDateTime;

        Assert.True(viewModel.CanSave);
        Assert.True(viewModel.IsDirty);

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFileDatetime(editedFileId, newFileDateTime)).MustHaveHappened();
        eventRecorder.AssertEventRecorded<FileEdited>();
        Assert.Equal(editedFileId, viewModel.PrevEditedFileId);
        Assert.Empty(viewModel.UpdateHistoryItems);
    }

    [Fact]
    public void ResetCommand_NewFileDescription_IsReset()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var editedFileId = 1;
        LoadAFile(editedFileId);

        Assert.False(viewModel.CanSave);
        Assert.False(viewModel.IsDirty);
        Assert.Equal(string.Empty, viewModel.NewFileDescription);

        viewModel.NewFileDescription = "New desciption";

        Assert.True(viewModel.CanSave);
        Assert.True(viewModel.IsDirty);

        // Act
        viewModel.ResetCommand.Execute(null);

        // Assert
        Assert.False(viewModel.IsDirty);
        Assert.False(viewModel.CanSave);
        Assert.Equal(string.Empty, viewModel.NewFileDescription);
    }

    [Fact]
    public void ResetCommand_NewFileDateTime_IsReset()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var editedFileId = 1;
        LoadAFile(editedFileId);

        Assert.False(viewModel.CanSave);
        Assert.False(viewModel.IsDirty);
        Assert.Equal(string.Empty, viewModel.NewFileDateTime);

        viewModel.NewFileDateTime = "2025-09-26";

        Assert.True(viewModel.CanSave);
        Assert.True(viewModel.IsDirty);

        // Act
        viewModel.ResetCommand.Execute(null);

        // Assert
        Assert.False(viewModel.IsDirty);
        Assert.False(viewModel.CanSave);
        Assert.Equal(string.Empty, viewModel.NewFileDateTime);
    }

    [Fact]
    public void ReloadCommand()
    {
        // Arrange
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(true);

        var viewModel = CreateViewModel();
        var editedFileId = 1;
        LoadAFile(editedFileId);

        var reloadedFile = new FileModel() { Id = editedFileId, Path = "file.jpg", Description = "desc", Datetime = "2025-09-26", Position = "12.34 56.78", Orientation = 1 };
        A.CallTo(() => dbAccessProvider.DbAccess.GetFileById(editedFileId)).Returns(reloadedFile);

        // Act
        viewModel.ReloadCommand.Execute(null);

        // Arrange
        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFileFromMetaData(editedFileId, A<IFilesystemAccess>._)).MustHaveHappened();
        eventRecorder.AssertEventRecorded<FileEdited>();

        // Act
        Messenger.Send<FileSelectionChanged>();

        // Assert
        Assert.Equal(reloadedFile.Datetime, viewModel.NewFileDateTime);
        Assert.Equal(FileTextOverlayCreator.GetShortPositionText(reloadedFile), viewModel.FilePosition);
        Assert.Equal(DatabaseParsing.OrientationToDegrees(reloadedFile.Orientation), viewModel.ImageRotation);
    }

    [Fact]
    public async Task ToggleVisibleItemsCommand_EmptyFilter_DoesNothing()
    {
        // Arrange
        PopulateRepositories();
        var viewModel = CreateViewModel();
        LoadAFile();

        viewModel.ItemsFilterText = string.Empty;

        // Act
        await viewModel.ToggleVisibleItemsCommand.ExecuteAsync(null);

        // Assert
        A.CallTo(() => dbAccessProvider.DbAccess.InsertFileTag(A<int>._, A<int>._)).MustNotHaveHappened();
        A.CallTo(() => dbAccessProvider.DbAccess.InsertFileLocation(A<int>._, A<int>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ToggleVisibleItemsCommand_WithFilter_TogglesVisibleItems()
    {
        // Arrange: one unchecked tag visible via filter
        var tag = new TagModel() { Id = 1, Name = "Favorites" };
        tags.Add(tag);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTagById(tag.Id)).Returns(tag);

        var viewModel = CreateViewModel();
        LoadAFile();

        viewModel.ItemsFilterText = "Favor";

        // Act
        await viewModel.ToggleVisibleItemsCommand.ExecuteAsync(null);

        // Assert: unchecked visible tag is added
        A.CallTo(() => dbAccessProvider.DbAccess.InsertFileTag(1, 1)).MustHaveHappened();
        A.CallTo(() => dbAccessProvider.DbAccess.InsertFileLocation(A<int>._, A<int>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ToggleVisibleItemsCommand_WithFilter_AlreadyCheckedItemIsNotRemoved()
    {
        // Arrange: tag already on file (starts checked)
        var tag = new TagModel() { Id = 1, Name = "Favorites" };
        tags.Add(tag);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTagById(tag.Id)).Returns(tag);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTagsFromFile(A<int>._)).Returns([tag]);

        var viewModel = CreateViewModel();
        LoadAFile();

        viewModel.ItemsFilterText = "Favor";

        // Act
        await viewModel.ToggleVisibleItemsCommand.ExecuteAsync(null);

        // Assert: already-checked visible tag is not removed
        A.CallTo(() => dbAccessProvider.DbAccess.DeleteFileTag(A<int>._, A<int>._)).MustNotHaveHappened();
        A.CallTo(() => dbAccessProvider.DbAccess.InsertFileTag(A<int>._, A<int>._)).MustNotHaveHappened();
    }

    private FileCategorizationViewModel CreateViewModel()
    {
        return new FileCategorizationViewModel(configProvider, dbAccessProvider, dialogs, filesystemAccessProvider, personsRepository, locationsRepository, tagsRepository, fileSelector, fileRotator);
    }

    private void PopulateRepositories()
    {
        persons.Clear();
        persons.Add(new() { Id = 1, ShortName = "Alice", FullName = "Alice Smith" });
        persons.Add(new() { Id = 2, ShortName = "Bob", FullName = "Bob Smith" });

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
