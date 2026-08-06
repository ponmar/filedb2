using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Model;
using FileDB.ViewModels;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.ViewModels;

public class UpdatePersonsViewModelTests
{
    private readonly IConfigProvider configProvider = A.Fake<IConfigProvider>();
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IDatabaseAccess dbAccess = A.Fake<IDatabaseAccess>();
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly IPersonsRepository personsRepository = A.Fake<IPersonsRepository>();

    public UpdatePersonsViewModelTests()
    {
        A.CallTo(() => dbAccessProvider.DbAccess).Returns(dbAccess);
        A.CallTo(() => configProvider.Config).Returns(new ConfigBuilder { ReadOnly = false }.Build());
    }

    [Fact]
    public void Constructor_LoadsPersons()
    {
        A.CallTo(() => personsRepository.Persons).Returns([
            new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith", DateOfBirth = "2000-01-01" },
            new PersonModel { Id = 2, ShortName = "Bob", FullName = "Bob Smith", DateOfBirth = "2000-01-01" },
        ]);

        var viewModel = new UpdatePersonsViewModel(configProvider, dbAccessProvider, dialogs, personsRepository);

        Assert.Equal(2, viewModel.Persons.Count);
        Assert.False(viewModel.ReadOnly);
    }

    [Fact]
    public void AddPersonCommand_UsesFilterText()
    {
        A.CallTo(() => personsRepository.Persons).Returns([]);

        var viewModel = new UpdatePersonsViewModel(configProvider, dbAccessProvider, dialogs, personsRepository)
        {
            FilterText = "Alice",
        };

        viewModel.AddPersonCommand.Execute(null);

        A.CallTo(() => dialogs.ShowAddPersonDialogAsync(null, "Alice")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemovePersonCommand_DeletesSelectedPerson()
    {
        A.CallTo(() => personsRepository.Persons).Returns([
            new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith", DateOfBirth = "2000-01-01" },
        ]);
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(true);
        A.CallTo(() => dbAccess.SearchFilesWithPersons(A<IEnumerable<int>>._)).Returns([]);
        var recorder = new SingleEventRecorder<PersonEdited>();

        var viewModel = new UpdatePersonsViewModel(configProvider, dbAccessProvider, dialogs, personsRepository)
        {
            SelectedPerson = new PersonWithAge { Id = 1, ShortName = "Alice", FullName = "Alice Smith" },
        };

        await viewModel.RemovePersonCommand.ExecuteAsync(null);

        A.CallTo(() => dbAccess.DeletePerson(1)).MustHaveHappenedOnceExactly();
        recorder.AssertEventRecorded();
    }

    [Fact]
    public void EditPersonCommand_UsesSelectedPerson()
    {
        A.CallTo(() => personsRepository.Persons).Returns([
            new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith", DateOfBirth = "2000-01-01" },
        ]);

        var viewModel = new UpdatePersonsViewModel(configProvider, dbAccessProvider, dialogs, personsRepository)
        {
            SelectedPerson = new PersonWithAge { Id = 1, ShortName = "Alice", FullName = "Alice Smith" },
        };

        viewModel.EditPersonCommand.Execute(null);

        A.CallTo(() => dialogs.ShowAddPersonDialogAsync(1, null)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void PersonSelectionCommand_SetsSelection()
    {
        A.CallTo(() => personsRepository.Persons).Returns([]);
        var viewModel = new UpdatePersonsViewModel(configProvider, dbAccessProvider, dialogs, personsRepository);
        var selected = new PersonWithAge { Id = 7, ShortName = "Seven", FullName = "Seven" };

        viewModel.PersonSelectionCommand.Execute(selected);

        Assert.Same(selected, viewModel.SelectedPerson);
    }
}
