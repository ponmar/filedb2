using FakeItEasy;
using FileDBInterface.Model;
using FileDB.Model;
using FileDB.Dialogs;
using FileDB;
using FileDB.ViewModels.Search.File;
using FileDB.ViewModels.Search;
using Xunit;
using FileDBInterface.FilesystemAccess;
using FileDB.Infrastructure;

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
        A.CallTo(() => configProvider.Config).Returns(new ConfigBuilder
        {
            ReadOnly = false,
            ShortItemNameMaxLength = 15,
        }.Build());

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
    public void Constructor_ReadOnly_DisablesCategorization()
    {
        A.CallTo(() => configProvider.Config).Returns(new ConfigBuilder { ReadOnly = true }.Build());
        var viewModel = CreateViewModel();

        Assert.True(viewModel.ReadOnly);
        Assert.False(viewModel.CanCategorize);
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

    [Fact]
    public async Task ToggleCombinedCommand_PersonAddCancelled_DoesNotCheckItem()
    {
        // Arrange
        var person = new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith", DateOfBirth = "2025-01-01" };
        persons.Add(person);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonById(person.Id)).Returns(person);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(false);

        var viewModel = CreateViewModel();
        LoadAFile(datetime: "2020-01-01");
        var item = viewModel.Items.First(x => x.Type == CombinedItemType.Person && x.Id == person.Id);

        // Act
        await viewModel.ToggleCombinedCommand.ExecuteAsync(item);

        // Assert
        A.CallTo(() => dbAccessProvider.DbAccess.InsertFilePerson(A<int>._, A<int>._)).MustNotHaveHappened();
        Assert.False(item.IsChecked);
        eventRecorder.AssertNoEventsRecorded();
    }

    [Fact]
    public async Task ToggleCombinedCommand_PersonAddSucceeded_AddsItem()
    {
        var person = new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith", DateOfBirth = "2025-01-01" };
        persons.Add(person);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonById(person.Id)).Returns(person);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([]);

        var viewModel = CreateViewModel();
        LoadAFile(datetime: null);
        var item = viewModel.Items.First(x => x.Type == CombinedItemType.Person && x.Id == person.Id);

        await viewModel.ToggleCombinedCommand.ExecuteAsync(item);

        A.CallTo(() => dbAccessProvider.DbAccess.InsertFilePerson(1, 1)).MustHaveHappenedOnceExactly();
        Assert.True(item.IsChecked);
        Assert.Single(viewModel.UpdateHistoryItems);
    }

    [Fact]
    public async Task ToggleCombinedCommand_TagRemove_RemovesItem()
    {
        var tag = new TagModel { Id = 1, Name = "Favorites" };
        tags.Add(tag);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTagById(tag.Id)).Returns(tag);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTagsFromFile(A<int>._)).Returns([tag]);

        var viewModel = CreateViewModel();
        LoadAFile();
        var item = viewModel.Items.First(x => x.Type == CombinedItemType.Tag && x.Id == tag.Id);

        await viewModel.ToggleCombinedCommand.ExecuteAsync(item);

        A.CallTo(() => dbAccessProvider.DbAccess.DeleteFileTag(1, 1)).MustHaveHappenedOnceExactly();
        Assert.False(item.IsChecked);
    }

    [Fact]
    public async Task AddItemsCommand_AddsVisibleUncheckedItems()
    {
        PopulateRepositories();
        var person = new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith", DateOfBirth = "2025-01-01" };
        var location = new LocationModel { Id = 1, Name = "Home" };
        var tag = new TagModel { Id = 1, Name = "Favorites" };
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonById(person.Id)).Returns(person);
        A.CallTo(() => dbAccessProvider.DbAccess.GetLocationById(location.Id)).Returns(location);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTagById(tag.Id)).Returns(tag);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetLocationsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTagsFromFile(A<int>._)).Returns([]);

        var viewModel = CreateViewModel();
        LoadAFile(datetime: null);
        foreach (var item in viewModel.Items)
        {
            item.IsVisible = false;
        }
        viewModel.Items.First(x => x.Type == CombinedItemType.Person && x.Id == 1).IsVisible = true;
        viewModel.Items.First(x => x.Type == CombinedItemType.Location && x.Id == 1).IsVisible = true;
        viewModel.Items.First(x => x.Type == CombinedItemType.Tag && x.Id == 1).IsVisible = true;

        await viewModel.AddItemsCommand.ExecuteAsync(null);

        A.CallTo(() => dbAccessProvider.DbAccess.InsertFilePerson(1, 1)).MustHaveHappenedOnceExactly();
        A.CallTo(() => dbAccessProvider.DbAccess.InsertFileLocation(1, 1)).MustHaveHappenedOnceExactly();
        A.CallTo(() => dbAccessProvider.DbAccess.InsertFileTag(1, 1)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void RemoveItemsCommand_RemovesVisibleCheckedItems()
    {
        PopulateRepositories();
        var person = new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith" };
        var location = new LocationModel { Id = 1, Name = "Home" };
        var tag = new TagModel { Id = 1, Name = "Favorites" };
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonById(person.Id)).Returns(person);
        A.CallTo(() => dbAccessProvider.DbAccess.GetLocationById(location.Id)).Returns(location);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTagById(tag.Id)).Returns(tag);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([person]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetLocationsFromFile(A<int>._)).Returns([location]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTagsFromFile(A<int>._)).Returns([tag]);

        var viewModel = CreateViewModel();
        LoadAFile();
        foreach (var item in viewModel.Items)
        {
            item.IsVisible = false;
        }
        viewModel.Items.First(x => x.Type == CombinedItemType.Person && x.Id == 1).IsVisible = true;
        viewModel.Items.First(x => x.Type == CombinedItemType.Location && x.Id == 1).IsVisible = true;
        viewModel.Items.First(x => x.Type == CombinedItemType.Tag && x.Id == 1).IsVisible = true;

        viewModel.RemoveItemsCommand.Execute(null);

        A.CallTo(() => dbAccessProvider.DbAccess.DeleteFilePerson(1, 1)).MustHaveHappenedOnceExactly();
        A.CallTo(() => dbAccessProvider.DbAccess.DeleteFileLocation(1, 1)).MustHaveHappenedOnceExactly();
        A.CallTo(() => dbAccessProvider.DbAccess.DeleteFileTag(1, 1)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void ReloadCommand_UserDeclines_DoesNotUpdate()
    {
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(false);
        var viewModel = CreateViewModel();
        LoadAFile();

        viewModel.ReloadCommand.Execute(null);

        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFileFromMetaData(A<int>._, A<IFilesystemAccess>._)).MustNotHaveHappened();
    }

    [Fact]
    public void PlaceBoundingBox_WhenMissing_StartsPlacement()
    {
        var person = new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith" };
        persons.Add(person);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetFilePersonBoundingBoxes(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonById(person.Id)).Returns(person);
        var recorder = new SingleEventRecorder<StartPersonBoundingBoxPlacement>();

        var viewModel = CreateViewModel();
        LoadAFile();
        var item = viewModel.Items.First(x => x.Type == CombinedItemType.Person && x.Id == person.Id);

        viewModel.PlaceBoundingBoxCommand.Execute(item);

        recorder.AssertEventRecorded();
    }

    [Fact]
    public void PlaceBoundingBox_WhenBoxExists_ClearsOnlyBoundingBox()
    {
        // Arrange
        var person = new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith" };
        persons.Add(person);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([person]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetFilePersonBoundingBoxes(A<int>._)).Returns([(person.Id, new PersonBoundingBox(0.1, 0.2, 0.3, 0.4))]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonById(person.Id)).Returns(person);

        var viewModel = CreateViewModel();
        LoadAFile();
        var item = viewModel.Items.First(x => x.Type == CombinedItemType.Person && x.Id == person.Id);
        Assert.True(item.IsChecked);
        Assert.True(item.HasBoundingBox);

        // Act
        viewModel.PlaceBoundingBoxCommand.Execute(item);

        // Assert
        A.CallTo(() => dbAccessProvider.DbAccess.UpdateFilePersonBoundingBox(1, person.Id, null)).MustHaveHappenedOnceExactly();
        A.CallTo(() => dbAccessProvider.DbAccess.DeleteFilePerson(A<int>._, A<int>._)).MustNotHaveHappened();
        Assert.True(item.IsChecked);
        Assert.False(item.HasBoundingBox);
    }

    [Fact]
    public void MarkPrevEditedFileCommand_SetsPreviousFile()
    {
        var viewModel = CreateViewModel();
        LoadAFile();

        viewModel.MarkPrevEditedFileCommand.Execute(null);

        Assert.Equal(1, viewModel.PrevEditedFileId);
    }

    [Fact]
    public async Task RemoveHistoryItemCommand_RemovesItem()
    {
        var person = new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith" };
        persons.Add(person);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetLocationsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTagsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetFilePersonBoundingBoxes(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonById(person.Id)).Returns(person);

        var viewModel = CreateViewModel();
        LoadAFile();
        var item = viewModel.Items.First(x => x.Type == CombinedItemType.Person && x.Id == person.Id);
        await viewModel.ToggleCombinedCommand.ExecuteAsync(item);
        Assert.NotEmpty(viewModel.UpdateHistoryItems);

        var itemToRemove = viewModel.UpdateHistoryItems[0];
        viewModel.RemoveHistoryItemCommand.Execute(itemToRemove);

        Assert.Empty(viewModel.UpdateHistoryItems);
    }

    [Fact]
    public void RotateFileClockwiseCommand_UpdatesRotation()
    {
        var person = new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith" };
        persons.Add(person);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetLocationsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTagsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetFilePersonBoundingBoxes(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonById(person.Id)).Returns(person);
        A.CallTo(() => fileRotator.Rotate(A<FileModel>._, A<int>._, RotationDirection.Clockwise)).Returns(90);

        var viewModel = CreateViewModel();
        LoadAFile();

        viewModel.RotateFileClockwiseCommand.Execute(null);

        A.CallTo(() => fileRotator.Rotate(A<FileModel>._, A<int>._, RotationDirection.Clockwise)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void RotateFileCounterClockwiseCommand_UpdatesRotation()
    {
        var person = new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith" };
        persons.Add(person);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetLocationsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetTagsFromFile(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetFilePersonBoundingBoxes(A<int>._)).Returns([]);
        A.CallTo(() => dbAccessProvider.DbAccess.GetPersonById(person.Id)).Returns(person);
        A.CallTo(() => fileRotator.Rotate(A<FileModel>._, A<int>._, RotationDirection.CounterClockwise)).Returns(270);

        var viewModel = CreateViewModel();
        LoadAFile();

        viewModel.RotateFileCounterClockwiseCommand.Execute(null);

        A.CallTo(() => fileRotator.Rotate(A<FileModel>._, A<int>._, RotationDirection.CounterClockwise)).MustHaveHappenedOnceExactly();
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

    private void LoadAFile(int fileId = 1, string? datetime = null)
    {
        A.CallTo(() => fileSelector.SelectedFile).Returns(new FileModel() { Id = fileId, Path = "file.jpg", Datetime = datetime });
        Messenger.Send<FileSelectionChanged>();
    }
}
