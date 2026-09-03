using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Model;
using FileDB.ViewModels;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.ViewModels;

public class UpdateLocationsViewModelTests
{
    private readonly IConfigProvider configProvider = A.Fake<IConfigProvider>();
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IDatabaseAccess dbAccess = A.Fake<IDatabaseAccess>();
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly ILocationsRepository locationsRepository = A.Fake<ILocationsRepository>();
    private readonly IProcessUtils processUtils = A.Fake<IProcessUtils>();

    public UpdateLocationsViewModelTests()
    {
        A.CallTo(() => dbAccessProvider.DbAccess).Returns(dbAccess);
        A.CallTo(() => configProvider.Config).Returns(new ConfigBuilder { ReadOnly = true, LocationLink = "https://maps.example/?q={0},{1}" }.Build());
        A.CallTo(() => processUtils.IsOpenUriInBrowserSupported()).Returns(true);
    }

    [Fact]
    public void Constructor_LoadsLocations()
    {
        A.CallTo(() => locationsRepository.Locations).Returns([
            new LocationModel { Id = 1, Name = "Home" },
            new LocationModel { Id = 2, Name = "Work" },
        ]);

        var viewModel = new UpdateLocationsViewModel(configProvider, dbAccessProvider, dialogs, locationsRepository, processUtils);

        Assert.Equal(2, viewModel.Locations.Count);
        Assert.True(viewModel.ReadOnly);
    }

    [Fact]
    public void AddLocationCommand_UsesFilterText()
    {
        A.CallTo(() => locationsRepository.Locations).Returns([]);

        var viewModel = new UpdateLocationsViewModel(configProvider, dbAccessProvider, dialogs, locationsRepository, processUtils)
        {
            FilterText = "Home",
        };

        viewModel.AddLocationCommand.Execute(null);

        A.CallTo(() => dialogs.ShowAddLocationDialogAsync(null, "Home")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemoveLocationCommand_DeletesSelectedLocation()
    {
        A.CallTo(() => locationsRepository.Locations).Returns([
            new LocationModel { Id = 1, Name = "Home" },
        ]);
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(true);
        A.CallTo(() => dbAccess.SearchFilesWithLocations(A<IEnumerable<int>>._)).Returns([]);
        var recorder = new SingleEventRecorder<LocationEdited>();

        var viewModel = new UpdateLocationsViewModel(configProvider, dbAccessProvider, dialogs, locationsRepository, processUtils)
        {
            SelectedLocation = new LocationModel { Id = 1, Name = "Home" },
        };

        await viewModel.RemoveLocationCommand.ExecuteAsync(null);

        A.CallTo(() => dbAccess.DeleteLocation(1)).MustHaveHappenedOnceExactly();
        recorder.AssertEventRecorded();
    }

    [Fact]
    public void EditLocationCommand_UsesSelectedLocation()
    {
        A.CallTo(() => locationsRepository.Locations).Returns([
            new LocationModel { Id = 1, Name = "Home" },
        ]);

        var viewModel = new UpdateLocationsViewModel(configProvider, dbAccessProvider, dialogs, locationsRepository, processUtils)
        {
            SelectedLocation = new LocationModel { Id = 1, Name = "Home" },
        };

        viewModel.EditLocationCommand.Execute(null);

        A.CallTo(() => dialogs.ShowAddLocationDialogAsync(1, null)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void LocationSelectionCommand_SetsSelection()
    {
        A.CallTo(() => locationsRepository.Locations).Returns([]);
        var viewModel = new UpdateLocationsViewModel(configProvider, dbAccessProvider, dialogs, locationsRepository, processUtils);
        var selected = new LocationModel { Id = 7, Name = "Home" };

        viewModel.LocationSelectionCommand.Execute(selected);

        Assert.Same(selected, viewModel.SelectedLocation);
    }
}
