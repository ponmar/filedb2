using FakeItEasy;
using FileDB.Model;
using FileDB.Services;
using FileDB.ViewModels;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.ViewModels;

public class RipViewModelTests
{
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IDatabaseAccess dbAccess = A.Fake<IDatabaseAccess>();
    private readonly IFilesystemAccessProvider filesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
    private readonly IFilesystemAccess filesystemAccess = A.Fake<IFilesystemAccess>();
    private readonly IImageLoader imageLoader = A.Fake<IImageLoader>();
    private readonly ICriteriaViewModel criteriaViewModel = A.Fake<ICriteriaViewModel>();

    public RipViewModelTests()
    {
        A.CallTo(() => dbAccessProvider.DbAccess).Returns(dbAccess);
        A.CallTo(() => filesystemAccessProvider.FilesystemAccess).Returns(filesystemAccess);
    }

    [Fact]
    public void DeceasedPersonViewModel_CommandCallsCriteria()
    {
        var person = new PersonModel
        {
            Id = 1,
            ShortName = "Alice",
            FullName = "Alice Smith",
            DateOfBirth = "2000-01-01",
            Deceased = "2020-01-01",
        };

        var viewModel = new DeceasedPersonViewModel(criteriaViewModel, person, "/photo.jpg");

        Assert.Equal("/photo.jpg", viewModel.ProfilePictureAbsPath);
        Assert.Equal(20, viewModel.Age);

        viewModel.SearchForPersonCommand.Execute(null);

        A.CallTo(() => criteriaViewModel.SearchForPersonAsync(person)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Constructor_LoadsOnlyDeceasedPersons()
    {
        A.CallTo(() => dbAccess.GetPersons()).Returns([
            new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith", DateOfBirth = "2000-01-01", Deceased = "2020-01-01" },
            new PersonModel { Id = 2, ShortName = "Bob", FullName = "Bob Smith", DateOfBirth = "2000-01-01" },
        ]);

        var viewModel = new RipViewModel(dbAccessProvider, filesystemAccessProvider, imageLoader, criteriaViewModel);

        Assert.Single(viewModel.Persons);
        Assert.Equal("Alice Smith", viewModel.Persons[0].Name);
    }

    [Fact]
    public void ClearFilterTextCommand_ResetsFilter()
    {
        A.CallTo(() => dbAccess.GetPersons()).Returns([
            new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith", DateOfBirth = "2000-01-01", Deceased = "2020-01-01" },
        ]);

        var viewModel = new RipViewModel(dbAccessProvider, filesystemAccessProvider, imageLoader, criteriaViewModel)
        {
            FilterText = "Alice",
        };

        viewModel.ClearFilterTextCommand.Execute(null);

        Assert.Equal(string.Empty, viewModel.FilterText);
    }
}
