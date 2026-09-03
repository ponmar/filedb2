using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Lang;
using FileDB.Model;
using FileDB.ViewModels.Dialogs;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.ViewModels.Dialogs;

public class AddLocationViewModelTests
{
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IDatabaseAccess dbAccess = A.Fake<IDatabaseAccess>();
    private readonly IDialogs dialogs = A.Fake<IDialogs>();

    public AddLocationViewModelTests()
    {
        A.CallTo(() => dbAccessProvider.DbAccess).Returns(dbAccess);
    }

    [Fact]
    public void Constructor_NewLocation_UsesAddTitle()
    {
        var viewModel = new AddLocationViewModel(dbAccessProvider, dialogs);

        Assert.Equal(Strings.AddLocationAddTitle, viewModel.Title);
        Assert.Null(viewModel.AffectedLocation);
    }

    [Fact]
    public void Constructor_EditLocation_LoadsExistingValues()
    {
        A.CallTo(() => dbAccess.GetLocationById(7)).Returns(new LocationModel
        {
            Id = 7,
            Name = "Home",
            Description = "Family",
            Position = "1 2",
        });

        var viewModel = new AddLocationViewModel(dbAccessProvider, dialogs, 7);

        Assert.Equal(Strings.AddLocationEditTitle, viewModel.Title);
        Assert.Equal("Home", viewModel.Name);
        Assert.Equal("Family", viewModel.Description);
        Assert.Equal("1 2", viewModel.Position);
    }

    [Fact]
    public async Task SaveCommand_NewLocation_InsertsLocation()
    {
        A.CallTo(() => dbAccess.GetLocations()).Returns([]);
        A.CallTo(() => dbAccess.InsertLocation(A<LocationModel>._)).Returns(7);
        A.CallTo(() => dbAccess.GetLocationById(7)).Returns(new LocationModel { Id = 7, Name = "Home" });

        var viewModel = new AddLocationViewModel(dbAccessProvider, dialogs)
        {
            Name = "Home",
            Description = "Family",
            Position = "1 2",
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(7, viewModel.AffectedLocation!.Id);
        A.CallTo(() => dbAccess.InsertLocation(A<LocationModel>.That.Matches(x => x.Name == "Home"))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SaveCommand_EditLocation_UpdatesLocation()
    {
        A.CallTo(() => dbAccess.GetLocationById(7)).Returns(new LocationModel { Id = 7, Name = "Home" });

        var viewModel = new AddLocationViewModel(dbAccessProvider, dialogs, 7)
        {
            Name = "Home",
            Description = "Updated",
            Position = "1 2",
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        A.CallTo(() => dbAccess.UpdateLocation(A<LocationModel>.That.Matches(x => x.Id == 7 && x.Description == "Updated"))).MustHaveHappenedOnceExactly();
        Assert.NotNull(viewModel.AffectedLocation);
    }

    [Fact]
    public async Task SaveCommand_DuplicateLocation_ShowsError()
    {
        A.CallTo(() => dbAccess.GetLocations()).Returns([new LocationModel { Id = 1, Name = "Home" }]);

        var viewModel = new AddLocationViewModel(dbAccessProvider, dialogs)
        {
            Name = "Home",
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>.That.Contains("Home"))).MustHaveHappenedOnceExactly();
        A.CallTo(() => dbAccess.InsertLocation(A<LocationModel>._)).MustNotHaveHappened();
    }
}
