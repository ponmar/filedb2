using FakeItEasy;
using FileDBAvalonia;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;
using FileDBShared.Model;

namespace FileDBAvaloniaTests.ViewModels;

[TestClass]
public class BirthdaysViewModelTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private IConfigProvider fakeConfigRepo;
    private IDatabaseAccess fakeDbAccess;
    private IDatabaseAccessProvider fakeDbAccessRepo;
    private IPersonsRepository fakePersonsRepo;
    private IFilesystemAccess fakeFilsystemAccess;
    private IFilesystemAccessProvider fakeFilsystemAccessRepo;
    private IImageLoader fakeImageLoader;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [TestInitialize]
    public void Initialize()
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

    [TestMethod]
    public void Constructor_NoPersons()
    {
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessRepo, fakeDbAccessRepo, fakeImageLoader);

        Assert.AreEqual(0, viewModel.Persons.Count);
        Assert.AreEqual("", viewModel.FilterText);
    }

    [TestMethod]
    public void Constructor_SomePersons()
    {
        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());

        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessRepo, fakeDbAccessRepo, fakeImageLoader);

        Assert.AreEqual(2, viewModel.Persons.Count);
    }

    [TestMethod]
    public void PersonsUpdated()
    {
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessRepo, fakeDbAccessRepo, fakeImageLoader);
        Assert.AreEqual(0, viewModel.Persons.Count);

        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());
        Messenger.Send<PersonsUpdated>();

        Assert.AreEqual(2, viewModel.Persons.Count);
    }

    [TestMethod]
    public void DateChanged()
    {
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessRepo, fakeDbAccessRepo, fakeImageLoader);
        Assert.AreEqual(0, viewModel.Persons.Count);

        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());
        Messenger.Send<DateChanged>();

        Assert.AreEqual(2, viewModel.Persons.Count);
    }

    [TestMethod]
    public void FilterText()
    {
        A.CallTo(() => fakePersonsRepo.Persons).Returns(SomePersons());
        var config = new ConfigBuilder().Build();
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);

        var viewModel = new BirthdaysViewModel(fakePersonsRepo, fakeFilsystemAccessRepo, fakeDbAccessRepo, fakeImageLoader);

        viewModel.FilterText = "Al";
        Assert.AreEqual(1, viewModel.Persons.Count);
        Assert.AreEqual("Alice Andersson", viewModel.Persons.First().Name);

        viewModel.FilterText = "Bo";
        Assert.AreEqual(1, viewModel.Persons.Count);
        Assert.AreEqual("Bob Andersson", viewModel.Persons.First().Name);

        viewModel.FilterText = "";
        Assert.AreEqual(2, viewModel.Persons.Count);
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
