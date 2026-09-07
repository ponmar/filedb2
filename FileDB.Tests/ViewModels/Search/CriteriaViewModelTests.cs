using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Infrastructure;
using FileDB.Model;
using FileDB.ViewModels;
using FileDB.ViewModels.Search;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System.Linq;
using Xunit;

namespace FileDB.Tests.ViewModels.Search;

public class CriteriaViewModelTests : IDisposable
{
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly ISearchResultRepositoryManagement searchResultRepoManagement = A.Fake<ISearchResultRepositoryManagement>();
    private readonly IDatabaseAccess dbAccess = A.Fake<IDatabaseAccess>();

    public CriteriaViewModelTests()
    {
        ServiceLocator.Reset();

        var configProvider = A.Fake<IConfigProvider>();
        A.CallTo(() => configProvider.Config).Returns(new ConfigBuilder
        {
            InitialSearchFilterType = FilterType.AllFiles,
        }.Build());

        var searchResultRepository = A.Fake<ISearchResultRepository>();
        A.CallTo(() => searchResultRepository.Files).Returns([]);

        ServiceLocator.RegisterSingleton<IConfigProvider>(configProvider);
        ServiceLocator.RegisterSingleton<ISearchResultRepository>(searchResultRepository);
        ServiceLocator.RegisterTransient<FilterSelectionViewModel>();
        ServiceLocator.RegisterTransient<AllFilesViewModel>();
        ServiceLocator.RegisterTransient<FileListViewModel>();

        A.CallTo(() => dbAccessProvider.DbAccess).Returns(dbAccess);
    }

    public void Dispose() => ServiceLocator.Reset();

    [Fact]
    public void Constructor_AddsInitialFilter()
    {
        var viewModel = new CriteriaViewModel(dialogs, dbAccessProvider, searchResultRepoManagement);

        Assert.Single(viewModel.FilterSettings);
        Assert.False(viewModel.FilterCanBeRemoved);
    }

    [Fact]
    public async Task SearchForFilesAsync_PopulatesSearchResultRepository()
    {
        var files = new[] { new FileModel { Id = 1, Path = "a.jpg" } };
        A.CallTo(() => dbAccess.SearchFilesFromIds(A<IEnumerable<int>>._)).Returns(files);

        var viewModel = new CriteriaViewModel(dialogs, dbAccessProvider, searchResultRepoManagement);

        await viewModel.SearchForFilesAsync("1");

        A.CallTo(() => searchResultRepoManagement.PopulateRepo(A<IEnumerable<FileModel>>.That.Matches(x => x.Single().Id == 1))).MustHaveHappened();
        Assert.Single(viewModel.FilterSettings);
        Assert.IsType<FileListViewModel>(viewModel.FilterSettings.First().FilterViewModel);
    }

    [Fact]
    public void AddAndRemoveFilter_UpdatesFirstFilterAndRemovability()
    {
        var viewModel = new CriteriaViewModel(dialogs, dbAccessProvider, searchResultRepoManagement);

        viewModel.AddFilterCommand.Execute(null);
        Assert.Equal(2, viewModel.FilterSettings.Count);
        Assert.True(viewModel.FilterCanBeRemoved);
        Assert.True(viewModel.FilterSettings[0].IsFirstFilter);
        Assert.False(viewModel.FilterSettings[1].IsFirstFilter);

        viewModel.RemoveFilterCommand.Execute(viewModel.FilterSettings[0]);

        Assert.Single(viewModel.FilterSettings);
        Assert.True(viewModel.FilterSettings[0].IsFirstFilter);
        Assert.False(viewModel.FilterCanBeRemoved);
    }

    [Fact]
    public async Task FindFilesFromFiltersAsync_WithErrors_ShowsErrorAndDoesNotPopulate()
    {
        var viewModel = new CriteriaViewModel(dialogs, dbAccessProvider, searchResultRepoManagement)
        {
            HasFilterErrors = true,
        };

        await viewModel.FindFilesFromFiltersCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync("Invalid filter settings")).MustHaveHappenedOnceExactly();
        A.CallTo(() => searchResultRepoManagement.PopulateRepo(A<IEnumerable<FileModel>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task FindFilesFromFiltersAsync_AndCombinesResultsByFileId()
    {
        var first = new[] { new FileModel { Id = 1, Path = "a" }, new FileModel { Id = 2, Path = "b" } };
        var second = new[] { new FileModel { Id = 2, Path = "b" }, new FileModel { Id = 3, Path = "c" } };
        var firstFilter = viewModelFilter(first);
        var secondFilter = viewModelFilter(second);
        var viewModel = new CriteriaViewModel(dialogs, dbAccessProvider, searchResultRepoManagement);
        viewModel.FilterSettings[0].FilterViewModel = firstFilter;
        viewModel.AddFilterCommand.Execute(null);
        viewModel.FilterSettings[1].FilterViewModel = secondFilter;
        viewModel.FilterSettings[1].SelectedCombineMethod = CombineMethod.And;

        await viewModel.FindFilesFromFiltersCommand.ExecuteAsync(null);

        A.CallTo(() => searchResultRepoManagement.PopulateRepo(A<IEnumerable<FileModel>>.That.Matches(x => x.Select(f => f.Id).SequenceEqual(new[] { 2 })))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void FilterErrorsUpdated_ReevaluatesFilterErrors()
    {
        var viewModel = new CriteriaViewModel(dialogs, dbAccessProvider, searchResultRepoManagement);
        viewModel.FilterSettings[0].FilterViewModel = A.Fake<IFilterViewModel>();
        A.CallTo(() => viewModel.FilterSettings[0].FilterViewModel.HasErrors).Returns(true);

        Messenger.Send<FilterErrorsUpdated>();

        Assert.True(viewModel.HasFilterErrors);
    }

    private static IFilterViewModel viewModelFilter(IEnumerable<FileModel> files)
    {
        var filter = A.Fake<IFilterViewModel>();
        A.CallTo(() => filter.ApplyFilter(A<IDatabaseAccess>._)).Returns(files);
        return filter;
    }
}
