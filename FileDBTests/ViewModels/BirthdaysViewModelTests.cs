using FakeItEasy;
using FileDB;
using FileDB.Model;
using FileDB.ViewModels;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.ViewModels;

public class BirthdaysViewModelTests
{
    private readonly IConfigProvider fakeConfigRepo = A.Fake<IConfigProvider>();
    private readonly IDatabaseAccess fakeDbAccess = A.Fake<IDatabaseAccess>();
    private readonly IDatabaseAccessProvider fakeDbAccessRepo = A.Fake<IDatabaseAccessProvider>();
    private readonly IPersonsRepository fakePersonsRepo = A.Fake<IPersonsRepository>();
    private readonly IFilesystemAccess fakeFilsystemAccess = A.Fake<IFilesystemAccess>();
    private readonly IFilesystemAccessProvider fakeFilsystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
    private readonly IImageLoader fakeImageLoader = A.Fake<IImageLoader>();
    private readonly ICriteriaViewModel fakeCriteriaViewModel = A.Fake<ICriteriaViewModel>();

    public BirthdaysViewModelTests()
    {
        A.CallTo(() => fakeDbAccessRepo.DbAccess).Returns(fakeDbAccess);
        A.CallTo(() => fakeFilsystemAccessProvider.FilesystemAccess).Returns(fakeFilsystemAccess);
    }

    [Fact]
    public void Constructor_NoPersons()
    {
        // Arrange
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        // Act
        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessProvider, fakeDbAccessRepo, fakeImageLoader, fakeCriteriaViewModel);

        // Assert
        Assert.Empty(viewModel.Persons);
        Assert.Equal("", viewModel.FilterText);
    }

    [Fact]
    public void Constructor_SomePersons()
    {
        // Arrange
        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        // Act
        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessProvider, fakeDbAccessRepo, fakeImageLoader, fakeCriteriaViewModel);

        // Assert
        Assert.Equal(2, viewModel.Persons.Count);
    }

    [Fact]
    public void ConfigUpdated()
    {
        // Arrange
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessProvider, fakeDbAccessRepo, fakeImageLoader, fakeCriteriaViewModel);
        Assert.Empty(viewModel.Persons);

        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());

        // Act
        Messenger.Send<ConfigUpdated>();

        // Assert
        Assert.Equal(2, viewModel.Persons.Count);
    }

    [Fact]
    public void PersonsUpdated()
    {
        // Arrange
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessProvider, fakeDbAccessRepo, fakeImageLoader, fakeCriteriaViewModel);
        Assert.Empty(viewModel.Persons);

        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());

        // Act
        Messenger.Send<PersonsUpdated>();

        // Assert
        Assert.Equal(2, viewModel.Persons.Count);
    }

    [Fact]
    public void DateChanged()
    {
        // Arrange
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessProvider, fakeDbAccessRepo, fakeImageLoader, fakeCriteriaViewModel);
        Assert.Empty(viewModel.Persons);

        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());

        // Act
        Messenger.Send<DateChanged>();

        // Assert
        Assert.Equal(2, viewModel.Persons.Count);
    }

    [Fact]
    public void FilterText()
    {
        // Arrange
        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessProvider, fakeDbAccessRepo, fakeImageLoader, fakeCriteriaViewModel);

        // Act
        viewModel.FilterText = "Al";

        // Assert
        Assert.Single(viewModel.Persons);
        Assert.Equal("Alice Andersson", viewModel.Persons.First().Name);

        // Act
        viewModel.FilterText = "Bo";

        // Assert
        Assert.Single(viewModel.Persons);
        Assert.Equal("Bob Andersson", viewModel.Persons.First().Name);

        // Act
        viewModel.FilterText = "";

        // Assert
        Assert.Equal(2, viewModel.Persons.Count);
    }

    [Fact]
    public void ClearFilterTextCommand()
    {
        // Arrange
        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessProvider, fakeDbAccessRepo, fakeImageLoader, fakeCriteriaViewModel);

        viewModel.FilterText = "Al";
        Assert.Single(viewModel.Persons);
        Assert.Equal("Alice Andersson", viewModel.Persons.First().Name);

        // Act
        viewModel.ClearFilterTextCommand.Execute(null);
        
        Assert.Equal("", viewModel.FilterText);
        Assert.Equal(2, viewModel.Persons.Count);
    }

    private static List<PersonModel> SomePersons()
    {
        return
        [
            new () { Id = 1, Firstname = "Alice", Lastname = "Andersson", DateOfBirth = "2000-01-01" },
            new () { Id = 2, Firstname = "Bob", Lastname = "Andersson", DateOfBirth = "2000-02-02" },
        ];
    }
}
