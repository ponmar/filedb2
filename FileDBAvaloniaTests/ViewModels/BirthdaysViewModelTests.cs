using FakeItEasy;
using FileDBAvalonia;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;
using FileDBShared.Model;
using Xunit;

namespace FileDBAvaloniaTests.ViewModels;

[Collection("Sequential")]
public class BirthdaysViewModelTests
{
    private IConfigProvider fakeConfigRepo;
    private IDatabaseAccess fakeDbAccess;
    private IDatabaseAccessProvider fakeDbAccessRepo;
    private IPersonsRepository fakePersonsRepo;
    private IFilesystemAccess fakeFilsystemAccess;
    private IFilesystemAccessProvider fakeFilsystemAccessRepo;
    private IImageLoader fakeImageLoader;

    public BirthdaysViewModelTests()
    {
        Bootstrapper.Reset();

        fakeConfigRepo = A.Fake<IConfigProvider>();
        fakeDbAccess = A.Fake<IDatabaseAccess>();
        fakeDbAccessRepo = A.Fake<IDatabaseAccessProvider>();
        fakeFilsystemAccess = A.Fake<IFilesystemAccess>();
        fakeFilsystemAccessRepo = A.Fake<IFilesystemAccessProvider>();
        fakePersonsRepo = A.Fake<IPersonsRepository>();
        fakeImageLoader = A.Fake<IImageLoader>();

        A.CallTo(() => fakeDbAccessRepo.DbAccess).Returns(fakeDbAccess);
        A.CallTo(() => fakeFilsystemAccessRepo.FilesystemAccess).Returns(fakeFilsystemAccess);
    }

    [Fact]
    public void Constructor_NoPersons()
    {
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessRepo, fakeDbAccessRepo, fakeImageLoader);

        Assert.Empty(viewModel.Persons);
        Assert.Equal("", viewModel.FilterText);
    }

    [Fact]
    public void Constructor_SomePersons()
    {
        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());

        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessRepo, fakeDbAccessRepo, fakeImageLoader);

        Assert.Equal(2, viewModel.Persons.Count);
    }

    [Fact]
    public void PersonsUpdated()
    {
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessRepo, fakeDbAccessRepo, fakeImageLoader);
        Assert.Empty(viewModel.Persons);

        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());
        Messenger.Send<PersonsUpdated>();

        Assert.Equal(2, viewModel.Persons.Count);
    }

    [Fact]
    public void DateChanged()
    {
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessRepo, fakeDbAccessRepo, fakeImageLoader);
        Assert.Empty(viewModel.Persons);

        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());
        Messenger.Send<DateChanged>();

        Assert.Equal(2, viewModel.Persons.Count);
    }

    [Fact]
    public void FilterText()
    {
        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessRepo, fakeDbAccessRepo, fakeImageLoader);

        viewModel.FilterText = "Al";
        Assert.Single(viewModel.Persons);
        Assert.Equal("Alice Andersson", viewModel.Persons.First().Name);

        viewModel.FilterText = "Bo";
        Assert.Single(viewModel.Persons);
        Assert.Equal("Bob Andersson", viewModel.Persons.First().Name);

        viewModel.FilterText = "";
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
