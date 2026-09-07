using FakeItEasy;
using FileDB.Infrastructure;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.Model;

public class DatabaseCacheTests : IDisposable
{
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IDatabaseAccess dbAccess = A.Fake<IDatabaseAccess>();
    private DatabaseCache? cache;

    public DatabaseCacheTests()
    {
        A.CallTo(() => dbAccessProvider.DbAccess).Returns(dbAccess);
    }

    [Fact]
    public void Constructor_LoadsAndSortsAllRepositories()
    {
        A.CallTo(() => dbAccess.GetPersons()).Returns([
            new PersonModel { Id = 1, FullName = "Zoe", ShortName = "Zoe" },
            new PersonModel { Id = 2, FullName = "Anna", ShortName = "Anna" },
        ]);
        A.CallTo(() => dbAccess.GetLocations()).Returns([
            new LocationModel { Id = 1, Name = "Zoo" },
            new LocationModel { Id = 2, Name = "Home" },
        ]);
        A.CallTo(() => dbAccess.GetTags()).Returns([
            new TagModel { Id = 1, Name = "Video" },
            new TagModel { Id = 2, Name = "Family" },
        ]);

        cache = new DatabaseCache(dbAccessProvider);

        Assert.Equal(["Anna", "Zoe"], cache.Persons.Select(x => x.FullName));
        Assert.Equal(["Home", "Zoo"], cache.Locations.Select(x => x.Name));
        Assert.Equal(["Family", "Video"], cache.Tags.Select(x => x.Name));
        A.CallTo(() => dbAccess.GetPersons()).MustHaveHappenedOnceExactly();
        A.CallTo(() => dbAccess.GetLocations()).MustHaveHappenedOnceExactly();
        A.CallTo(() => dbAccess.GetTags()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void EditedMessages_ReloadOnlyTheirRepository()
    {
        var cache = CreateCache();
        A.CallTo(() => dbAccess.GetPersons()).Returns([
            new PersonModel { Id = 3, FullName = "Updated", ShortName = "Updated" },
        ]);

        Messenger.Send<PersonEdited>();

        Assert.Equal(["Updated"], cache.Persons.Select(x => x.FullName));
        A.CallTo(() => dbAccess.GetPersons()).MustHaveHappenedTwiceExactly();
        A.CallTo(() => dbAccess.GetLocations()).MustHaveHappenedOnceExactly();
        A.CallTo(() => dbAccess.GetTags()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void ConfigUpdated_ReloadsAllRepositories()
    {
        var cache = CreateCache();
        A.CallTo(() => dbAccess.GetLocations()).Returns([
            new LocationModel { Id = 3, Name = "Updated location" },
        ]);
        A.CallTo(() => dbAccess.GetTags()).Returns([
            new TagModel { Id = 3, Name = "Updated tag" },
        ]);

        Messenger.Send<ConfigUpdated>();

        Assert.Equal(["Updated location"], cache.Locations.Select(x => x.Name));
        Assert.Equal(["Updated tag"], cache.Tags.Select(x => x.Name));
        A.CallTo(() => dbAccess.GetPersons()).MustHaveHappenedTwiceExactly();
        A.CallTo(() => dbAccess.GetLocations()).MustHaveHappenedTwiceExactly();
        A.CallTo(() => dbAccess.GetTags()).MustHaveHappenedTwiceExactly();
    }

    public void Dispose()
    {
        if (cache is not null)
        {
            Messenger.Unregister(cache);
        }
    }

    private DatabaseCache CreateCache()
    {
        A.CallTo(() => dbAccess.GetPersons()).Returns([]);
        A.CallTo(() => dbAccess.GetLocations()).Returns([]);
        A.CallTo(() => dbAccess.GetTags()).Returns([]);
        cache = new DatabaseCache(dbAccessProvider);
        return cache;
    }
}
