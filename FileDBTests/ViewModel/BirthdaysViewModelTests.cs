using CommunityToolkit.Mvvm.Messaging;
using FakeItEasy;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Notifiers;
using FileDB.ViewModel;
using FileDBInterface.DbAccess;
using FileDBInterface.FilesystemAccess;
using FileDBShared.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace FileDBTests.ViewModel;

[TestClass]
public class BirthdaysViewModelTests
{
    private IDbAccess fakeDbAccess;
    private IFilesystemAccess fakeFilsystemAccess;
    private INotifierFactory fakeNotifierFactory;

    private Model model;

    [TestInitialize]
    public void Initialize()
    {
        model = Model.Instance;

        fakeDbAccess = A.Fake<IDbAccess>();
        fakeFilsystemAccess = A.Fake<IFilesystemAccess>();
        fakeNotifierFactory = A.Fake<INotifierFactory>();

        model.InitConfig(DefaultConfigs.Default, fakeDbAccess, fakeFilsystemAccess, fakeNotifierFactory);
    }

    [TestMethod]
    public void Constructor_NoPersons()
    {
        var viewModel = new BirthdaysViewModel();

        Assert.AreEqual(0, viewModel.Persons.Count);
        Assert.AreEqual("", viewModel.FilterText);
    }

    [TestMethod]
    public void Constructor_SomePersons()
    {
        A.CallTo(() => fakeDbAccess.GetPersons()).Returns(SomePersons());

        var viewModel = new BirthdaysViewModel();

        Assert.AreEqual(2, viewModel.Persons.Count);
    }

    [TestMethod]
    public void PersonsUpdated()
    {
        var viewModel = new BirthdaysViewModel();
        Assert.AreEqual(0, viewModel.Persons.Count);

        A.CallTo(() => fakeDbAccess.GetPersons()).Returns(SomePersons());
        WeakReferenceMessenger.Default.Send(new PersonsUpdated());

        Assert.AreEqual(2, viewModel.Persons.Count);
    }

    [TestMethod]
    public void DateChanged()
    {
        var viewModel = new BirthdaysViewModel();
        Assert.AreEqual(0, viewModel.Persons.Count);

        A.CallTo(() => fakeDbAccess.GetPersons()).Returns(SomePersons());
        WeakReferenceMessenger.Default.Send(new DateChanged());

        Assert.AreEqual(2, viewModel.Persons.Count);
    }

    [TestMethod]
    public void FilterText()
    {
        A.CallTo(() => fakeDbAccess.GetPersons()).Returns(SomePersons());

        var viewModel = new BirthdaysViewModel();
        
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
        return new()
        {
            new () { Id = 1, Firstname = "Alice", Lastname = "Andersson", DateOfBirth = "2000-01-01" },
            new () { Id = 2, Firstname = "Bob", Lastname = "Andersson", DateOfBirth = "2000-02-02" },
        };
    }
}
