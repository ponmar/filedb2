﻿using FileDB.Model;
using FileDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FakeItEasy;
using FileDB.ViewModel;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDBTests.ViewModel;

[TestClass]
public class FileCategorizationViewModelTests
{
    private IConfigRepository configRepository;
    private IDbAccessRepository dbAccessRepository;
    private IDialogs dialogs;
    private IFilesystemAccessRepository filesystemAccessRepository;
    private IPersonsRepository personsRepository;
    private ILocationsRepository locationsRepository;
    private ITagsRepository tagsRepository;

    private readonly List<PersonModel> persons = new();
    private readonly List<LocationModel> locations = new();
    private readonly List<TagModel> tags = new();

    private readonly EventRecorder eventRecorder = new();

    [TestInitialize]
    public void Init()
    {
        configRepository = A.Fake<IConfigRepository>();
        dbAccessRepository = A.Fake<IDbAccessRepository>();
        dialogs = A.Fake<IDialogs>();
        filesystemAccessRepository = A.Fake<IFilesystemAccessRepository>();
        personsRepository = A.Fake<IPersonsRepository>();
        locationsRepository = A.Fake<ILocationsRepository>();
        tagsRepository = A.Fake<ITagsRepository>();

        persons.Clear();
        locations.Clear();
        tags.Clear();

        A.CallTo(() => personsRepository.Persons).Returns(persons);
        A.CallTo(() => locationsRepository.Locations).Returns(locations);
        A.CallTo(() => tagsRepository.Tags).Returns(tags);

        eventRecorder.Reset();
        eventRecorder.Record<FileEdited>();
    }

    [TestMethod]
    public void Constructor_ReposEmpty()
    {
        var viewModel = CreateViewModel();

        Assert.AreEqual(0, viewModel.Persons.Count);
        Assert.AreEqual(0, viewModel.Locations.Count);
        Assert.AreEqual(0, viewModel.Tags.Count);

        Assert.AreEqual(0, viewModel.UpdateHistoryItems.Count);
        Assert.IsNull(viewModel.PrevEditedFileId);

        Assert.IsFalse(viewModel.SelectedPersonCanBeAdded);
        Assert.IsFalse(viewModel.SelectedPersonCanBeRemoved);

        Assert.IsFalse(viewModel.SelectedLocationCanBeAdded);
        Assert.IsFalse(viewModel.SelectedLocationCanBeRemoved);

        Assert.IsFalse(viewModel.SelectedTagCanBeAdded);
        Assert.IsFalse(viewModel.SelectedTagCanBeRemoved);
    }

    [TestMethod]
    public void Constructor_ReposFilled()
    {
        PopulateRepositories();
        var viewModel = CreateViewModel();

        Assert.AreEqual(2, viewModel.Persons.Count);
        Assert.AreEqual(2, viewModel.Locations.Count);
        Assert.AreEqual(1, viewModel.Tags.Count);

        Assert.AreEqual(0, viewModel.UpdateHistoryItems.Count);
        Assert.IsNull(viewModel.PrevEditedFileId);

        Assert.IsFalse(viewModel.SelectedPersonCanBeAdded);
        Assert.IsFalse(viewModel.SelectedPersonCanBeRemoved);

        Assert.IsFalse(viewModel.SelectedLocationCanBeAdded);
        Assert.IsFalse(viewModel.SelectedLocationCanBeRemoved);

        Assert.IsFalse(viewModel.SelectedTagCanBeAdded);
        Assert.IsFalse(viewModel.SelectedTagCanBeRemoved);
    }

    [TestMethod]
    public void SelectSearchResultFile()
    {
        var viewModel = CreateViewModel();
        LoadAFile();
    }

    [TestMethod]
    public void PersonUpdated()
    {
        var viewModel = CreateViewModel();
        Assert.AreEqual(0, viewModel.Persons.Count);

        persons.Add(new() { Id = 1, Firstname = "Alice", Lastname = "Smith" });
        Events.Send<PersonsUpdated>();

        Assert.AreEqual(1, viewModel.Persons.Count);
    }

    [TestMethod]
    public void LocationsUpdated()
    {
        var viewModel = CreateViewModel();
        Assert.AreEqual(0, viewModel.Locations.Count);

        locations.Add(new() { Id = 1, Name = "Home" });
        Events.Send<LocationsUpdated>();

        Assert.AreEqual(1, viewModel.Locations.Count);
    }

    [TestMethod]
    public void TagsUpdated()
    {
        var viewModel = CreateViewModel();
        Assert.AreEqual(0, viewModel.Tags.Count);

        tags.Add(new() { Id = 1, Name = "Favorites" });
        Events.Send<TagsUpdated>();

        Assert.AreEqual(1, viewModel.Tags.Count);
    }

    [TestMethod]
    public void SetFileDescription()
    {
        var viewModel = CreateViewModel();

        var editedFileId = 1;
        LoadAFile(editedFileId);

        var newDescription = "New desciption";
        viewModel.NewFileDescription = newDescription;
        viewModel.SetFileDescriptionCommand.Execute(null);

        A.CallTo(() => dbAccessRepository.DbAccess.UpdateFileDescription(editedFileId, newDescription)).MustHaveHappened();
        eventRecorder.AssertEventRecorded<FileEdited>();
        Assert.AreEqual(editedFileId, viewModel.PrevEditedFileId);
        Assert.AreEqual(0, viewModel.UpdateHistoryItems.Count);
    }

    private FileCategorizationViewModel CreateViewModel()
    {
        return new FileCategorizationViewModel(configRepository, dbAccessRepository, dialogs, filesystemAccessRepository, personsRepository, locationsRepository, tagsRepository);
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

    private static void LoadAFile(int fileId = 1)
    {
        Events.Send(new SelectSearchResultFile(new FileModel() { Id = fileId, Path = "file.jpg", }));
    }
}