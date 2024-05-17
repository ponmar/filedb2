using FileDBAvalonia.Sorters;
using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.Model;

public interface IPersonsRepository
{
    IEnumerable<PersonModel> Persons { get; }
}

public interface ILocationsRepository
{
    IEnumerable<LocationModel> Locations { get; }
}

public interface ITagsRepository
{
    IEnumerable<TagModel> Tags { get; }
}

public class DatabaseCache : IPersonsRepository, ILocationsRepository, ITagsRepository
{
    private readonly IDatabaseAccessProvider dbAccessProvider;

    public IEnumerable<PersonModel> Persons => persons;
    public IEnumerable<LocationModel> Locations => locations;
    public IEnumerable<TagModel> Tags => tags;

    private readonly List<PersonModel> persons = [];
    private readonly List<LocationModel> locations = [];
    private readonly List<TagModel> tags = [];

    public DatabaseCache(IDatabaseAccessProvider dbAccessProvider)
    {
        this.dbAccessProvider = dbAccessProvider;

        ReloadPersons();
        ReloadLocations();
        ReloadTags();

        this.RegisterForEvent<PersonEdited>((x) => ReloadPersons());
        this.RegisterForEvent<LocationEdited>((x) => ReloadLocations());
        this.RegisterForEvent<TagEdited>((x) => ReloadTags());

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            ReloadPersons();
            ReloadLocations();
            ReloadTags();
        });
    }

    private void ReloadPersons()
    {
        persons.Clear();
        persons.AddRange(dbAccessProvider.DbAccess.GetPersons());
        persons.Sort(new PersonModelByNameSorter());
        Messenger.Send<PersonsUpdated>();
    }

    private void ReloadLocations()
    {
        locations.Clear();
        locations.AddRange(dbAccessProvider.DbAccess.GetLocations());
        locations.Sort(new LocationModelByNameSorter());
        Messenger.Send<LocationsUpdated>();
    }

    private void ReloadTags()
    {
        tags.Clear();
        tags.AddRange(dbAccessProvider.DbAccess.GetTags());
        tags.Sort(new TagModelByNameSorter());
        Messenger.Send<TagsUpdated>();
    }
}
