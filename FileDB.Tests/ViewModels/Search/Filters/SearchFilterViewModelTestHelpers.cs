using FakeItEasy;
using FileDB.Model;
using FileDB.ViewModels.Search;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.Tests.ViewModels.Search.Filters;

internal static class SearchFilterViewModelTestHelpers
{
    internal static FileModel CreateFile(int id, string path, string? datetime = null, string? position = null)
    {
        return new FileModel
        {
            Id = id,
            Path = path,
            Datetime = datetime,
            Position = position,
        };
    }

    internal static PersonModel CreatePerson(int id, string fullName, string? birthDate = null, string? deceasedDate = null, int? profileFileId = null, Sex sex = Sex.NotKnown, string? description = null)
    {
        return new PersonModel
        {
            Id = id,
            ShortName = fullName,
            FullName = fullName,
            Description = description,
            DateOfBirth = birthDate,
            Deceased = deceasedDate,
            ProfileFileId = profileFileId,
            Sex = sex,
        };
    }

    internal static LocationModel CreateLocation(int id, string name, string? position = null, string? description = null)
    {
        return new LocationModel
        {
            Id = id,
            Name = name,
            Position = position,
            Description = description,
        };
    }

    internal static TagModel CreateTag(int id, string name)
    {
        return new TagModel
        {
            Id = id,
            Name = name,
        };
    }

    internal static IFileSelector CreateFileSelector(FileModel? selectedFile = null)
    {
        var fileSelector = A.Fake<IFileSelector>();
        A.CallTo(() => fileSelector.SelectedFile).Returns(selectedFile);
        return fileSelector;
    }

    internal static IDatabaseAccessProvider CreateDatabaseAccessProvider(IDatabaseAccess dbAccess)
    {
        var provider = A.Fake<IDatabaseAccessProvider>();
        A.CallTo(() => provider.DbAccess).Returns(dbAccess);
        return provider;
    }

    internal static ISearchResultRepository CreateSearchResultRepository(IReadOnlyCollection<FileModel> files)
    {
        var repo = A.Fake<ISearchResultRepository>();
        A.CallTo(() => repo.Files).Returns(files);
        return repo;
    }

    internal static IPersonsRepository CreatePersonsRepository(IReadOnlyCollection<PersonModel> persons)
    {
        var repo = A.Fake<IPersonsRepository>();
        A.CallTo(() => repo.Persons).Returns(persons);
        return repo;
    }

    internal static ILocationsRepository CreateLocationsRepository(IReadOnlyCollection<LocationModel> locations)
    {
        var repo = A.Fake<ILocationsRepository>();
        A.CallTo(() => repo.Locations).Returns(locations);
        return repo;
    }

    internal static ITagsRepository CreateTagsRepository(IReadOnlyCollection<TagModel> tags)
    {
        var repo = A.Fake<ITagsRepository>();
        A.CallTo(() => repo.Tags).Returns(tags);
        return repo;
    }
}
