using FileDB.Sorters;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.Model;

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
    private readonly IDbAccessRepository dbAccessRepository;

    public IEnumerable<PersonModel> Persons => persons;
    public IEnumerable<LocationModel> Locations => locations;
    public IEnumerable<TagModel> Tags => tags;

    private readonly List<PersonModel> persons = new();
    private readonly List<LocationModel> locations = new();
    private readonly List<TagModel> tags = new();

    public DatabaseCache(IDbAccessRepository dbAccessRepository)
    {
        this.dbAccessRepository = dbAccessRepository;

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
        persons.AddRange(dbAccessRepository.DbAccess.GetPersons());
        persons.Sort(new PersonModelByNameSorter());
        Events.Send<PersonsUpdated>();
    }

    private void ReloadLocations()
    {
        locations.Clear();
        locations.AddRange(dbAccessRepository.DbAccess.GetLocations());
        locations.Sort(new LocationModelByNameSorter());
        Events.Send<LocationsUpdated>();
    }

    private void ReloadTags()
    {
        tags.Clear();
        tags.AddRange(dbAccessRepository.DbAccess.GetTags());
        tags.Sort(new TagModelByNameSorter());
        Events.Send<TagsUpdated>();
    }
}
