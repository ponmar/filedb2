using System;
using System.Linq;
using FakeItEasy;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDBInterfaceTests.DatabaseAccess;

public class ReadOnlyDatabaseAccessTests
{
    private readonly IDatabaseAccess _inner = A.Fake<IDatabaseAccess>();
    private readonly ReadOnlyDatabaseAccess _sut;

    public ReadOnlyDatabaseAccessTests()
    {
        _sut = new ReadOnlyDatabaseAccess(_inner);
    }

    // ── Inner property ─────────────────────────────────────────────────────

    [Fact]
    public void Inner_ReturnsWrappedInstance()
    {
        Assert.Same(_inner, _sut.Inner);
    }

    // ── Read methods delegate to inner ────────────────────────────────────

    [Fact]
    public void GetFiles_DelegatesToInner()
    {
        var files = new[] { new FileModel { Id = 1, Path = "a.jpg" } };
        A.CallTo(() => _inner.GetFiles()).Returns(files);

        var result = _sut.GetFiles().ToList();

        A.CallTo(() => _inner.GetFiles()).MustHaveHappenedOnceExactly();
        Assert.Single(result);
    }

    [Fact]
    public void GetFileCount_DelegatesToInner()
    {
        A.CallTo(() => _inner.GetFileCount()).Returns(42);

        Assert.Equal(42, _sut.GetFileCount());
        A.CallTo(() => _inner.GetFileCount()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void GetFileById_DelegatesToInner()
    {
        var file = new FileModel { Id = 7, Path = "x.jpg" };
        A.CallTo(() => _inner.GetFileById(7)).Returns(file);

        var result = _sut.GetFileById(7);

        Assert.Same(file, result);
    }

    [Fact]
    public void GetFileByPath_DelegatesToInner()
    {
        var file = new FileModel { Id = 1, Path = "img.jpg" };
        A.CallTo(() => _inner.GetFileByPath("img.jpg")).Returns(file);

        var result = _sut.GetFileByPath("img.jpg");

        Assert.Same(file, result);
    }

    [Fact]
    public void GetDirectories_DelegatesToInner()
    {
        A.CallTo(() => _inner.GetDirectories()).Returns(["dir1", "dir2"]);

        var result = _sut.GetDirectories().ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void SearchFilesFromIds_DelegatesToInner()
    {
        var ids = new[] { 1, 2 };
        var files = new[] { new FileModel { Id = 1, Path = "a.jpg" } };
        A.CallTo(() => _inner.SearchFilesFromIds(ids)).Returns(files);

        _sut.SearchFilesFromIds(ids);

        A.CallTo(() => _inner.SearchFilesFromIds(ids)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void GetPersons_DelegatesToInner()
    {
        var persons = new[] { new PersonModel { Id = 1, ShortName = "JD", FullName = "John Doe", Sex = Sex.Male } };
        A.CallTo(() => _inner.GetPersons()).Returns(persons);

        var result = _sut.GetPersons().ToList();

        Assert.Single(result);
        A.CallTo(() => _inner.GetPersons()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void GetPersonCount_DelegatesToInner()
    {
        A.CallTo(() => _inner.GetPersonCount()).Returns(5);

        Assert.Equal(5, _sut.GetPersonCount());
    }

    [Fact]
    public void GetPersonById_DelegatesToInner()
    {
        var person = new PersonModel { Id = 3, ShortName = "AB", FullName = "Alice Bob", Sex = Sex.Female };
        A.CallTo(() => _inner.GetPersonById(3)).Returns(person);

        Assert.Same(person, _sut.GetPersonById(3));
    }

    [Fact]
    public void HasPersonId_DelegatesToInner()
    {
        A.CallTo(() => _inner.HasPersonId(10)).Returns(true);

        Assert.True(_sut.HasPersonId(10));
    }

    [Fact]
    public void GetPersonsFromFile_DelegatesToInner()
    {
        A.CallTo(() => _inner.GetPersonsFromFile(1)).Returns([]);

        _sut.GetPersonsFromFile(1);

        A.CallTo(() => _inner.GetPersonsFromFile(1)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void GetLocations_DelegatesToInner()
    {
        A.CallTo(() => _inner.GetLocations()).Returns([]);

        _sut.GetLocations();

        A.CallTo(() => _inner.GetLocations()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void GetLocationCount_DelegatesToInner()
    {
        A.CallTo(() => _inner.GetLocationCount()).Returns(3);

        Assert.Equal(3, _sut.GetLocationCount());
    }

    [Fact]
    public void GetLocationById_DelegatesToInner()
    {
        var loc = new LocationModel { Id = 2, Name = "Home" };
        A.CallTo(() => _inner.GetLocationById(2)).Returns(loc);

        Assert.Same(loc, _sut.GetLocationById(2));
    }

    [Fact]
    public void HasLocationId_DelegatesToInner()
    {
        A.CallTo(() => _inner.HasLocationId(5)).Returns(false);

        Assert.False(_sut.HasLocationId(5));
    }

    [Fact]
    public void GetLocationsFromFile_DelegatesToInner()
    {
        A.CallTo(() => _inner.GetLocationsFromFile(1)).Returns([]);

        _sut.GetLocationsFromFile(1);

        A.CallTo(() => _inner.GetLocationsFromFile(1)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void GetTags_DelegatesToInner()
    {
        A.CallTo(() => _inner.GetTags()).Returns([]);

        _sut.GetTags();

        A.CallTo(() => _inner.GetTags()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void GetTagCount_DelegatesToInner()
    {
        A.CallTo(() => _inner.GetTagCount()).Returns(7);

        Assert.Equal(7, _sut.GetTagCount());
    }

    [Fact]
    public void GetTagById_DelegatesToInner()
    {
        var tag = new TagModel { Id = 1, Name = "Nature" };
        A.CallTo(() => _inner.GetTagById(1)).Returns(tag);

        Assert.Same(tag, _sut.GetTagById(1));
    }

    [Fact]
    public void HasTagId_DelegatesToInner()
    {
        A.CallTo(() => _inner.HasTagId(4)).Returns(true);

        Assert.True(_sut.HasTagId(4));
    }

    [Fact]
    public void GetTagsFromFile_DelegatesToInner()
    {
        A.CallTo(() => _inner.GetTagsFromFile(1)).Returns([]);

        _sut.GetTagsFromFile(1);

        A.CallTo(() => _inner.GetTagsFromFile(1)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void NeedsMigration_DelegatesToInner()
    {
        A.CallTo(() => _inner.NeedsMigration).Returns(false);

        Assert.False(_sut.NeedsMigration);
    }

    [Fact]
    public void GetFilePersonBoundingBox_DelegatesToInner()
    {
        var bbox = new PersonBoundingBox(0.1, 0.2, 0.3, 0.4);
        A.CallTo(() => _inner.GetFilePersonBoundingBox(1, 2)).Returns(bbox);

        var result = _sut.GetFilePersonBoundingBox(1, 2);

        Assert.Equal(bbox, result);
    }

    [Fact]
    public void GetFilePersonBoundingBoxes_DelegatesToInner()
    {
        A.CallTo(() => _inner.GetFilePersonBoundingBoxes(1)).Returns([]);

        _sut.GetFilePersonBoundingBoxes(1);

        A.CallTo(() => _inner.GetFilePersonBoundingBoxes(1)).MustHaveHappenedOnceExactly();
    }

    // ── Write methods throw ────────────────────────────────────────────────

    [Fact]
    public void Migrate_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.Migrate());
    }

    [Fact]
    public void InsertFile_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _sut.InsertFile("path.jpg", null, A.Fake<IFilesystemAccess>(), false));
    }

    [Fact]
    public void DeleteFile_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.DeleteFile(1));
    }

    [Fact]
    public void UpdateFileFromMetaData_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _sut.UpdateFileFromMetaData(1, A.Fake<IFilesystemAccess>()));
    }

    [Fact]
    public void UpdateFileDescription_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.UpdateFileDescription(1, "desc"));
    }

    [Fact]
    public void UpdateFileDatetime_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.UpdateFileDatetime(1, "2024-01-01"));
    }

    [Fact]
    public void UpdateFileOrientation_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.UpdateFileOrientation(1, 1));
    }

    [Fact]
    public void InsertFilePerson_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.InsertFilePerson(1, 2));
    }

    [Fact]
    public void DeleteFilePerson_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.DeleteFilePerson(1, 2));
    }

    [Fact]
    public void UpdateFilePersonBoundingBox_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _sut.UpdateFilePersonBoundingBox(1, 2, null));
    }

    [Fact]
    public void InsertFileLocation_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.InsertFileLocation(1, 2));
    }

    [Fact]
    public void DeleteFileLocation_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.DeleteFileLocation(1, 2));
    }

    [Fact]
    public void InsertFileTag_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.InsertFileTag(1, 2));
    }

    [Fact]
    public void DeleteFileTag_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.DeleteFileTag(1, 2));
    }

    [Fact]
    public void InsertPerson_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _sut.InsertPerson(new PersonModel { Id = 0, ShortName = "X", FullName = "Y", Sex = Sex.NotKnown }));
    }

    [Fact]
    public void UpdatePerson_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _sut.UpdatePerson(new PersonModel { Id = 0, ShortName = "X", FullName = "Y", Sex = Sex.NotKnown }));
    }

    [Fact]
    public void DeletePerson_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.DeletePerson(1));
    }

    [Fact]
    public void InsertLocation_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _sut.InsertLocation(new LocationModel { Id = 0, Name = "Home" }));
    }

    [Fact]
    public void UpdateLocation_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _sut.UpdateLocation(new LocationModel { Id = 0, Name = "Home" }));
    }

    [Fact]
    public void DeleteLocation_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.DeleteLocation(1));
    }

    [Fact]
    public void InsertTag_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _sut.InsertTag(new TagModel { Id = 0, Name = "Tag" }));
    }

    [Fact]
    public void UpdateTag_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _sut.UpdateTag(new TagModel { Id = 0, Name = "Tag" }));
    }

    [Fact]
    public void DeleteTag_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.DeleteTag(1));
    }
}
